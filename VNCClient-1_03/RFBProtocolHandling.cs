using System;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Drawing;
using VNC.RFBDrawing;
using VNC.RFBDrawing.UpdateDecoders;
using System.Threading;
using VNC.RFBProtocolHandling.Authenticate;
using System.IO;
using System.Collections;

// author: Dominic Ullmann, dominic_ullmann@swissonline.ch
// Version: 1.02

// VNC-Client for .NET
// Copyright (C) 2002  Dominic Ullmann

// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
	

namespace VNC.RFBProtocolHandling {

	/// <summary> 
	/// this class handles sending, receiving and dispatching of VNC-Messages
	/// </summary>
	public class RFBProtocolHandler {
	
		/// <summary> the connection used by the protocol-handler </summary>
		private RFBNetworkStream stream;
		
		internal RFBNetworkStream Stream {
			get { return stream; }
		}
		
		private RFBTcpClient client;
		private string server;
		private int port;
		/// <summary> the surface preserving the pixeldata at the client </summary>
		private RFBSurface surface;
		/// <summary> The Thread handing incoming regular protocol messages </summary>
		private Thread handler = null; 

		/// <summary> creates a protocol handler, which handels a vnc-connection
		/// </summary>
		public RFBProtocolHandler(string server, int port, RFBSurface surface) {
			this.server = server;
			this.surface = surface;
			this.port = port;
		}

		/// <summary> handshake with the vnc-server </summary>
		public ServerData handshake() {
			
			try {
				client = new RFBTcpClient();
				client.Connect(server, port); // connect to port on server
				client.NoDelay = true; // sending without delay!
				stream = client.GetRFBStream();
				// connection established!
				// getting the protocol-Message from the Server
				uint serverMajor = 0; uint serverMinor = 0;
				receiveProtocolMessage(out serverMajor, out serverMinor);
				Console.WriteLine("serverMajor : " + serverMajor + " serverMinor : " + serverMinor);
							
				if (serverMajor > 3) { 
					throw new Exception("server Major too big!");
				}

				// sending my protocol-Message to server
				sendProtocolMessage(3,3); // version 3.3
				
				// do authentification
				authenticate(); // throws exception, if failed

				// send client initalization
				sendClientInitialization();

				// server initalization
				ServerData data = receiveServerInitialization();
				// handshake ok!
				return data;
				
			} catch (Exception e ) {
            	try { client.Close(); } catch (SocketException) {} // close connection if open!
				Console.WriteLine(e.ToString());
				throw new Exception("handshake failed");
		    }
		}

		/// <summary> closing connection to the vnc-server </summary>
		public void closeConnection() {
			// shutting down handler
			handler.Abort();
			// closing connection
			client.Close();	
			// should not be need, but Thread-Abort doesn't work always
			Environment.Exit(0);	
		}

		/// <summary> starting listener thread, this thread handles incoming
		/// messages after handshake is completed
		/// </summary>
		public void startMessageProcessing() {
			ThreadStart action = new ThreadStart(processIncomingMessages);
			handler = new Thread(action);
			handler.Start();
		}
		
		/// <summary>
		/// Listener thread action: handles incoming normal VNC-Messages (after handshake)
		/// </summary>
		private void processIncomingMessages() {
			// decodes Incoming Protocol-Messages and distribute them to the approriate receiver
			// read the message number
			
			try {
			while (true) {
			
				byte msgNr = (byte)stream.ReadByte();

				// distribute message to appropriate receiver
				switch (msgNr) {
					case 0 :
						// Framebuffer-Update
						// inform surface, that there is a new rfbupdate --> surface can send messages to server on this event
						surface.gotRFBUpdate();
						
						int minX = -1;
						int minY = -1;
						int maxX = -1;
						int maxY = -1;
						
						stream.ReadByte(); // padding
						ushort rectCount = stream.ReadCard16();
						for (int i = 0; i < rectCount; i++) {
							ushort xpos = stream.ReadCard16();
							ushort ypos = stream.ReadCard16();
							ushort width = stream.ReadCard16();
							ushort height = stream.ReadCard16();
							
							if ((minX == -1) || (xpos < minX)) { minX = xpos; }
							if ((minY == -1) || (ypos < minY)) { minY = ypos; }				
							if ((xpos + width) > maxX) { maxX = xpos + width; }
							if ((ypos + height) > maxY) { maxY = ypos + height; }
							
							uint encoding = stream.ReadCard32();
							surface.decodeUpdate(encoding, xpos, ypos, width, height);							
						} // end for

						surface.updateDone(minX, minY, maxX, maxY); // inform surface of completed updateprocessing
							
						break;
					case 1 :
						// SetColorMapEntries
						// not really supported
						stream.ReadByte(); // padding
						surface.setColorMapEntries(stream);
						break;
					case 2 :
						// bell
						surface.beep();
						break;
					case 3 :
						surface.serverCutText(stream);
						break;
					default :
						Console.WriteLine("Error in processIncomingMessages");
						throw new Exception("message unknown"); 
				}

			}
			} catch (ThreadAbortException) { /* Console.WriteLine("ThreadAbort"); */ }
			  catch (System.IO.IOException ioEx) { 
			  		if (!(ioEx.InnerException is ThreadAbortException)) {
			  			Console.WriteLine(ioEx); 
			  			// unexpected: end application
		  				Environment.Exit(1);
			  		} // else a ThreadAbort-Exception was thrown to stop this thread on application-Closing!
			  }
			  catch (Exception e) {
				Console.WriteLine(e);
				Environment.Exit(1);
				// unexpected: end application
			  }
	
		}
	
		// ----------------------
		// Handling RFB Handshake
		// ----------------------
		
		/// <summary>
		/// this methods sends the protocol message to the server and starts
		/// the handshake with the server with it
		/// </summary>
		private void sendProtocolMessage(uint major, uint minor) {
			byte[] arr = new byte[12];
			arr[0] = 0x52;
			arr[1] = 0x46;
			arr[2] = 0x42;
			arr[3] = 0x20;
			arr[7] = 0x2e;
			arr[11] = 0x0a;

			// major
			arr[4] = (byte) (((major >> 16) & 0xFF) + 0x30);
			arr[5] = (byte) (((major >> 8) & 0xFF) + 0x30);
			arr[6] = (byte) ((major & 0xFF) + 0x30);

			// minor: least sig byte first in int-type
			arr[8] = (byte) (((minor >> 16) & 0xFF) + 0x30);
			arr[9] = (byte) (((minor >> 8) & 0xFF) + 0x30);
			arr[10] = (byte) ((minor & 0xFF) + 0x30);
	
			stream.Write(arr, 0, 12);
			stream.Flush();
		}

		/// <summary>
		/// receive the Protocol Message from the Server
		/// </summary>
		private void receiveProtocolMessage(out uint major, out uint minor) {
		
			byte[] arr = new byte[12];
			stream.ReadBlocking(arr, 0, 12);

			// check
			if ((arr[0] != 0x52) || (arr[1] != 0x46) || (arr[2] != 0x42) ||
			    (arr[3] != 0x20) ||	(arr[7] != 0x2e) || (arr[11] != 0x0a)) {
				throw new InvalidRFBDataException();
			}
			// convert Version-Numbers
			major = (uint) ( (arr[4] - 0x30) * 100 + (arr[5] - 0x30) * 10 + (arr[6] - 0x30) );
			minor = (uint) ( (arr[8] - 0x30) * 100 + (arr[9] - 0x30) * 10 + (arr[10] - 0x30) );
		}

		/// <summary> do authentication </summary>
		private void authenticate() {
			uint authMethod = stream.ReadCard32();
			switch (authMethod) {
				case 0:
					uint reasonLength = stream.ReadCard32();
					string reason = stream.ReadString(reasonLength);
					throw new Exception("Connection Failed : " + reason);
				case 1:
					break;
				case 2:
					byte[] challenge = new byte[16];
					stream.ReadBlocking(challenge, 0, 16);
					
					// get password, encrypt challenge
					string password = "";
					AuthenticationForm authForm = new AuthenticationForm();
					if (authForm.ShowDialog() == DialogResult.OK) {
						password = authForm.getPassword();
						authForm.Dispose();
					} else { 
						authForm.Dispose();
						throw new Exception("authentication failed");
					}
					
					byte[] response = DESAuthenticatior.encryptChallenge(challenge, password);
					// send response
					stream.Write(response, 0, response.Length);
					stream.Flush();

					// get authentication result
					uint authRes = stream.ReadCard32();
					if (authRes != 0) { throw new Exception("authentication failed:" + authRes); }
					break;
				default:
					throw new Exception("Connection Failed");
			}
		}

		/// <summary> getting serverInitalization </summary>
		private ServerData receiveServerInitialization() {
			ServerData data = new ServerData();
			data.fbWidth = stream.ReadCard16();
			data.fbHeight = stream.ReadCard16();
			data.pixForm = stream.ReadPixelFormat();
			uint serverNameLength = stream.ReadCard32();
			data.serverName = stream.ReadString(serverNameLength);
			return data;
		}
		/// <summary> sends the client initialization </summary>
		private void sendClientInitialization() {
				stream.WriteByte(0); // don't share connection
				stream.Flush();
		} 

		// Messages after Initialization

		/// <summary> send setPixelFormat-message </summary>
		public void sendSetPixelFormat(PixelFormat pixFormat) {
			Monitor.Enter(stream);
		
			stream.WriteByte(0); // mgs-type
			for (int i = 0; i < 3; i++) { stream.WriteByte(0); } // padding
			stream.WritePixelFormat(pixFormat);
			stream.Flush();
			
			Monitor.Exit(stream);
		}
		
		 /// <summary> send all supported formats </summary>
		public void sendSetEncodings(ArrayList decoders) {
			Monitor.Enter(stream);
			
			stream.WriteByte(2); // mgs-type
			stream.WriteByte(0); // padding
			stream.WriteCard16((ushort)decoders.Count); // nof-encodings
			foreach (Decoder dec in decoders) {
				stream.WriteCard32(dec.getEncodingNr());
			}
			stream.Flush();
			
			Monitor.Exit(stream);
		}

		/// <summary> send framebuffer-Update message: 
		/// implicitly tells server, that the last sent update-messages was processed at the client
		/// </summary>
		public void sendFBIncrementalUpdateRequest(ushort x, ushort y, ushort width, ushort height) {
			// incremental update
			sendFBUpdateRequest(x, y, width, height, true);
		}
		
		/// <summary> send a full update request </summary>
		public void sendFBNonIncrementalUpdateRequest(ushort x, ushort y, ushort width, ushort height) {
			sendFBUpdateRequest(x, y, width, height, false);
		}
		
		/// <summary> send update request, implicitly tells server, that the last sent update-messages was processed at the client </summary>
		private void sendFBUpdateRequest(ushort x, ushort y, ushort width, ushort height, bool incremental) {
			Monitor.Enter(stream);
			
			stream.WriteByte(3); // mgs-type
			if (incremental) { stream.WriteByte(1); } else { stream.WriteByte(0); }
			stream.WriteCard16(x);
			stream.WriteCard16(y);
			stream.WriteCard16(width);
			stream.WriteCard16(height);
			stream.Flush();
					
			Monitor.Exit(stream);
		}
		
		/// <summary> inform the server of a pointer event </summary>
		public void sendPointerEvent(byte buttonMask, ushort x, ushort y) {
			Monitor.Enter(stream);
			
			stream.WriteByte(5); // msg-type
			stream.WriteByte(buttonMask);
			stream.WriteCard16(x);
			stream.WriteCard16(y);
			stream.Flush();
			
			Monitor.Exit(stream);
		}
		
		/// <summary> inform the server of a key event </summary>
		public void sendKeyEvent(uint keySym, bool pressed) {
			Monitor.Enter(stream);

			stream.WriteByte(4); // msg-type
			if (pressed) { stream.WriteByte(1); } else { stream.WriteByte(0); }
			stream.WriteCard16(0); // padding
			stream.WriteCard32(keySym);
			stream.Flush();
			
			Monitor.Exit(stream);
		}
		
		/// <summary> inform the server: client has new text in its cutbuffer </summary>
		public void setClientCutText(string text) {
			Monitor.Enter(stream);
			
			stream.WriteByte(6);
			for (int i = 0; i < 3; i++) { stream.WriteByte(0); } // padding
			stream.WriteCard32((uint)text.Length);
			stream.WriteString(text);
			stream.Flush();

	       	// pasting text at current server insert position:
			System.Text.ASCIIEncoding encode = new System.Text.ASCIIEncoding();
			
			byte[] textAsBytes = encode.GetBytes(text);
			for (int i = 0; i < textAsBytes.Length; i++) {
				sendKeyEvent(textAsBytes[i],true);
				sendKeyEvent(textAsBytes[i],false);
			}
			
			Monitor.Exit(stream);
		}
		
		/// <summary> Fixes the colormap at the server
		/// not really supported
		/// </summary>
		public void sendColorMap(System.Drawing.Imaging.ColorPalette pal) {
				// not supported by servers: ignore
				Monitor.Enter(stream);
				stream.WriteByte(1); // msg-type
				stream.WriteByte(0); // padding
				stream.WriteCard16(1); // first colormap-Entry
				
				Color[] entries = pal.Entries;
				stream.WriteCard16((ushort)entries.Length); // number of colormap-Entries
				for (int i = 0; i < entries.Length; i++) {
					// in the color-map: 8bit pro Color-Component --> calculate 16bit color components!
					stream.WriteCard16((ushort)(entries[i].R << 8));
					stream.WriteCard16((ushort)(entries[i].G << 8));
					stream.WriteCard16((ushort)(entries[i].B << 8));
				}
				stream.Flush();
				Monitor.Exit(stream);
		}

	}

	// *****************************************************************
	// The following two Classes handles the connection to an RFBServer.
	// *****************************************************************

	/// <summary> this class represents a TCP-Client.
	/// </summary>
	public class RFBTcpClient : TcpClient {
		
		/// <summary> no argument constructor </summary>
		public RFBTcpClient() : base(){
		}

		/// <summary> get an RFBStream for this connection </summary>
		public RFBNetworkStream GetRFBStream() {
			return new RFBNetworkStream(this);
		}
		
	}

	/// <summary>
	/// interface specifing the functionality of a stream, from which byte could be read.
	///	</summary>
	/// <remarks>
	/// This interface is used for the PixelDecoders, because they should support decoding pixels from all streams,
	/// which allow reading bytes, e.g. RFBNetworkStream or InflateStream (stream for reading zlib compressed data)
	/// </remarks>
	public interface ReadByteStream {
		/// <summary> reading a single byte </summary>
		int ReadByte();
	}

	/// <summary>
	/// this class represents a Network stream, knowing the VNC-Datatypes
	/// </summary>
	public class RFBNetworkStream : ReadByteStream {
		
		private byte[] rfbWriteBuffer = new byte[4]; // for efficient writeop's
		private BufferedStream writeBufferedStream;
		private BufferedStream readBufferedStream;

		/// <summary> constructs an RFBStream for the connection represented by the TCPClient client </summary>
		/// <param name="client">the connection</param>	
		public RFBNetworkStream(TcpClient client) {
			NetworkStream stream = client.GetStream();
			writeBufferedStream = new BufferedStream(stream); // using a bufferedstream for writing
			readBufferedStream = new BufferedStream(stream, 20000); // using a bufferedstream for reading, using large buffer!
		}

		/// <summary> blocking readByte: returns exactly one byte </summary>
		public int ReadByte() {
			return readBufferedStream.ReadByte();			
		}			
		
		/// <summary> blocking reading of multiple bytes </summary>
		public int ReadBlocking(byte[] buffer, int offset, int size) {
			
 			for (int i = 0; i < size; i++) {
				buffer[offset+i] = (byte) ReadByte();
			} 

			return size;
		}
		
		/// <summary> reading without blocking, returns after at most size bytes have been read </summary>
		public int Read(byte[] buffer, int offset, int size) {
			// non blocking Read of multiple bytes
			return readBufferedStream.Read(buffer, offset, size);
		}
		
		// ----------------------------
		// Reading primitive Data types
		// ----------------------------
		/// <summary> read a card16 from stream </summary>
		public ushort ReadCard16() {
			return (ushort)((ReadByte() << 8) + ReadByte());
		}
		/// <summary> read a card32 from stream </summary>
		public uint ReadCard32() {
			return (uint)((ReadByte() << 24) + (ReadByte() << 16) +
				(ReadByte() << 8) + ReadByte());			
		}
		/// <summary> read a pixelformat from stream </summary>
		public PixelFormat ReadPixelFormat() {
			PixelFormat pixForm = new PixelFormat();
			pixForm.bitsPerPixel = (byte)ReadByte();
			pixForm.depth = (byte)ReadByte();
			pixForm.bigEndian = (byte)ReadByte();
			pixForm.trueColor = (byte)ReadByte();
			pixForm.redMax = ReadCard16();
			pixForm.greenMax = ReadCard16();
			pixForm.blueMax = ReadCard16();
			pixForm.redShift = (byte)ReadByte();
			pixForm.greenShift = (byte)ReadByte();
			pixForm.blueShift = (byte)ReadByte();			
			for (int i = 0; i<3; i++) { ReadByte(); } // padding
			return pixForm;
		}
		/// <summary> read a string with the given length from stream </summary>
		public string ReadString(uint length) {
			byte[] msg = new byte[length];
			ReadBlocking(msg,0,msg.Length);
			return System.Text.Encoding.ASCII.GetString(msg);			
		}

		/// <summary>
		/// flushing the stream: RFBStream uses a buffer to enhance network throughoutput
		/// this method flushes the buffer
		/// </summary>
		public void Flush() {
			writeBufferedStream.Flush();
		}

		// ----------------------------
		// writing primitive data types
		// ----------------------------
		/// <summary> write a byte to the stream </summary>
		public void WriteByte(byte value) {
			rfbWriteBuffer[0] = value;
			writeBufferedStream.Write(rfbWriteBuffer, 0, 1);			
		}
		/// <summary> write a number of bytes from a byte array to the stream </summary>
		public void Write(byte[] buffer, int offset, int size) {
			// writing to bufferedStream
			writeBufferedStream.Write(buffer, offset, size);
		}
		/// <summary> write a card16 to the stream </summary>
		public void WriteCard16(ushort value) {
			rfbWriteBuffer[0] = (byte) ((value >> 8) & 0xFF);
			rfbWriteBuffer[1] = (byte) (value & 0xFF);
			Write(rfbWriteBuffer, 0, 2);
		}
		/// <summary> write a card32 to the stream </summary>
		public void WriteCard32(uint value) {
			rfbWriteBuffer[0] = (byte) ((value >> 24) & 0xFF);
			rfbWriteBuffer[1] = (byte) ((value >> 16) & 0xFF);
			rfbWriteBuffer[2] = (byte) ((value >> 8) & 0xFF);
			rfbWriteBuffer[3] = (byte) (value & 0xFF);
			Write(rfbWriteBuffer, 0, 4);
		}
		/// <summary> wirte a string to the stream </summary>		
		public void WriteString(string text) {
			Write(System.Text.Encoding.ASCII.GetBytes(text), 0, text.Length);
		}
		/// <summary> write a pixelformat to the stream </summary>
		public void WritePixelFormat(PixelFormat format) {
			WriteByte(format.bitsPerPixel);
			WriteByte(format.depth);
			WriteByte(format.bigEndian);
			WriteByte(format.trueColor);
			WriteCard16(format.redMax);
			WriteCard16(format.greenMax);
			WriteCard16(format.blueMax);
			WriteByte(format.redShift);
			WriteByte(format.greenShift);
			WriteByte(format.blueShift);
			for (int i = 0; i<3; i++) { WriteByte(0); } // padding
		}
		
	}

	/// <summary> Exception class for invalid data</summary>
	public class InvalidRFBDataException : System.ApplicationException {
		/// <summary> error string </summary>
		public override String ToString() {
			return "invalid input data";
		}
	}

	// **********************************************************************************************
	// Structs for exchanging Data with RFBSurface
	// **********************************************************************************************
	/// <summary> this struct represents the data of a remote framebuffer </summary>
	public struct ServerData {
		/// <summary> the width of the framebuffer </summary>
		public ushort fbWidth;
		/// <summary> the height of the framebuffer </summary>
		public ushort fbHeight;
		/// <summary> the pixelformat of the framebuffer </summary>
		public PixelFormat pixForm;
		/// <summary> the name of the server </summary>
		public string serverName;
	}

	/// <summary> this struct represents a vnc-pixelformat </summary>
	public struct PixelFormat {
		/// <summary> the bits used for a pixel in the stream </summary>
		public byte bitsPerPixel;
		/// <summary> the depth of this pixelformat e.g. 32bit</summary>
		public byte depth;
		/// <summary> is pixeldata encoded in bigendian format </summary>
		public byte bigEndian;
		/// <summary> is this pixelformat a truecolor format or only a palette format </summary>
		public byte trueColor;
		/// <summary> the maximal value the red component can reach </summary>
		public ushort redMax;
		/// <summary> the maximal value the green component can reach </summary>
		public ushort greenMax;
		/// <summary> the maximal value the blue component can reach </summary>
		public ushort blueMax;
		/// <summary> the bit shift needed to accesss the red component </summary>
		public byte redShift;
		/// <summary> the bit shift needed to accesss the greeen component </summary>
		public byte greenShift;
		/// <summary> the bit shift needed to accesss the blue component </summary>		
		public byte blueShift;
	}


}