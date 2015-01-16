using System;
using System.Drawing;
using System.Net.Sockets;
using VNC.RFBProtocolHandling;
using System.Collections;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DrawingSupport;
using VNC.RFBDrawing.PixelDecoders;
// Zlib libary	
using NZlib.Compression;
using VNC.zlib;


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



namespace VNC.RFBDrawing.UpdateDecoders {

	/// <summary> the base class for all Decoders. The decoders decode
	/// an encoded bitmap data stream sent during a framebuffer-update
	/// </summary>
	public abstract class Decoder {
		
		/// <summary> constructs a decoder </summary>
		/// <remarks> before the Decoder is usable, the stream the data should be read from
		/// and the pixelDecoder which should be used to decode read pixel Data must be set. Additionally
		/// the surface and the PixelDecoder must be set.
		/// This is done using the initalize method.
		///	Normally this is done by the surface 
		/// </remarks>
		protected Decoder() {
		}

		/// <summary> initalizes this Decoder, only after calling this method the decoder is able to work </summary>
		public void initalize(RFBSurface surface, RFBNetworkStream stream, PixelDecoder pixelDecoder, DrawSupport drawSup) {
			this.pixelDecoder = pixelDecoder;
			this.stream = stream;
			this.surface = surface;
			this.drawSup = drawSup;
			inputBuffer = new byte[pixelDecoder.calculateStride(surface.BufferWidth) * surface.BufferHeight];
		}

		private PixelDecoder pixelDecoder;
		/// <summary> the pixelDecoder used to decode pixelValues </summary>		
		protected PixelDecoder PixelDecoder {
			get {
				return pixelDecoder;
			}
		}

		private RFBNetworkStream stream;
		/// <summary> the netwrokStream the data is read from </summary>
		protected RFBNetworkStream Stream {
			get {
				return stream;
			}
		}

		/// <summary> the width of the remote frame buffer </summary>
		protected int BufferWidth {
			get {
				return surface.BufferWidth;
			}
		}

		/// <summary> the width of the remote frame buffer </summary>
		protected int BufferHeight {
			get {
				return surface.BufferHeight;
			}
		}
		
		private RFBSurface surface;
		/// <summary> the surface this Decoder is created and used by </summary>
		protected RFBSurface Surface {
			get {
				return surface;
			}
		}
		private DrawSupport drawSup;
		/// <summary> the DrawSupport instance is used for drawing to the buffer/screen </summary>
		protected DrawSupport DrawingSupport {
			get {
				return drawSup;
			}
		}
		
		/// <summary> the inputBuffer usable to read in raw bitmap data </summary>
		protected byte[] inputBuffer;
		
		/// <summary> reads the raw-data into raw-inputbuffer, helper procedure for Decoders </summary>
		protected void decodeRawToByteArray(ReadByteStream stream, ushort width, ushort height) { 
			for (int j = 0; j < height; j++) {
				for (int i = 0; i < width; i++) {
					pixelDecoder.decodePixel(stream, inputBuffer, i, j, width);
				}
			}
		}

		/// <summary> decodes an update. </summary>
		/// <param name="xpos">the x-coordinate of the top-left corner of the update </param>
		/// <param name="ypos">the y-coordinate of the top-left corner of the update </param>
		/// <param name="width"> the width of the update</param>
		/// <param name="height"> the height of the update</param>
		/// <remarks> the precondition for calling this method is: the Decode must have been
		/// initalized using initalize </remarks>
		public abstract void decode(ushort xpos, ushort ypos, ushort width, ushort height);
		
		/// <summary> gets the encoding number this decoder belongs to </summary>
		public abstract uint getEncodingNr();
		
	}

	
	// ********************************************************************************************
	// Implementaion of the provided decoders (all RFB-Protocol defaults, plus additional ones)
	// ********************************************************************************************

	/// <summary> this class decodes an update in the RAW-Format </summary>		
	public class RawDecoder : Decoder {
		
		/// <seealso cref="Decoder.decode"/>
		public override void decode(ushort xpos, ushort ypos, ushort width, ushort height) {
			// read raw-data from Stream an update buffer
			// inefficient at the moment			
			decodeRawToByteArray(Stream, width, height);
			DrawingObject drawDest = DrawingSupport.getDrawingObject(xpos, ypos, width, height);
			drawDest.drawFromByteArray(inputBuffer, width, height, 0, 0, PixelDecoder);
			drawDest.updateDone();
		}
		
		/// <seealso cref="Decoder.getEncodingNr"/>
		public override uint getEncodingNr() {
			return 0; // raw-encoding in the RFBProtocol spec
		}
		
	}


	/// <summary> this class decodes an update in the Hextile-Format </summary>		
	public abstract class HextileDecoder : Decoder {
		
		/// <summary> raw subencoding </summary>
		protected const byte hextileRaw = 0x01;
		/// <summary> background spec subencoding </summary>
		protected const byte hextileBGSpec = 0x02;
		/// <summary> foreground spec subencoding </summary>
		protected const byte hextileFGSpec = 0x04;
		/// <summary> subrects subencoding </summary>
		protected const byte hextileSubRects = 0x08;
		/// <summary> colored subrects encoding </summary>
		protected const byte hextileSubRectColor = 0x10;
		/// <summary> new subencoding: hextileZlibRaw (not standard) </summary>
		protected const byte hextileZlibRaw = 0x20; // hextileRaw compressed with zlib
		
		/// <summary> tells the decoder, how to decode HextileZlibRawSubencoding if supported, only supported in ExtendenHextileDecoder </summary>
		protected virtual void decodeHextileZlibRaw(DrawingObject drawDest, ushort aktxpos, ushort aktypos, ushort tileWidth, ushort tileHeight) {
			// default implementation for standard hextile encoding, overridden in extended version
			throw new Exception("subencoding not supported in standard hextile");
		}
		
		/// <seealso cref="Decoder.decode"/>
		public override void decode(ushort xpos, ushort ypos, ushort width, ushort height) {
			DrawingObject drawDest = DrawingSupport.getDrawingObject(xpos, ypos, width, height);

			byte subenc; // subencoding
			ushort tileHeight, tileWidth;

			Color foreCol = new Color();
			Color backCol = new Color();
			Color aktForeCol = new Color();
			
			// decode tiles
			for (ushort aktypos = 0; (aktypos < height); aktypos += 16) {
					for (ushort aktxpos = 0; (aktxpos < width); aktxpos += 16) {
					
					tileHeight = 16;
					tileWidth = 16;
					if (width - aktxpos < 16) { tileWidth = (ushort)(width - aktxpos); }
					if (height - aktypos < 16) { tileHeight = (ushort)(height - aktypos); }  
					
					subenc = (byte)Stream.ReadByte();
					
					if ((subenc & hextileRaw) > 0) { // raw update
						decodeRawToByteArray(Stream, tileWidth, tileHeight);
						drawDest.drawFromByteArray(inputBuffer, tileWidth, tileHeight, aktxpos, aktypos, PixelDecoder);
						continue;
					} 
						
					if ((subenc & hextileBGSpec) > 0) {
						backCol = PixelDecoder.decodePixel(Stream);
						drawDest.drawFilledRectangle(backCol, aktxpos, aktypos, tileWidth, tileHeight);
					}
					if ((subenc & hextileFGSpec) > 0) {
						foreCol = PixelDecoder.decodePixel(Stream);	
					}
						
					if ((subenc & hextileSubRects) > 0) {
						// we have subrects:
						drawDest.drawFilledRectangle(backCol, aktxpos, aktypos, tileWidth, tileHeight);
						byte nofsubrects = (byte) Stream.ReadByte();
						aktForeCol = foreCol;
						for (int subr = 0; subr < nofsubrects; subr++) {
							if ((subenc & hextileSubRectColor) > 0) { aktForeCol = PixelDecoder.decodePixel(Stream); }
							byte xy = (byte)Stream.ReadByte();
							byte widthHeight = (byte)Stream.ReadByte();
							drawDest.drawFilledRectangle(aktForeCol,
											aktxpos + ((xy >> 4) & 0x0F),aktypos + (xy & 0x0F),
											1 + ((widthHeight >> 4) & 0x0F),1 + (widthHeight & 0x0F)
							);
						} // end for subrects
						continue;
					}

					if ((subenc & hextileZlibRaw) > 0) {
						// is this subencoding allowed (not for standard hextile)
						decodeHextileZlibRaw(drawDest, aktxpos, aktypos, tileWidth, tileHeight);
						continue;	
					}
						
					// nothing special specified --> whole tile: backcol
					drawDest.drawFilledRectangle(backCol, aktxpos, aktypos, tileWidth, tileHeight);
					
				} // end for x-tiles		
			} // end for y-tiles

			// inform draw-Destination of drawing has ended
			drawDest.updateDone();
		}
				
	}
	
	
	/// <summary> the hextileDecoder as described in the RFBProtocol V3.3 </summary>
	public class StandardHexitleDecoder : HextileDecoder {
			
		/// <seealso cref="Decoder.getEncodingNr"/>
		public override uint getEncodingNr() {
			return 5; // standard hextile-encoding in the RFBProtocol spec
		}

	}
	
	
	/// <summary> an extended version of the HextileDecoder, supporting a subencoding 0x20 for compressing a tile with zlib </summary>
	public class ExtendedHexitleDecoder : HextileDecoder {
		
		/// <summary> zlib decompression </summary>
		private InflateStream inflateStream;
		
		/// <summary> the standard constructor for an ExtendedHextileDecoder </summary>
		public ExtendedHexitleDecoder() {
			inflateStream = new InflateStream();
		}
		
		/// <seealso cref="HextileDecoder.decodeHextileZlibRaw"/>
		protected override void decodeHextileZlibRaw(DrawingObject drawDest, ushort aktxpos, ushort aktypos, ushort tileWidth, ushort tileHeight) {
			// zlib compressed raw data!
			ushort numOfCompressedBytes = Stream.ReadCard16();

			inflateStream.setRFBStream(Stream, numOfCompressedBytes);
			decodeRawToByteArray(inflateStream, tileWidth, tileHeight);
						
			drawDest.drawFromByteArray(inputBuffer, tileWidth, tileHeight, aktxpos, aktypos, PixelDecoder);
		}
		
		/// <seealso cref="Decoder.getEncodingNr"/>
		public override uint getEncodingNr() {
			return 9; // extended hextile-encoding, new subencoding 0x20 for compressing a tile with zlib
		}

	}	


	/// <summary> the base class for CoRRE and RRE Decoder </summary>
	public abstract class RREBaseDecoder : Decoder {
		
		/// <summary> the difference in the CoRRE and RRE Protocol is in the method to encode a rectangle </summary>
		protected abstract void drawRectangle(Color foreCol, DrawingObject drawDest);
		
		/// <seealso cref="Decoder.decode"/>
		public override void decode(ushort xpos, ushort ypos, ushort width, ushort height) {
			DrawingObject drawDest = DrawingSupport.getDrawingObject(xpos, ypos, width, height);
			
			uint nrOfSubrects = Stream.ReadCard32();
			Color backCol = PixelDecoder.decodePixel(Stream);
			drawDest.drawFilledRectangle(backCol, 0, 0, width, height);
				
			for (int i = 0; i < nrOfSubrects; i++) {
				Color foreCol = PixelDecoder.decodePixel(Stream);
				// decodes and draws rectangle for RRE/CoRRE
				drawRectangle(foreCol, drawDest);
			}
			
			drawDest.updateDone();
		}		
	
	}
	
	
	/// <summary> this class decodes an update in the RRE-Format </summary>		
	public class RREDecoder : RREBaseDecoder {

		/// <seealso cref="RREBaseDecoder.drawRectangle"/>
		protected override void drawRectangle(Color foreCol, DrawingObject drawDest) {
			drawDest.drawFilledRectangle(foreCol, Stream.ReadCard16(), Stream.ReadCard16(),
												Stream.ReadCard16(), Stream.ReadCard16()
			);			
		}		
		
		/// <seealso cref="Decoder.getEncodingNr"/>
		public override uint getEncodingNr() {
			return 2; // rre-encoding in the RFBProtocol spec
		}	
			
	}


	/// <summary> this class decodes an update in the CoRRE-Format </summary>
	public class CoRREDecoder : RREBaseDecoder {
		
		/// <seealso cref="RREBaseDecoder.drawRectangle"/>
		protected override void drawRectangle(Color foreCol, DrawingObject drawDest) {
			drawDest.drawFilledRectangle(foreCol, Stream.ReadByte(), Stream.ReadByte(),
												Stream.ReadByte(), Stream.ReadByte()
			);			
		}		
		
		/// <seealso cref="Decoder.getEncodingNr"/>
		public override uint getEncodingNr() {
			return 4; // CoRRE-encoding in the RFBProtocol spec
		}	
		
	}
	
	
	/// <summary> decodes an update in the copy-rect encoding </summary>	
	public class CopyRectDecoder : Decoder {

		/// <seealso cref="Decoder.decode"/>
		public override void decode(ushort xpos, ushort ypos, ushort width, ushort height) {
			ushort srcX = Stream.ReadCard16();
			ushort srcY = Stream.ReadCard16();
			
			OffScreenBuffer doublebuffer = DrawingSupport.copyFromBackBuffer(srcX, srcY, width, height);
			DrawingObject drawDest = DrawingSupport.getDrawingObject(xpos, ypos, width, height);
			drawDest.drawOffScreenBuffer(doublebuffer, 0, 0); // relativ to drawing object at coords (0,0)
			drawDest.updateDone();
			doublebuffer.Dispose();			
		}
		
		/// <seealso cref="Decoder.getEncodingNr"/>
		public override uint getEncodingNr() {
			return 1; // copyRect-encoding in the RFBProtocol spec
		}
		
	}


	/// <summary> decodes a zlibCompressed raw update </summary>	
	public class ZlibEncDecoder : Decoder {

		/// <summary> zlib decompression </summary>
		private InflateStream inflateStream;

		/// <summary> the standard constructor for a zlibEncDecoder </summary>
		public ZlibEncDecoder() {
			inflateStream = new InflateStream();
		}

		/// <seealso cref="Decoder.decode"/>
		public override void decode(ushort xpos, ushort ypos, ushort width, ushort height) {
			// get number of compressed bytes
			int numOfCompressedBytes = (int)Stream.ReadCard32();
			
			inflateStream.setRFBStream(Stream, numOfCompressedBytes);
			decodeRawToByteArray(inflateStream, width, height);
						
			DrawingObject drawDest = DrawingSupport.getDrawingObject(xpos, ypos, width, height);
			drawDest.drawFromByteArray(inputBuffer, width, height, 0, 0, PixelDecoder);
			drawDest.updateDone();
		}
		
		/// <seealso cref="Decoder.getEncodingNr"/>
		public override uint getEncodingNr() {
			return 6; // zlibEnc-encoding
		}
		
	}
	

}
