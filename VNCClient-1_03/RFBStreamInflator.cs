using System;
using NZlib.Compression;

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


namespace VNC.zlib {

	/// <summary> this Stream is responsible to decompress the zlib compressed input from the server </summary>
	public class InflateStream : ReadByteStream {

		/// <summary> the connection to the server </summary>
		private RFBNetworkStream stream;
		/// <summary> the decompressor </summary>
		private Inflater inflator;
		private const int bufferSize = 4096;
		private byte[] inputBuffer;
		private byte[] outputBuffer;
		private int remaining = 0;
		private int outputOffset = 0;
		private int nofBytesInOutputBuffer = 0;
		private int nofBytesInInputBuffer = 0;

		/// <summary> the constructor for an InflateStream </summary>
		/// <param name="stream">the underlaying stream</param>
		/// <param name="nofCompressedBytes">the number of compressed bytes in the underlaying stream</param>
		public InflateStream(RFBNetworkStream stream, int nofCompressedBytes) {
			init();
			setRFBStream(stream, nofCompressedBytes);
		}
		
		/// <summary>
		/// a no argument constructor for an Inflate stream, use later setRFBStream to
		/// set the underlaying stream
		/// </summary>
		public InflateStream() {
			init();
		}
		
		private void init() {
			inflator = new Inflater();
			inputBuffer = new byte[bufferSize];
			outputBuffer = new byte[bufferSize * 4];
		}
		/// <summary> set a compressed connection for decompressing a specified
		/// number of bytes from it
		/// </summary>
		public void setRFBStream(RFBNetworkStream stream, int nofCompressedBytes) {
			this.stream = stream;
			if (remaining != 0 || inflator.IsFinished) { inflator.Reset(); }
			remaining = nofCompressedBytes;
			nofBytesInInputBuffer = 0;
			nofBytesInOutputBuffer = 0;
			outputOffset = 0;
		}

		/// <summary>
		/// with this method a client of this Stream can read a byte out of the stream
		/// </summary>
		public int ReadByte() {
		    // Console.WriteLine("reading byte from compressed stream");
			if (outputOffset >= nofBytesInOutputBuffer) {
				outputOffset = 0;
				nofBytesInOutputBuffer = 0;
				fillOutputBuffer();
			}
			if (nofBytesInOutputBuffer == 0) { throw new Exception("end of stream"); }
			int result = outputBuffer[outputOffset];
			outputOffset++;
			return result;
		}
		
		/// <summary> decompress some input </summary>
		private void fillOutputBuffer() {
			// Console.WriteLine("filling outputBuffer");
			if (inflator.IsNeedingInput) { fillInputBuffer(); }
			nofBytesInOutputBuffer = inflator.Inflate(outputBuffer,0,outputBuffer.Length);
			// Console.WriteLine("bytes in outputBuffer: " + nofBytesInOutputBuffer);
		}
		
		/// <summary> set the input buffer of the decompressor </summary>
		private void fillInputBuffer() {
			int toRead = remaining;
			if (remaining > bufferSize) {
				toRead = bufferSize;
			}
			// Console.WriteLine("toRead: " + toRead);
			stream.ReadBlocking(inputBuffer,0,toRead);
			inflator.SetInput(inputBuffer,0,toRead);
			remaining -= toRead;
			nofBytesInInputBuffer = toRead;
			// Console.WriteLine("inputBuffer filled: " + nofBytesInInputBuffer);
		}
		
	}


}