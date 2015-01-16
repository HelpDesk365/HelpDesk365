using System;
using System.Drawing;
using System.Net.Sockets;
using System.Reflection;
using VNC.RFBProtocolHandling;
using System.Collections;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DrawingSupport;
using VNC.RFBDrawing.UpdateDecoders;
using VNC.RFBDrawing.PixelDecoders;
using VNC.Config;
// Zlib libary	
using NZlib.Compression;
using VNC.zlib;


// author: Dominic Ullmann, dominic_ullmann@swissonline.ch
// Version: 1.03
	
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



namespace VNC.RFBDrawing {


	/// <summary>
	/// The RFBSurface class contains the data of the remote frame buffer and handles updates of the
	/// remote frame buffer
	/// </summary>
	public class RFBSurface { 

		/// <summary> reference to protocol-Handler </summary>
		private RFBProtocolHandler protocolHandler;		

		private int bufferWidth, bufferHeight;
		/// <summary> the width of the remote framebuffer </summary>
		public int BufferWidth {
			get { if (!connected) { throw new Exception("not connected"); }
				  return bufferWidth; }
		}
		/// <summary> the height of the remote framebuffer </summary>
		public int BufferHeight {
			get { if (!connected) { throw new Exception("not connected"); }
				  return bufferHeight; }
		}
		private byte depth;
		/// <summary> the depth selected for usage at client side. The depth is choosable by the client during connection handshake </summary>
		public byte Depth {
			get { if (!connected) { throw new Exception("not connected"); }
				  return depth; }
		}

		/// <summary> the vies connected to this surface </summary>
		private ArrayList views = new ArrayList();
		private System.Drawing.Imaging.PixelFormat format;
		/// <summary> decodes pixelvalues from stream </summary>
		private PixelDecoder pixDecod; 
		/// <summary> the drawSupport object: facility for drawing </summary>
		private DrawSupport drawSup;
		
		/// <summary> the decoders for decoding updates, O(1) access in normal case </summary>
		private Hashtable decoders = new Hashtable();
		/// <summary> the decoders sorted by the priority the client wishes to use them </summary>
		private ArrayList decoderPriorities = new ArrayList();
		
		private string serverName;
		/// <summary> the name of the server received during connection establishment </summary>
		public string ServerName {
			get {	if (!connected) { throw new Exception("not connected"); }
					return serverName; 	}	
		}
		
		/// <summary> the configuration information </summary>
		private VNCConfiguration config;
		
		// server, port: the information needed for connection to the VNC-Server
		private String server;
		private int port;
		
		private bool connected;
		/// <summary> is this surface has an established connection to a VNC-Server </summary>
		public bool Connected {
			get { return connected; }
		}
					
		/// <summary> constructor for an RFBSurface </summary>
		/// <param name="server">the server to connect to</param>
		/// <param name="port">the port to connect to</param>
		/// <param name="config">the configuration information read from the config-file</param>						
		public RFBSurface(String server, int port, VNCConfiguration config) {
			this.server = server;
			this.port = port;
			this.config = config;
			// create the decoders, after the handshake is complete, the decoders are initalized (before starting listening to regular messages)
			createDecoders();			
		}		

		// ****************************************************************************************************************
		// ************************************ methods for connecting and disconnecting the surface 
		// ****************************************************************************************************************

		/// <summary> Register views to notify
		/// Views displays the content of the RFBSurface
		/// </summary>
		internal void connectSurfaceToView(RFBView view) {
			Monitor.Enter(this);
			if (views.Contains(view)) { Monitor.Exit(this); throw new Exception("surface already connected to view"); }

			if (views.Count == 0) { // first view connected
				views.Add(view);				
				// connecting surface
				establishConnection(server, port);
				
				// creating draw-Support:
				if (config.DrawType == DrawTypes.GDIPlus) {
					// GDI+:
					drawSup = new DrawDotNetSupport(format, bufferWidth, bufferHeight, depth);	
				} else if (config.DrawType == DrawTypes.DirectDraw) {
					// DirectX:
					drawSup = new DirectXDrawSupport(format, bufferWidth, bufferHeight, depth, this);	
				} else {
					// this can only be the case, during development of new drawTypes
					Console.WriteLine("a drawType that is not known encountered in RFBSurface: " + config.DrawType);
					Monitor.Exit(this);
					throw new Exception("drawtype not ok: " + config.DrawType);
				}
			
				view.setRFBSize(bufferWidth, bufferHeight);
				// provide draw-Support Object to get access to drawing facilities
				view.setDrawSupport(drawSup);
				// initalize the decoders
				connectDecoders();

				// start listening for regular server-messages	
				protocolHandler.startMessageProcessing(); // now ready to receive noninitalization-messages from RFBServer!				
				// send update request to server, to get content of the RFB
				getFullUpdate();
			} else { // not the first view: enqueue only
				views.Add(view);
				view.setRFBSize(bufferWidth, bufferHeight);
				// provide draw-Support Object to get access to drawing facilities
				try {
					view.setDrawSupport(drawSup);				
				} catch (Exception e) {
					// Exception: removing the view from the view-List, leaving the monitor! and rethrow exception
					disconnectView(view);
					Monitor.Exit(this);
					throw e;
				}
			}
			
			Monitor.Exit(this);			
		}

		/// <summary> disconnect a view from the surface </summary>
		internal void disconnectView(RFBView view) {
			Monitor.Enter(this); /* synchronize with adding/notification */
			views.Remove(view);
			if (views.Count == 0) {
				// last view disconnected: closing connection
				closeConnection();
			}
			Monitor.Exit(this);
		}
		
		/// <summary> is called, when last view gets disconnected </summary>
		public void closeConnection() {
			protocolHandler.closeConnection();
			drawSup.Dispose();
		}

		/// <summary> connects this RFBSurface to the RFBServer,
		/// afterwards this surface stores pixeldata and informs the server of
		/// events
		/// </summary>
		private void establishConnection(string server, int port) {
			// creating Connection-Handler for Connection to the RFB-Server
			
			protocolHandler = new RFBProtocolHandler(server, port, this);
			ServerData data = protocolHandler.handshake();

			bufferWidth = data.fbWidth;
			bufferHeight = data.fbHeight;
			serverName = data.serverName;
			depth = data.pixForm.depth;

			// creating buffer and decoder
			// chooses the format I like and sending setFormat to Server
			// decoder: strategy-pattern for short decoding routines
			switch (depth) {
				case 8:  // bitsPerPixel = 8;
					 // 8bit not supported, use 16bit
					 depth = 16;
					 goto case 16;
				case 16: // bitsPerPixel = 16;
					 // inputBuffer = new byte[bufferWidth * bufferHeight * 2]; // creating reading buffer
					 format = System.Drawing.Imaging.PixelFormat.Format16bppRgb565;
					 pixDecod = new Pixel16bitDecoder(); 
					 Console.WriteLine("16 bit modus selected");
					 break;
				case 24: // bitsPerPixel = 32;
					 // inputBuffer = new byte[bufferWidth*bufferHeight * 3]; // creating reading buffer
					 format = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
					 pixDecod = new Pixel24bitDecoder();
 					 Console.WriteLine("24 bit modus selected");
					 break;
				case 32: // bitsPerPixel = 32;
					//  inputBuffer = new byte[bufferWidth*bufferHeight * 4]; // creating reading buffer
					 format = System.Drawing.Imaging.PixelFormat.Format32bppRgb;
					 pixDecod = new Pixel32bitDecoder();
 					 Console.WriteLine("32 bit modus selected");
					 break;
				default:
					// error
					throw new Exception("Server-Depth not supported!");
			}	
		
			// sending setPixelFormat-Message:
			// the format is defined by the choosen decoder
			protocolHandler.sendSetPixelFormat(pixDecod.getFormatDescription());
			// sends supported encodings
			protocolHandler.sendSetEncodings(decoderPriorities);
			// listening is started, when surface is connected to a View
			connected = true; // here the connection handshake is completed, connection to the server is ok
			Console.WriteLine("framebuffer size: " + bufferWidth + " " + bufferHeight);				
		}
		
		/// <summary> create the configured Decoders. </summary>
		/// <remarks> for extending the RFB-Protocol by an own decoder, add it to the decoder section
		/// in the VNCClient.Config.xml file.
		/// Use the fully qualified name of the decoder in the config-file.
		/// If the decoder is in a separate dll, append the name of the dll to the name after a comma,
		/// eg. VNC.RFBDrawing.UpdateDecoders.MyOwnDecoder,decoder for MyOwnDecoder in decoder.dll
		/// A decoder which should be usable here, must provide a no-argument constructor.
		/// </remarks>
		private void createDecoders() {	
			// use the decoders from the config
			int nrOfDecodersWorking = 0;
			foreach (String dec in config.Decoders) {
				// try to instantiate the decoder
				try {
					Type decoderType = Type.GetType(dec, true);
					Decoder decoder = (Decoder) decoderType.Assembly.CreateInstance(decoderType.FullName);
					registerDecoder(decoder);
					nrOfDecodersWorking++;
					// Console.WriteLine("created decoder " + dec + ": " + decoder);	
				} catch (Exception e) {
					Console.WriteLine("WARNING: error instantiating decoder " + dec + ": " + e);
				}
			}
			if (nrOfDecodersWorking == 0) { throw new Exception("no decoders could be instantiated!"); }
		}
		
		/// <summary> registers a Decoder in the order the client wishes to use it, highest priority decoder
		/// must be registered first
		/// </summary>
		private void registerDecoder(Decoder decoder) {
			// add in list, sorted by priority the client wishes to use decoder
			decoderPriorities.Add(decoder);
		}
		
		/// <summary> connect the decoders to the DrawSupport instance; after completion of this method, the decoders are usable </summary>
		private void connectDecoders() {
			foreach (Decoder decoder in decoderPriorities) {
				decoder.initalize(this, protocolHandler.Stream, pixDecod, drawSup);
				// add for accessing decoder
				decoders[decoder.getEncodingNr()] = decoder;
			}
		}
		
		// *******************************************************************************
		// ****************** methods used during regular rfb-protocol message exchange
		// *******************************************************************************
		
		/// <summary> gets a full update from the server </summary>
		public void getFullUpdate() {
			// send update request to server, to get content of the RFB
			protocolHandler.sendFBNonIncrementalUpdateRequest(0, 0, (ushort)bufferWidth, (ushort)bufferHeight);
		}
				
		/// <summary> notify interested views, changement discribed by x,y,width,height </summary>
		private void notifyView(int x, int y, int width, int height) {
			Monitor.Enter(this); /* synchronize with adding/removing of views */
   			IEnumerator enumerator = views.GetEnumerator();
   			while (enumerator.MoveNext()) {
	   			((RFBView)enumerator.Current).notifyUpdate(x,y,width,height);
	   		}
	   		Monitor.Exit(this);
		}
		
		/// <summary>
		/// decodes a received update with the encoding encoding
		/// </summary>
		public void decodeUpdate(uint encoding, ushort xpos, ushort ypos, ushort width, ushort height) {
			// get the decoder for this update
			Decoder dec = (Decoder)decoders[encoding];
			if (dec == null) {
				Console.WriteLine("encoding not supported: " + encoding + "; check the VNCClient.config.xml if a decoder should be present!");	
				throw new Exception("encoding not Supported: " + encoding);
			}
			// update is decoded and drawn by the Decoder
			dec.decode(xpos, ypos, width, height);
		}

		/// <summary>
		/// this method decides what to do on an update receiving
		/// </summary>
		public void gotRFBUpdate() {
			// sending network-events as early as possible to reduce delay
			
			// sending a new update request to stay informed of changes of the remote frame buffer
			// incremental updated are ok, because the buffer contains a probably old, but valid content
			protocolHandler.sendFBIncrementalUpdateRequest(0, 0, (ushort)bufferWidth, (ushort)bufferHeight);
		}
				
		/// <summary>
		/// this method decides how to proceed after a successful update of the framebuffer
		/// </summary>
		public void updateDone(int minX, int minY, int maxX, int maxY) {
			// draw the update to the screen!
			notifyView(minX, minY, maxX-minX, maxY-minY);
		}
		
		/// <summary>
		/// new cutbuffer content at the server
		/// </summary>
		public void serverCutText(RFBNetworkStream stream) {
			// the server has new date in its cutBuffer
			for (int i = 0; i < 3; i++) {
				stream.ReadByte();
			}
			uint length = stream.ReadCard32();
			String buf = stream.ReadString(length);
			// Insert data into Clipboard:
			Clipboard.SetDataObject(buf, true);
		}
		/// <summary>
		/// handling a beep received from server, dummy implementation at the moment
		/// (how to play sound using only safe code)
		/// </summary>		
		public void beep() {
			// handling a beep received from Server
			Console.WriteLine("beep");
		}
		
		/// <summary> reading a color-Map: unsopperted by this client </summary>
		public void setColorMapEntries(RFBNetworkStream stream) {
			ushort firstColor = stream.ReadCard16();
			ushort numberOfColors = stream.ReadCard16();
			for (int i = 0; i < numberOfColors; i++) {
				ushort red = stream.ReadCard16();
				ushort green = stream.ReadCard16();
				ushort blue = stream.ReadCard16();
			}
			Console.WriteLine("Ignoring setColormap-Entry because of Client-specified colors!");
		}

		// --------------
		// Event-Handling
		// --------------
		/// <summary> handles a pointer event, used by the connected views to inform the surface </summary>
		public void handlePointerEvent(byte buttonMask, ushort x, ushort y) {
			// for slow connections: it would be possible to drop some events, but this slows down faster
			// connections
			protocolHandler.sendPointerEvent(buttonMask, x, y);
		}
		
		/// <summary> handles a key event, used by the connected views to inform the surface </summary>
		public void handleKeyEvent(uint keySym, bool pressed) {
			protocolHandler.sendKeyEvent(keySym, pressed);
		}
		
		/// <summary> user wants to paste CutBuffer-Contents to server </summary>
		public void sendClientCutText() {
			IDataObject data = Clipboard.GetDataObject();
			if(data.GetDataPresent(DataFormats.Text)) {
		       	String text = (String)data.GetData(DataFormats.Text); 
		       	// sending to server cut buffer
		    	protocolHandler.setClientCutText(text);
    		} else {
    			Console.WriteLine("no Text data in Clipboard present!)");
    		}
		}
				
	}

}