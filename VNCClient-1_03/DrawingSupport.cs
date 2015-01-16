using System.Windows.Forms;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using VNC.RFBDrawing;
using VNC.RFBDrawing.PixelDecoders;
using System.Collections;
using DirectXLIB;

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
	


// this namespace contains classes that support drawing. This classes make the code
// in the RFB-Surface independent of the used Graphics libary
namespace DrawingSupport {


	// ***************************************************************************************/	
	// ************************************** Abstract Classes *******************************/
	// ***************************************************************************************/	
	
	/// <summary>
	///	the DrawSupport class is the main class used by the RFB-Surface to access the 
	/// drawing functionality
	/// </summary>
	public abstract class DrawSupport {
		/// <summary> the pixelformat of the remote framebuffer</summary>
		protected PixelFormat format;
		/// <summary> the width of the remote framebuffer</summary>
		protected int width;
		/// <summary> the height of the remote framebuffer</summary>
		protected int height;
		/// <summary> the depth of the remote framebuffer</summary>
		protected int depth;
		/// <summary> the views using this drawsupport instance </summary>
		protected Hashtable views = new Hashtable();
		/// <summary> is dispose already called </summary>
		protected bool disposed = false;
		
		/// <summary> register a view, which this Drawsupport instance draws to. In this step the view is prepared for drawing to it</summary>
		public abstract void registerView(RFBView view);
		
		/// <summary> initalizies the Drawsupport </summary>
		protected abstract void initalize(PixelFormat format, int width, int height, int depth);
		/// <summary> draws the specified region of backbuffer to screen </summary>
		public abstract void drawBackBufferToScreen(RFBView screen, Rectangle region);
		/// <summary> draws the whole backbuffer to screen </summary>
		public abstract void drawBackBufferToScreen(RFBView screen);
		/// <summary> get Drawing object for updating the backbuffer </summary>
		/// <param name="x">the x coordinate of the drawing region</param>
		/// <param name="y">the y coordinate of the drawing region</param>
		/// <param name="width">the width of the drawing region</param>
		/// <param name="height">the height of the drawing region</param>			
		public abstract DrawingObject getDrawingObject(int x, int y, int width, int height);
		/// <summary> get an offscreenbuffer with the given dimension </summary>
		public abstract OffScreenBuffer getOffScreenBuffer(int width, int height);
		/// <summary> creates an offscreenbuffer with a part of the backbuffer as contents  </summary>		
		public abstract OffScreenBuffer copyFromBackBuffer(int srcX, int srcY, int width, int height);
		
		/// <summary> called by a drawing-Object to inform drawsupport of a performed update.
		/// Should only be called by a drawing-Object created by the getDrawingObject method
		/// </summary>
		protected internal abstract void drawingObjectDone(int x, int y, int width, int height);
		
		/// <summary> for freeing resources, call if drawing support object is no longer needed,
		/// (is otherwise called during finalization)
		/// </summary>
		public abstract void Dispose();
		/// <summary> finalizer </summary>
		~DrawSupport() {
			if (!disposed) { Dispose(); }
		}
		
	}
	
	
	/// <summary>
	/// provides functionality to draw to a buffer / to the screen.
	/// </summary>
	/// <remarks>	
	/// This class is a general Implementation of DrawingObject, usable with many DrawingSupport classes 	
	/// </remarks>	
	public class DrawingObject {
		private int width;
		private int height;
		private int x;
		private int y;
		private OffScreenBuffer backBuffer;
		private DrawSupport draw;

		/// <summary> the width of the region this drawing object represents </summary>
		public int Width { get { return width; } }
		/// <summary> the height of the region this drawing object represents </summary>		
		public int Height { get  { return height; } }
		/// <summary> the x-coordinate of the top-left corner of the region this drawing object represents </summary>		
		public int X { get { return x; } }
		/// <summary> the y-coordinate of the top-left corner of the region this drawing object represents </summary>		
		public int Y { get  { return y; } }
		
		/// <summary>
		/// creates a drawing object, for drawing to a specified region
		/// </summary>
		/// <remarks>
		/// this constructor should only be used from methods/constructors within a drawing-Support class
		/// </remarks>
		/// <param name="x">the x-coordinate of the top-left corner of the drawing region</param>
		/// <param name="y">the y-coordinate of the top-left corner of the drawing region</param>
		/// <param name="width">the width of the drawing region</param>
		/// <param name="height">the height of the drawing region</param>						
		/// <param name="buffer">the backbuffer used during drawing</param>
		/// <param name="drawSup">the drawSupport instance used for drawing</param>
		/// <seealso cref="DrawSupport"/>
		public DrawingObject(int x, int y, int width, int height, OffScreenBuffer buffer, DrawSupport drawSup) {
			this.x = x;
			this.y = y;
			this.width = width;
			this.height = height;
			this.backBuffer = buffer;
			this.draw = drawSup;
		}
		/// <summary> this methods draws a filled rectangle in the specified color and with
		/// the specified size at the specified position 
		/// </summary>
		public virtual void drawFilledRectangle(Color color, int x, int y, int width, int height) {
			backBuffer.drawFilledRectangle(color, this.x + x,this.y + y, width, height);
		}
		
		/// <summary> draw from the contents of an offscreen buffer </summary>
		public virtual void drawOffScreenBuffer(OffScreenBuffer src, int destX, int destY) {
			backBuffer.drawOffScreenBuffer(src, this.x + destX, this.y + destY);
		}
		
		/// <summary> this methods draws data from a byte array. </summary>
		/// <param name="data">the byte array containting the pixel data</param>
		/// <param name="width">the width of the pixeldata in pixel</param>
		/// <param name="height">the height of the pixeldata in pixel</param>
		/// <param name="destX">x-coordinate of the top-left corner of the region, where the pixeldata should be placed to</param>
		/// <param name="destY">y-coordinate of the top-left corner of the region, where the pixeldata should be placed to</param>
		/// <param name="usedDecoder">the usedDecoder parameter specifies the decoder used to produce data in the byte array, must not be null</param>
		public virtual void drawFromByteArray(byte[] data, int width, int height, int destX, int destY, PixelDecoder usedDecoder) {
			backBuffer.drawFromByteArray(data, width, height, this.x + destX, this.y + destY, usedDecoder);
		}
		
		/// <summary> calling this to tell update is done, forcing the changes made with this
		/// drawing object to appear </summary>
		public virtual void updateDone() {
			draw.drawingObjectDone(x, y, width, height);
		}
		
	}

	
	/// <summary>
	/// a buffer for buffering bitmap-data
	/// </summary>
	public interface OffScreenBuffer {
		/// <summary> the widht of the OffscreenBuffer </summary>
		int Width { get; }
		/// <summary> the height of the OffscreenBuffer </summary>		
		int Height { get; }
		
		/// <summary> draw a filled rectangle to the OffscreenBuffer </summary>
		void drawFilledRectangle(Color color, int x, int y, int width, int height);
		/// <summary> draw the contents of another Offscreenbuffer to the OffscreenBuffer </summary>
		void drawOffScreenBuffer(OffScreenBuffer src, int destX, int destY); 
		/// <summary> draw the contents of another Offscreenbuffer to the OffscreenBuffer </summary>
		void drawOffScreenBuffer(OffScreenBuffer src, int destX, int destY, int width, int height, int srcX, int srcY);
		/// <summary> draw text to the OffScreenBuffer </summary>
		void drawText(string text, int x, int y, Font font, Color color);
		/// <summary> draw the data from a byte array to the OffScreenBuffer </summary>
		/// <param name="data">the byte array containting the pixel data</param>
		/// <param name="width">the width of the pixeldata in pixel</param>
		/// <param name="height">the height of the pixeldata in pixel</param>
		/// <param name="destX">x-coordinate of the top-left corner of the region, where the pixeldata should be placed to</param>
		/// <param name="destY">y-coordinate of the top-left corner of the region, where the pixeldata should be placed to</param>
		/// <param name="usedDecoder">the usedDecoder parameter specifies the decoder used to produce data, must not be null</param>
		void drawFromByteArray(byte[] data, int width, int height, int destX, int destY, PixelDecoder usedDecoder);
		/// <summary> disposes resources allocated by offscreenbuffer </summary>
		void Dispose();
	}

	// ***************************************************************************************/	
	// ************************************** DirectX-Implementation *************************/
	// ***************************************************************************************/	

	/// <summary>
	/// this class provides an implmentation of Drawsupport using DirectX
	/// </summary>
	public class DirectXDrawSupport : DrawSupport {
		
		private DirectDraw ddraw;
		private DirectXOffScreenBuffer backBuffer = null;
		private DirectDrawSurface primarySurface = null;
		private RFBSurface surface;
		
		/// <summary> constructor for DirectXDrawSupport </summary>
		/// <param name="surface">the RFBSurface using this drawsupport instance for drawing, needed to reget the pixel-data if content DirectDraw surface is lost</param>
		/// <param name="format">the pixelformat of the remote framebuffer</param>
		/// <param name="width">the width of the remote framebuffer</param>
		/// <param name="height">the height of the remote framebuffer</param>
		/// <param name="depth"></param>			
		public DirectXDrawSupport(PixelFormat format, int width, int height, int depth, RFBSurface surface) {
			initalize(format,width,height,depth);
			
			// initalize direct Draw
			DirectX dx = new DirectX();
			ddraw = dx.createDirectDraw7();	
			this.surface = surface;
		}

		private void createBackBuffer() {
			// must be created after setCooperativeLevel is called
			// backbuffer:
			STRUCT_DDSURFACEDESC2 backBufferDescription = new STRUCT_DDSURFACEDESC2();
			backBufferDescription.flags = CONST_DDSURFACEDESCFLAGS.DdSD_CAPS | CONST_DDSURFACEDESCFLAGS.DdSD_WIDTH | CONST_DDSURFACEDESCFLAGS.DdSD_HEIGHT;
			backBufferDescription.ddsCaps = CONST_DDSCAPSFLAGS.DdSCAPS_SYSTEMMEMORY;
			// backBufferDescription.ddsCaps = CONST_DDSCAPSFLAGS.DdSCAPS_OFFSCREENPLAIN;
			backBufferDescription.dwWidth = width;
			backBufferDescription.dwHeight = height;
			DirectDrawSurface bBuffer = ddraw.createSurface(ref backBufferDescription);				

			backBuffer = new DirectXOffScreenBuffer(bBuffer, width, height, format, depth);
			// clear backbuffer
			backBuffer.drawFilledRectangle(Color.FromArgb(0,0,0), 0, 0, width,height);
		}
		
		private void createPrimarySurface() {
			STRUCT_DDSURFACEDESC2 primarySurfaceDescription = new STRUCT_DDSURFACEDESC2();
			primarySurfaceDescription.flags = CONST_DDSURFACEDESCFLAGS.DdSD_CAPS;
			primarySurfaceDescription.ddsCaps = CONST_DDSCAPSFLAGS.DdSCAPS_PRIMARYSURFACE;
			primarySurface = ddraw.createSurface(ref primarySurfaceDescription);
		}
		
		/// <see name="DrawSupport.registerView"/> 
		public override void registerView(RFBView view) {
			// creating graphics-Object for view:
			if (views.Count > 0) { throw new Exception("more than one view not possible with direct draw"); }
			if (!views.ContainsKey(view)) {
				
				// initalizing for the view
				ddraw.setCooperativeLevel(view.Handle.ToInt32(), CONST_DDSCLFLAGS.DdSCL_NORMAL);
				
				if (backBuffer == null) { // first view: creating backBuffer
					createBackBuffer();
				}
				if (primarySurface == null) { // first view: creating primary Surface
					createPrimarySurface();
				}
			
				// prepare view
			
				// attaching clipper:
			    DirectDrawClipper clipper = ddraw.createClipper(0);
				clipper.setHWnd(view.Handle.ToInt32());
				primarySurface.setClipper(clipper);
				
				views.Add(view,primarySurface);	
			}
		}
		
		/// <see name="DrawSupport.initalize"/> 
		protected override void initalize(PixelFormat format, int width, int height, int depth) {
			this.format = format;
			this.width = width;
			this.height = height;
			this.depth = depth;
		}	

		/// <see name="DrawSupport.drawBackBufferToScreen"/> 
		public override void drawBackBufferToScreen(RFBView screen) {
			drawBackBufferToScreen(screen, new Rectangle(0, 0, width, height));
		}
		
		/// <see name="DrawSupport.drawBackBufferToScreen"/> 
		public override void drawBackBufferToScreen(RFBView screen, Rectangle region) {
			Monitor.Enter(this);

			if (!views.ContainsKey(screen)) { throw new Exception("registerview not called! --> unable to draw"); }
			
			DirectDrawSurface primarySurface = (DirectDrawSurface)views[screen];

			// parameter region defines the changed part of the RFBSurface
			// regionOnScreen specifies the visible Region of the RFBSurface
			
			Rectangle regionOnScreen = screen.ShowedRectangle;
			Rectangle intersection = Rectangle.Intersect(region, regionOnScreen);
			if ((intersection.Width <= 0) || (intersection.Height <= 0)) { Monitor.Exit(this); return; } // nothing visible updated
			// the intersection definies the visible part of the region in RFBSurface-coordinates			
			// identify destination coordinates in screen coordinates 
			
			// the destination
			Rectangle regionDest = new Rectangle();
			Point screenCoord = screen.PointToScreen(new Point(0, 0));
			// intersection.X - regionOnScreen.X lies in the visible area
			regionDest.X = screenCoord.X + (intersection.X - regionOnScreen.X);
			// intersection.Y - regionOnScreen.Y lies in the visible area
			regionDest.Y = screenCoord.Y + (intersection.Y - regionOnScreen.Y);
			regionDest.Width = intersection.Width;
			regionDest.Height = intersection.Height;			
			
			// if surfaces are lost: restore them
			if (primarySurface.isLost()) { primarySurface.restore(); }
			if (backBuffer.getSurface().isLost()) { 
				backBuffer.getSurface().restore();
				// here i need a full update:
				surface.getFullUpdate();
				Monitor.Exit(this);
				return;
			}
			
			// first param: dest, third param source:
			try {
			primarySurface.blt(regionDest, backBuffer.getSurface(), intersection, CONST_DDBLTFLAGS.DdBLT_WAIT);
			} catch (COMException) { } // ignoring Exception due to surface losts ...
			Monitor.Exit(this);
		}

		/// <see name="DrawSupport.getOffScreenBuffer"/> 		
		public override OffScreenBuffer getOffScreenBuffer(int width, int height) {
			return new DirectXOffScreenBuffer(width,height,format,ddraw,depth);
		}

		/// <see name="DrawSupport.copyFromBackBuffer"/> 				
		public override OffScreenBuffer copyFromBackBuffer(int srcX, int srcY, int width, int height) {
			Monitor.Enter(this);
			OffScreenBuffer dst = getOffScreenBuffer(width,height);
			dst.drawOffScreenBuffer(backBuffer, 0, 0, width, height, srcX, srcY);
			Monitor.Exit(this);
			return dst;
		}
		
		/// <see name="DrawSupport.getDrawingObject"/> 
		public override DrawingObject getDrawingObject(int x, int y, int width, int height) {
			return new DrawingObject(x, y, width, height, backBuffer, this);
		}
		
		/// <see name="DrawSupport.drawingObjectDone"/> 
		protected internal override void drawingObjectDone(int x, int y, int width, int height) {
			// it gets painted to screen, when a screen-update is done with drawBackBufferToScreen ...
		}		
		
		/// <see name="DrawSupport.Dispose"/> 
		public override void Dispose() {
			// waiting for last drawing operation to complete
			Monitor.Enter(this);
			backBuffer.Dispose();
			disposed = true;
			Monitor.Exit(this);
			
		}
	
	}
	
	
	/// <summary>
	/// this class provides an implementation of OffScreenBuffer using DirectX 
	/// </summary>
	public class DirectXOffScreenBuffer : OffScreenBuffer {
		
		private int width;
		private int height;
		private PixelFormat format;
		private DirectDrawSurface buffer;
		private bool disposed = false;
		private int depth;
		private SolidBrush brush;
			
		/// <summary> constructor for DirectXOffScreenBuffer, uses an existing DirectDraw surface to store it's data </summary>
		/// <param name="width">the width of the buffer</param>	
		/// <param name="height">the height of the buffer</param>
		/// <param name="format">the pixelformat for the buffer</param>				
		/// <param name="surface">where the data of the offscreenbuffer is located</param>	
		/// <param name="depth"></param>
		public DirectXOffScreenBuffer(DirectDrawSurface surface, int width, int height, PixelFormat format, int depth) {
			
			// create a buffer from a bufferSurface
			setFields(width,height,format,depth);
			buffer = surface;
			brush = new SolidBrush(new Color());
		}

		/// <summary> constructor for DirectXOffScreenBuffer, creates a DirectDraw surface to store it's data</summary>
		/// <param name="width">the width of the buffer</param>	
		/// <param name="height">the height of the buffer</param>
		/// <param name="format">the pixelformat for the buffer</param>				
		/// <param name="ddraw">the direct-draw instance</param>	
		/// <param name="depth"></param>				
		public DirectXOffScreenBuffer(int width, int height, PixelFormat format, DirectDraw ddraw, int depth) {
			setFields(width,height,format,depth);
			// creating an Systemmem-Surface for the buffer:
			STRUCT_DDSURFACEDESC2 bufferDescription = new STRUCT_DDSURFACEDESC2();
			bufferDescription.flags = CONST_DDSURFACEDESCFLAGS.DdSD_CAPS | CONST_DDSURFACEDESCFLAGS.DdSD_WIDTH | CONST_DDSURFACEDESCFLAGS.DdSD_HEIGHT;
			bufferDescription.ddsCaps = CONST_DDSCAPSFLAGS.DdSCAPS_SYSTEMMEMORY;
			bufferDescription.dwWidth = width;
			bufferDescription.dwHeight = height;
			buffer = ddraw.createSurface(ref bufferDescription);				
		}
		
		private void setFields(int width, int height, PixelFormat format, int depth) {
			this.width = width;
			this.height = height;
			this.format = format;
			this.depth = depth;
		}

		/// <see name="OffScreenBuffer.Width"/> 
		public int Width { get { return width; } }
		/// <see name="OffScreenBuffer.Height"/> 		
		public int Height { get  { return height; } }
		
		internal DirectDrawSurface getSurface() { return buffer; }

		/// <see name="OffScreenBuffer.drawFilledRectangle"/> 		
		public void drawFilledRectangle(Color color, int x, int y, int width, int height) {
			// efficient DirectDraw variant to draw a rectangle			
			STRUCT_DDBLTFX ddbltfx = new STRUCT_DDBLTFX();
			ddbltfx.dwFillColor = color.ToArgb();
			Rectangle dest = new Rectangle(x,y,width,height);
			buffer.blt(dest, null, dest, CONST_DDBLTFLAGS.DdBLT_COLORFILL, ref ddbltfx);
		}

		/// <see name="OffScreenBuffer.drawOffScreenBuffer"/> 
		public void drawOffScreenBuffer(OffScreenBuffer src, int destX, int destY) {
			drawOffScreenBuffer(src, destX, destY, src.Width, src.Height, 0, 0);
		}
		
		/// <see name="OffScreenBuffer.drawOffScreenBuffer"/> 
		public void drawOffScreenBuffer(OffScreenBuffer src, int destX, int destY, int width, int height, int srcX, int srcY) {
			Rectangle region = new Rectangle();
			region.X = srcX;
			region.Y = srcY;
			region.Width = width;
			region.Height = height;
			
			buffer.bltFast(destX, destY, ((DirectXOffScreenBuffer)src).getSurface(), region, CONST_DDBLTFASTFLAGS.DdBLTFAST_WAIT);
		}
		
		/// <see name="OffScreenBuffer.drawFromByteArray"/> 
		public void drawFromByteArray(byte[] data, int width, int height, int destX, int destY, PixelDecoder usedDecoder) {
			
			GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			IntPtr srcAdr = Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);
			Bitmap bitmap = new Bitmap(width, height, usedDecoder.calculateStride(width), usedDecoder.getTargetPixelFormat(), srcAdr);
			dataHandle.Free();

			Monitor.Enter(this); // DC needs exclusive access to Buffer
			// copying bitmap to surface:
			int bufferDC = buffer.getDC();
			Graphics g = Graphics.FromHdc(new IntPtr(bufferDC));
			g.DrawImage(bitmap, destX, destY);
			g.Dispose();
			buffer.releaseDC(bufferDC);
			Monitor.Exit(this);

			bitmap.Dispose();
		}

		/// <see name="OffScreenBuffer.drawText"/> 		
		public void drawText(string text, int x, int y, Font font, Color color) {
			brush.Color = color;
			
			Monitor.Enter(this); // DC needs exclusive access to Buffer
			// copying bitmap to surface:
			int bufferDC = buffer.getDC();
			Graphics g = Graphics.FromHdc(new IntPtr(bufferDC));
			g.DrawString(text, font, brush, x, y);
			
			g.Dispose();
			buffer.releaseDC(bufferDC);
			Monitor.Exit(this);
		}
		
		/// <see name="OffScreenBuffer.Dispose"/> 
		public void Dispose() {
			buffer.Dispose();
			disposed = true;
		}
		
		/// <summary> finalizer </summary>
		~DirectXOffScreenBuffer() {
			if (!disposed) { Dispose(); }
		}
		
	}
	

	// ***************************************************************************************/	
	// ************************************** Standard GDI+-Implementation *******************/
	// ***************************************************************************************/	
		
	/// <summary>
	/// an implmentation of Drawsupport using the classes provided by the dotNet-Framework
	/// </summary>
	public class DrawDotNetSupport : DrawSupport {
		
		private DotNetOffScreenBuffer buffer;
		private DotNetOffScreenBuffer doubleBuffer;
		private Bitmap backBufferBitmap;
		
		/// <summary> constructor for DrawDotNetSupport </summary>
		/// <param name="format">the pixelformat of the remote framebuffer</param>
		/// <param name="width">the width of the remote framebuffer</param>
		/// <param name="height">the height of the remote framebuffer</param>
		/// <param name="depth"></param>
		public DrawDotNetSupport(PixelFormat format, int width, int height, int depth) {
			initalize(format, width, height, depth);			
		}
		
		/// <see name="DrawSupport.registerView"/> 
		public override void registerView(RFBView view) {
			// creating graphics-Object for view:
			if (!views.ContainsKey(view)) {
				Graphics viewGraphics = view.CreateGraphics();
				views.Add(view, viewGraphics);
			}
		}
		
		/// <see name="DrawSupport.initalize"/>
		protected override void initalize(PixelFormat format, int width, int height, int depth) {
			this.format = format;
			this.width = width;
			this.height = height;
			this.depth = depth;
			
			buffer = (DotNetOffScreenBuffer)getOffScreenBuffer(width,height);
			backBufferBitmap = buffer.getBitmap();
			doubleBuffer = (DotNetOffScreenBuffer)getOffScreenBuffer(width, height);
		}	

		/// <see name="DrawSupport.drawBackBufferToScreen"/>
		public override void drawBackBufferToScreen(RFBView screen) {
			drawBackBufferToScreen(screen, new Rectangle(0, 0, width, height));
		}

		/// <see name="DrawSupport.drawBackBufferToScreen"/>
		public override void drawBackBufferToScreen(RFBView screen, Rectangle region) {
			Monitor.Enter(buffer);
				
			if (!views.ContainsKey(screen)) { throw new Exception("registerview not called! --> unable to draw"); }
			
			// parameter region defines the changed part of the RFBSurface
			// regionOnScreen specifies the visible Region of the RFBSurface
			
			Rectangle regionOnScreen = screen.ShowedRectangle;
			Rectangle intersection = Rectangle.Intersect(region, regionOnScreen);
			if ((intersection.Width <= 0) || (intersection.Height <= 0)) { Monitor.Exit(buffer); return; } // nothing visible updated
			// the intersection definies the visible part of the region in RFBSurface-coordinates			
			// identify destination coordinates in screen coordinates 
			
			// the destination
			Rectangle regionDest = new Rectangle();
			// intersection.X - regionOnScreen.X lies in the visible area
			regionDest.X = (intersection.X - regionOnScreen.X);
			// intersection.Y - regionOnScreen.Y lies in the visible area
			regionDest.Y = (intersection.Y - regionOnScreen.Y);
			regionDest.Width = intersection.Width;
			regionDest.Height = intersection.Height;			
			
			// create Graphics object
			Graphics screenGraphics = screen.CreateGraphics();
			screenGraphics.DrawImage(backBufferBitmap, regionDest, intersection, GraphicsUnit.Pixel);
			screenGraphics.Dispose();

			Monitor.Exit(buffer);
		}

		/// <see name="DrawSupport.getOffScreenBuffer"/>		
		public override OffScreenBuffer getOffScreenBuffer(int width, int height) {
			return new DotNetOffScreenBuffer(width, height, format, depth);
		}
				
		/// <see name="DrawSupport.copyFromBackBuffer"/>
		public override OffScreenBuffer copyFromBackBuffer(int srcX, int srcY, int width, int height) {
			Monitor.Enter(buffer);
			OffScreenBuffer dst = getOffScreenBuffer(width,height);
			dst.drawOffScreenBuffer(buffer, 0, 0, width, height, srcX, srcY);
			Monitor.Exit(buffer);
			return dst;
		}
		
		/// <see name="DrawSupport.getDrawingObject"/>		
		public override DrawingObject getDrawingObject(int x, int y, int width, int height) {
			return new DrawingObject(x, y, width, height, doubleBuffer, this);
		}
		
		/// <see name="DrawSupport.drawingObjectDone"/>
		protected internal override void drawingObjectDone(int x, int y, int width, int height) {
			Monitor.Enter(buffer);
			buffer.drawOffScreenBuffer(doubleBuffer, x, y, width, height, x, y);
			Monitor.Exit(buffer);
		}		
		
		/// <see name="DrawSupport.Dispose"/>
		public override void Dispose() {
			// waiting for last drawing operation to complete
			Monitor.Enter(buffer);
			buffer.Dispose();
			doubleBuffer.Dispose();
			disposed = true;
			Monitor.Exit(buffer);
		}	
		
	}
	
	
	/// <summary>
	/// An implementation of OffScreenBuffer using the classes provided by the dotNet Framework
	/// </summary>
	public class DotNetOffScreenBuffer : OffScreenBuffer {
		
		private int width;
		private int height;
		private PixelFormat format;
		private Bitmap buffer;
		private SolidBrush brush;
		private Graphics graphics;
		private bool disposed = false;
		private int depth;
		
		/// <summary> constructor for DotNetOffScreenBuffer, uses an existing Bitmap to store it's data </summary>
		/// <param name="bitmap">where the data of the offscreenbuffer is located</param>	
		/// <param name="depth"></param>
		public DotNetOffScreenBuffer(Bitmap bitmap, int depth) {
			buffer = bitmap;
			width = bitmap.Width;
			height = bitmap.Height;
			format = bitmap.PixelFormat;
			this.depth = depth;
			brush = new SolidBrush(new Color());
			graphics = Graphics.FromImage(bitmap);
		}
		
		/// <summary> constructor for DotNetOffScreenBuffer, creates a new Bitmap to store it's data </summary>
		/// <param name="width">the width of the buffer</param>	
		/// <param name="height">the height of the buffer</param>
		/// <param name="format">the pixelformat for the buffer</param>
		/// <param name="depth"></param>
		public DotNetOffScreenBuffer(int width, int height, PixelFormat format, int depth) {
			this.width = width;
			this.height = height;
			this.format = format;
			this.depth = depth;
			buffer = new Bitmap(width, height, format);
			graphics = Graphics.FromImage(buffer);
			brush = new SolidBrush(new Color());
		}
		
		/// <see name="OffScreenBuffer.Width"/> 
		public int Width { get { return width; } }
		/// <see name="OffScreenBuffer.Height"/> 
		public int Height { get  { return height; } }
		
		internal Bitmap getBitmap() { return buffer; }
		
		/// <see name="OffScreenBuffer.drawFilledRectangle"/> 
		public void drawFilledRectangle(Color color, int x, int y, int width, int height) {
			brush.Color = color;
			graphics.FillRectangle(brush, x, y, width, height);	
		}
		
		/// <see name="OffScreenBuffer.drawOffScreenBuffer"/> 		
		public void drawOffScreenBuffer(OffScreenBuffer src, int destX, int destY) {
			graphics.DrawImage(((DotNetOffScreenBuffer)src).getBitmap(), destX, destY);
		}

		/// <see name="OffScreenBuffer.drawOffScreenBuffer"/> 		
		public void drawOffScreenBuffer(OffScreenBuffer src, int destX, int destY, int width, int height, int srcX, int srcY) {
			graphics.DrawImage(((DotNetOffScreenBuffer)src).getBitmap(), destX, destY, new Rectangle(srcX, srcY, width, height), GraphicsUnit.Pixel);
		}
		
		/// <see name="OffScreenBuffer.drawFromByteArray"/> 		
		public void drawFromByteArray(byte[] data, int width, int height, int destX, int destY, PixelDecoder usedDecoder) {
			// decoding:
			GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			IntPtr srcAdr = Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);
			Bitmap bitmap = new Bitmap(width, height, usedDecoder.calculateStride(width), usedDecoder.getTargetPixelFormat(), srcAdr);
			dataHandle.Free();

			// drawing
			graphics.DrawImage(bitmap, destX, destY);	
			bitmap.Dispose();
		}
		
		/// <see name="OffScreenBuffer.drawText"/> 
		public void drawText(string text, int x, int y, Font font, Color color) {
			brush.Color = color;
			graphics.DrawString(text, font, brush, x, y);
		}

		/// <see name="OffScreenBuffer.Dispose"/> 		
		public void Dispose() {
			brush.Dispose();
			graphics.Dispose();
			buffer.Dispose();
			disposed = true;
		}
		/// <summary> finalizer </summary>
		~DotNetOffScreenBuffer() {
			if (!disposed) { Dispose(); }
		}
			
	}	


} 