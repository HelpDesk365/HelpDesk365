using System;
using System.Drawing;
using VNC.RFBProtocolHandling;


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



// This namespace contain Strategy-Classes to decode Pixels from Stream. They make
// the UpdateDecoders independent of the used PixelFormat (Strategy-Pattern)
namespace VNC.RFBDrawing.PixelDecoders {
	

	/// <summary> The PixelDecoders decode pixeldata read from a stream </summary>
	/// <remarks> 
	/// subclasses of this class make the update decoders independant of the used Pixelformat
	/// (strategy pattern)
	/// </remarks>
	public abstract class PixelDecoder {
		/// <summary> decodes a pixel value from a stream </summary>
		public abstract Color decodePixel(ReadByteStream stream);
		/// <summary> decodes a pixel from the Stream stream and stores it in the byte[] buf at the position for pixel PixX, pixY </summary>
		public abstract void decodePixel(ReadByteStream stream, byte[] buf, int pixX, int pixY, int width); 
		/// <summary> returns the stride it uses for encoding to a buffer with bitmap width width </summary>
		public abstract int calculateStride(int width);
		/// <summary> returns the format decoded to in decodePixel into buffer </summary>
		public abstract System.Drawing.Imaging.PixelFormat getTargetPixelFormat();
		/// <summary> the expected stream Format for using decoder </summary>
		public abstract PixelFormat getFormatDescription();
	}
	
	/// <summary>
	/// decodes 32bit pixel values
	/// </summary>
	public class Pixel32bitDecoder : PixelDecoder {

		/// <see cref = "PixelDecoder.decodePixel"/>
		public override Color decodePixel(ReadByteStream stream) {
			byte blue = (byte)stream.ReadByte();
			byte green = (byte)stream.ReadByte();
			byte red = (byte)stream.ReadByte();
			stream.ReadByte();
			Color color = Color.FromArgb(red, green, blue);
			return color;
		}

		/// <see cref = "PixelDecoder.decodePixel"/>
		public override void decodePixel(ReadByteStream stream, byte[] buf, int pixX, int pixY, int width) {
			int stride = calculateStride(width);
		    buf[(pixY * stride) + (pixX * 4) + 0] = (byte)stream.ReadByte();
			buf[(pixY * stride) + (pixX * 4) + 1] = (byte)stream.ReadByte();
			buf[(pixY * stride) + (pixX * 4) + 2] = (byte)stream.ReadByte();
			buf[(pixY * stride) + (pixX * 4) + 3] = (byte)stream.ReadByte();
		}

		/// <see cref = "PixelDecoder.getFormatDescription"/>
		public override PixelFormat getFormatDescription() {
			PixelFormat formatToSet = new PixelFormat();
			formatToSet.bigEndian = 0; // little endian, format of client machine!		
			formatToSet.depth = 32; 
			formatToSet.bitsPerPixel = 32;
			formatToSet.trueColor = 1;
			formatToSet.redMax = 255;
			formatToSet.greenMax = 255;
			formatToSet.blueMax = 255;
			formatToSet.redShift = 16; 
			formatToSet.greenShift = 8;
			formatToSet.blueShift = 0;
			return formatToSet;
		}

		/// <see cref = "PixelDecoder.calculateStride"/>
		public override int calculateStride(int width) {
			return 4 * width;
		}

		/// <see cref = "PixelDecoder.getTargetPixelFormat"/>		
		public override System.Drawing.Imaging.PixelFormat getTargetPixelFormat() {
			return System.Drawing.Imaging.PixelFormat.Format32bppRgb;
		}
		
	}
	
	/// <summary> a decoder with a 24 bit format, 24 bit used per pixel </summary>
	public class Pixel24bitDecoder : PixelDecoder {

		/// <see cref = "PixelDecoder.decodePixel"/>
		public override Color decodePixel(ReadByteStream stream) {
			Color color = Color.FromArgb(stream.ReadByte(), stream.ReadByte(), stream.ReadByte());
			return color;
		}
		
		/// <see cref = "PixelDecoder.decodePixel"/>		
		public override void decodePixel(ReadByteStream stream, byte[] buf, int pixX, int pixY, int width) {
			int stride = calculateStride(width);
			buf[(pixY * stride) + (pixX * 3) + 0] = (byte)stream.ReadByte();
			buf[(pixY * stride) + (pixX * 3) + 1] = (byte)stream.ReadByte();
			buf[(pixY * stride) + (pixX * 3) + 2] = (byte)stream.ReadByte();
		}
		
		/// <see cref = "PixelDecoder.getFormatDescription"/>
		public override PixelFormat getFormatDescription() {
			PixelFormat formatToSet = new PixelFormat();
			formatToSet.bigEndian = 0; // little endian, format of client machine!		
			formatToSet.depth = 24; 
			formatToSet.bitsPerPixel = 32;
			formatToSet.trueColor = 1;
			formatToSet.redMax = 255;
			formatToSet.greenMax = 255;
			formatToSet.blueMax = 255;
			formatToSet.redShift = 16; 
			formatToSet.greenShift = 8;
			formatToSet.blueShift = 0;
			return formatToSet;		
		}
		
		/// <see cref = "PixelDecoder.calculateStride"/>		
		public override int calculateStride(int width) {
			int stride = width * 3;
			if (stride % 4 != 0) {
				stride = stride - (stride % 4) + 4;
			}
			return stride;
		}
		
		/// <see cref = "PixelDecoder.getTargetPixelFormat"/>
		public override System.Drawing.Imaging.PixelFormat getTargetPixelFormat() {
			return System.Drawing.Imaging.PixelFormat.Format24bppRgb;
		}

	}

	/// <summary> a decoder with a 16 bit format, 16 bit used per pixel </summary>
	/// <remarks> The 16 bits of this pixelformat are dived into:
	/// red: 5 most significant bits, green 6 bits in the middle, blue 5 least significant bits
	/// </remarks>
	public class Pixel16bitDecoder : PixelDecoder {
	
		/// <see cref = "PixelDecoder.decodePixel"/>
		public override Color decodePixel(ReadByteStream stream) {
			
			ushort pix = (ushort)(stream.ReadByte() + (stream.ReadByte() << 8)); // little endian
				
			// red: 5 most significant bit, target an 8 bit red value --> shift red value 8 bits right
			// green: 6 bits in the middle, target an 8 bit green value --> shift green value 2 bits right
			// blue: 5 least significant bits, target an 8 bit blue value --> shift blue value 3 bits left
			
			return Color.FromArgb((pix & 0xF800) >> 8, (pix & 0x07E0) >> 3, (pix & 0x001F) << 3);
		}

		/// <see cref = "PixelDecoder.decodePixel"/>		
		public override void decodePixel(ReadByteStream stream, byte[] buf, int pixX, int pixY, int width) {
			int stride = calculateStride(width);
			ushort pix = (ushort)(stream.ReadByte() + (stream.ReadByte() << 8)); // little endian
			int r = (pix & 0xF800) >> 8;
			int g = (pix & 0x07E0) >> 3;
			int b = (pix & 0x001F) << 3;
			
		    buf[(pixY * stride) + (pixX * 4) + 0] = (byte)b;
			buf[(pixY * stride) + (pixX * 4) + 1] = (byte)g;
			buf[(pixY * stride) + (pixX * 4) + 2] = (byte)r;
			buf[(pixY * stride) + (pixX * 4) + 3] = 0;
		}

		/// <see cref = "PixelDecoder.getFormatDescription"/>		
		public override PixelFormat getFormatDescription() {
			PixelFormat formatToSet = new PixelFormat();
			formatToSet.bigEndian = 0; // little endian, format of client machine!		
			formatToSet.depth = 16;
			formatToSet.bitsPerPixel = 16;
			formatToSet.trueColor = 1;
			formatToSet.redMax = 31;
			formatToSet.greenMax = 63;
			formatToSet.blueMax = 31;
			formatToSet.redShift = 11; 
			formatToSet.greenShift = 5;
			formatToSet.blueShift = 0;
			return formatToSet; 
		}
		
		/// <see cref = "PixelDecoder.calculateStride"/>		
		public override int calculateStride(int width) {
			// because of a bug in the .NET Bitmap class with 16bit pixel formats, decode to 32bit format instead
			return 4 * width;
		}
		
		/// <see cref = "PixelDecoder.getTargetPixelFormat"/>
		public override System.Drawing.Imaging.PixelFormat getTargetPixelFormat() {
			// because of a bug in the .NET Bitmap class with 16bit pixel formats, decode to 32bit format instead			
			return System.Drawing.Imaging.PixelFormat.Format32bppRgb;
		}

	}
	
		
}
