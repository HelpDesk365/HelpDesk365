using System.Windows.Forms;
using System;
using System.Drawing;
using DrawingSupport;

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
	

namespace VNC.RFBDrawing {

	/// <summary>
	/// An instance of this class shows the content of an RFB-Surface.
	/// </summary>
	/// <remarks>
	/// an RFBView observes a RFBSurface. This RFBSurface contains the data of the remote frame buffer.
	/// The RFBView show the content of this surface. Furthermore an RFB-View informs
	/// an RFBSurface of events like mouse move or key press.
	/// </remarks>
	public class RFBView : Control {
		/// <summary> the surface, this view is connected to </summary>
		private RFBSurface surface;
		/// <summary> the state of the mousebuttons </summary>
		private byte mouseMask; 
		private bool shiftDown = false;
		private bool controlDown = false;
		private bool altDown = false;
		
		private KeyTable keyTable;
		private DrawSupport drawSup;
		private Control controlConnectedTo = null;
		private int surfaceWidth, surfaceHeight;
		private HScrollBar horizontalscr;
		private VScrollBar verticalscr;
		private String serverName = "";
		/// <summary>the serverName of the server the shown RFB-Surface is connected to</summary>
		public String ServerName { get { return serverName; } }
		private bool connected = false;
		/// <summary> is the view connected to a surface </summary>
		public bool Connected { get { return connected; } }
		/// <summary> constructor for an RFBView </summary>
		public RFBView() {
				// register with the surface
				// creating vncCuror:
				Cursor = new Cursor("VNCCursor.cur");
				// adding scrollbars:
				horizontalscr = new HScrollBar();
				horizontalscr.Dock = DockStyle.Bottom;
				horizontalscr.Scroll += new ScrollEventHandler(handleHScroll);
				horizontalscr.Cursor = Cursors.Default;				
				Controls.Add(horizontalscr);
				verticalscr = new VScrollBar();
				verticalscr.Dock = DockStyle.Right;
				verticalscr.Scroll += new ScrollEventHandler(handleVScroll);
				verticalscr.Cursor = Cursors.Default;
				Controls.Add(verticalscr);
								
				horizontalscr.SmallChange = 1;
				horizontalscr.LargeChange = 1;
				
				// anchor views at all sides of the container
				Anchor = AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
				
				// inital mousemask
				mouseMask = 0; // no mouse button is pressed				
				
				// creating keytable: contains mapping between .NET and VNC key-Coding
				keyTable = KeyTable.getKeyTable();
				// set Location in container
				Location = new Point(0,0);
		}
		
		/// <summary>
		/// the surface calls this method, when the data has changed
		/// changed region: x,y,width,height
		/// </summary>
		internal void notifyUpdate(int x, int y, int width, int height) {
			repaintRegion(new Rectangle(x, y, width, height));			
		}

		/// <summary>
		/// repaints the region decribed by region
		/// </summary>
		private void repaintRegion(Rectangle region) {
			// repaint the content of the surface!
			drawSup.drawBackBufferToScreen(this, region);
		}
		
		/// <summary>
		/// this method must be called to connect the view to the parent-Control and to the Surface
		/// </summary>
		public void connect(Control parent, RFBSurface surface) {

			if (controlConnectedTo != null) { throw new Exception("cannot reconnect View to another control"); }

			parent.Controls.Add(this); // for some drawsupport-classes: must be done before calling surface.connectSurfaceToView
			
			Width = Parent.ClientRectangle.Width;
			Height = Parent.ClientRectangle.Height;
			controlConnectedTo = Parent;
			
			setRFBSurface(surface);
			serverName = surface.ServerName;
			connected = true;
		}
		/// <summary> connect this view to a surface </summary>
		private void setRFBSurface(RFBSurface surface) {
			// connecting to surface
			this.surface = surface;
			surface.connectSurfaceToView(this);
			// install Eventhandlers
			Resize += new EventHandler(resizeView);
			Paint += new PaintEventHandler(viewPaint);
			KeyUp += new KeyEventHandler(viewKeyUp);
			KeyDown += new KeyEventHandler(viewKeyDown);
			MouseDown += new MouseEventHandler(viewMouseDown);
			MouseUp += new MouseEventHandler(viewMouseUp);
			MouseMove += new MouseEventHandler(viewMouseMove);
		}
		/// <summary> sets the drawsupport which is used for drawing to screen </summary>
		internal void setDrawSupport(DrawSupport drawSup) {
			// getting the drawSup-Object
			this.drawSup = drawSup;
			drawSup.registerView(this);
		}

		/// <summary>
		/// this method is called, to set the size of the frame buffer after the size
		/// of the remote desktop is sent during connection establishment
		/// </summary>
		internal void setRFBSize(int width, int height) {
			surfaceWidth = width;
			surfaceHeight = height;
			// initalizing scrollbars and drawable size
			updateSizeAndScrollbars();
		}
		/// <summary>
		/// disconnect the view from the surface
		/// </summary>
		public void disconnectView() {
			if (surface != null) { 
				surface.disconnectView(this);
				surface = null;
			}
			connected = false;
		}
		
		private Size drawableSize = new Size();
		/// <summary> gets the size usable to draw in this view </summary>
		public Size DrawableSize { // efficient: do not recalculte if not necassary
			get { return drawableSize; }
		}
		
		/// <summary>
		/// this method specifies the rectangle of the RFBSurface to show
		/// </summary>
		public Rectangle ShowedRectangle {
			get {
				Rectangle toShow = new Rectangle();
				toShow.X = horizontalscr.Value;
				toShow.Y = verticalscr.Value;
				toShow.Width = DrawableSize.Width;
				toShow.Height = DrawableSize.Height;
				
				// if there is not enough to show ...
				if ((surfaceWidth-toShow.X) < toShow.Width) { toShow.Width = surfaceWidth - toShow.X; }
				if ((surfaceHeight-toShow.Y) < toShow.Height) { toShow.Height = surfaceHeight - toShow.Y; }
				
				return toShow;		
			}
		}

		/// <summary>
		/// updates the size of the drawable region and updates the position of the scrollbars
		/// after resizing or after connection establishment
		/// scrollbars: used in a way that should be easy for drawing
		/// </summary>
		private void updateSizeAndScrollbars() {
			
			// initalize drawable size to the value if scrollbars are needed (beware: scrollbars need some space)
			drawableSize.Width = ClientSize.Width - horizontalscr.Height;
			drawableSize.Height = ClientSize.Height - verticalscr.Width;

			bool horizontalBarNeeded = false;
			bool verticalBarNeeded = false;
			
			// is scrollbar needed because not enough space is usable without scrollbars
			if (ClientSize.Width < surfaceWidth) {
				horizontalBarNeeded = true;
			}
			if (ClientSize.Height < surfaceHeight) {
				verticalBarNeeded = true;
			}
			// check if using a scrollbar forces the usage of the scrollbar in the other direction
			if (horizontalBarNeeded && (drawableSize.Height < surfaceHeight)) {
				verticalBarNeeded = true;
			}
			if (verticalBarNeeded && (drawableSize.Width < surfaceWidth)) {
				horizontalBarNeeded = true; 
				// this can't force needing a vertical bar, because a verticalBar needed must
				// be true for this statement to be reachable
			}
			// the above check determine the need for scrollbars correctly because:
			// horizontal and vertical true because of ClientSize --> ok, both bars are needed
			// horizontal and vertical false after ClientSize test --> ok, both bare are not needed
			// horizontal true, vertical false after ClientSize test --> need to test, if horizontal scroll bar forces vertical one
			//
			// horizontal false, vertical true after ClientSize test --> need to test, if vertical scroll bar forces horizontal one
			//
			
			// set drawable region and scrollbar values accoring to space available
			if (horizontalBarNeeded) {
				// horizontal scrollbar needed
				horizontalscr.Visible = true;
			} else {
				// hide horizontal scrollbar
				horizontalscr.Visible = false;	
				// set horizontal scrollbar position to 0
				horizontalscr.Value = 0;
				// because no horizontal scrollbar is needed, vertical drawable size is not reduced by the scrollbar size
				drawableSize.Height = ClientSize.Height;
			}
			
			if (verticalBarNeeded) {
				// vertical scrollbar needed
				verticalscr.Visible = true;
			} else {
				// hide vertical scrollbar
				verticalscr.Visible = false;
				// set vetrical scrollbar position to 0
				verticalscr.Value = 0;
				// because no vertical scrollbar is needed, horizontal drawable size is not reduced by the scrollbar size
				drawableSize.Width = ClientSize.Width;
			}			
			
			// set scrollbars possible values
			horizontalscr.Maximum = surfaceWidth - DrawableSize.Width + horizontalscr.LargeChange - 1;
			if (horizontalscr.Maximum < 0) { horizontalscr.Maximum = 0; horizontalscr.Value = 0; }
			verticalscr.Maximum = surfaceHeight - DrawableSize.Height + verticalscr.LargeChange - 1;
			if (verticalscr.Maximum < 0) { verticalscr.Maximum = 0; verticalscr.Value = 0; }
			horizontalscr.Minimum = 0;
			verticalscr.Minimum = 0;
			
			// the following two conditions shouldn't be true normally, can happen if window is minimized (possible problem?)
			if (drawableSize.Width < 0) { drawableSize.Width = 0; }
			if (drawableSize.Height < 0) { drawableSize.Height = 0; }		
		}


		// --------------------------
		// the event-handling methods
		// --------------------------

		/// <summary> react to: rfb-View lost its content, needs repaint.
		/// (reaction to paint-event)
		/// </summary>
		private void viewPaint(object sender, PaintEventArgs e) {
			// repaint the content of the whole surface!				
			drawSup.drawBackBufferToScreen(this);
		}
		
		/// <summary> handle resize of the view (reaction to resize event) </summary>
		private void resizeView(object sender, EventArgs e) {
			// update drawableSize and scrollbar values
			updateSizeAndScrollbars();
			// redraw buffer			
			drawSup.drawBackBufferToScreen(this);
		}
		/// <summary> handles a horizontal scroll event (reaction to event) </summary>		
		private void handleHScroll(object sender, ScrollEventArgs e) {
			drawSup.drawBackBufferToScreen(this);
		}
		/// <summary> handles a vertical scroll event (reaction to event) </summary>
		private void handleVScroll(object sender, ScrollEventArgs e) {
			drawSup.drawBackBufferToScreen(this);
		}
		
		/// <summary> mouse button down (reaction to event) </summary>
		private void viewMouseDown(object sender, MouseEventArgs e) {
			// e.X, e.Y are in coordinates of the rfb-View.
			switch (e.Button) {
				case MouseButtons.Left: mouseMask = (byte)(mouseMask | 0x01); break;
				case MouseButtons.Middle: mouseMask = (byte)(mouseMask | 0x02); break;
				case MouseButtons.Right: mouseMask = (byte)(mouseMask | 0x04); break;
				case MouseButtons.XButton1: mouseMask = (byte)(mouseMask | 0x08); break;
				case MouseButtons.XButton2: mouseMask = (byte)(mouseMask | 0x10); break;
				default : break;
			}
			// inform surface
			Rectangle sr = ShowedRectangle; // compensate scrolling 
			surface.handlePointerEvent(mouseMask, (ushort)(e.X + sr.X), (ushort)(e.Y + sr.Y));
		}
		
		/// <summary> mouse button up (reaction to event) </summary>
		private void viewMouseUp(object sender, MouseEventArgs e) {
			// e.X, e.Y are in coordinates of the rfb-View.
			switch (e.Button) {
				case MouseButtons.Left: mouseMask = (byte)(mouseMask & 0xFE); break;
				case MouseButtons.Middle: mouseMask = (byte)(mouseMask & 0xFD); break;
				case MouseButtons.Right: mouseMask = (byte)(mouseMask & 0xFB); break;
				case MouseButtons.XButton1: mouseMask = (byte)(mouseMask & 0xF7); break;
				case MouseButtons.XButton2: mouseMask = (byte)(mouseMask & 0xEF); break;
				default : break;
			}
			// inform surface
			Rectangle sr = ShowedRectangle; // compensate scrolling 
			surface.handlePointerEvent(mouseMask, (ushort)(e.X+sr.X), (ushort)(e.Y+sr.Y));
		}
		
		/// <summary> mouse move (reaction to event) </summary>
		private void viewMouseMove(object sender, MouseEventArgs e) {
			// e.X, e.Y are in coordinates of the rfb-View.
			Rectangle sr = ShowedRectangle; // compensate scrolling 
			surface.handlePointerEvent(mouseMask, (ushort)(e.X + sr.X), (ushort)(e.Y + sr.Y));
		}

		/// <summary> key up (reaction to event) </summary>
		private void viewKeyUp(object sender, KeyEventArgs e) {

			// handle modifier: inform surface of changes in modifier key state
			handleModifierKeys(e);
			
			// calculate keySym:
			uint keySym = keyTable.getKeySym(e);
			if ((keySym >= 0xff51) && (keySym <= 0xff54)) {
				// no down event generated --> compensate:
				surface.handleKeyEvent(keySym,true);	
			}
			
			// key keySym now released: inform surface
			surface.handleKeyEvent(keySym,false);
		}

		/// <summary> key down (reaction to event) </summary>
		private void viewKeyDown(object sender, KeyEventArgs e) {

			// special: F7: paste the local clipboard to the server:
			if ( ((Keys.F7 & e.KeyData) == Keys.F7) ) {
				surface.sendClientCutText();
			}
			
			// handle modifier: inform surface of changes in modifier key state
			handleModifierKeys(e);
			// calculate keySym:
			uint keySym = keyTable.getKeySym(e);
			
			// key keySyn now pressed: inform surface
			surface.handleKeyEvent(keySym,true);			
		}
		
		/// <summary> inform surface of modifier key status change </summary>
		private void handleModifierKeys(KeyEventArgs e) {
								
			if (e.Alt) {
				if (!altDown) {
					surface.handleKeyEvent(0x00ffe9 , true);
					altDown = true;
				}	
			} else {
				if (altDown) {
					surface.handleKeyEvent(0x00ffe9 , false);
					altDown = false;
				}
			}
			if (e.Control) {
				if (!controlDown) {
					surface.handleKeyEvent(0x00ffe3 , true);
					controlDown = true;
				}
			} else {
				if (controlDown) {
					surface.handleKeyEvent(0x00ffe3 , false);
					controlDown = false;
				}
			}
			
			if (e.Shift) {
				if (!shiftDown) {
					surface.handleKeyEvent(0x00ffe1 , true);
					shiftDown = true;
				}
			} else {
				if (shiftDown) {
					surface.handleKeyEvent(0x00ffe1 , false);
					shiftDown = false;
				}
			}
		}
	}	


	/// <summary>
	/// KeyTable containts mapping from keys to keySyms
	/// </summary>
	class KeyTable {
		
		private uint[] table = new uint[512];
		
		private KeyTable() {
			initKeyTable();
		}
		private void initKeyTable() {
		
			// index calculated form e.KeyValue and e.Shift
			table[8] = 0xff08; // backspace
			table[9] = 0xff09; // tab
			table[13] = 0xff0d; // Return
			table[27] = 0xff1b; // ESC
			table[32] = 0x0020; // space
			table[33] =	0xff55; // Page Up
			table[34] = 0xff56; // Page Down
			table[35] = 0xff57; // End
			table[36] = 0xff50; // Home
			table[37] = 0xff51; // left
			table[38] =	0xff52;	// up
			table[39] =	0xff53;	// right
			table[40] = 0xff54;	// down

			table[45] = 0xff63; // insert
			table[46] = 0xffff; // delete
			
			
			for (uint i = 48; i <= 57; i++) { // 0 - 9
				table[i] = i; 
			}
			for (uint i = 65; i <= 90; i++) { // a-z
				table[i] = i+32;
			}
			
			for (uint i = (256+65); i <= (256+90) ; i++) { // A-Z
				table[i] = i - 256; 
			}
			
			for (uint i = 112; i <= 121; i++) { // F1 - F10
				table[i] = 0xffbe + (i-112);
			}
			
			table[107] = 240; // - doesn't work with tested servers
				
			table[190] = 46; // .
			table[190+256] = 58; // :
			table[188] = 44; // , 
			table[188+256] = 59; // ;
			
			table[192+256] = 33; // !
			table[50+256] = 34; // "
			table[223] = 36; // $
			table[53+256] = 37; // %
			table[54+256] = 38; // &
			table[219]    = 39; // '
			table[56+256] = 40; // (
			table[57+256] = 41; // )
			table[51+256] = 42; // *
			table[49+256] = 43; // +
			table[55+256] = 47; // /
			table[226] = 60; // <
			table[58+256] = 61; // = geht nicht
			table[226+256] = 62; // > 
			table[219+256] = 63; // ?
				
			table[52+256] = 135; // ç doesn't work with tested servers
			table[222] = 148; // ö
			table[220] = 132; // ä
			table[186] = 129; // ü
			table[222+256] = 130; // é
			table[220+256] = 133; // à
			table[186+256] = 138; // è
			table[191] = 0; // §  doesn't work with tested servers
			table[191+256] = 167; // °
			table[221] = 94; // ^
			table[221+256] = 96; // `
			
		}
		/// <summary> the singleton keyTable </summary>
		private static KeyTable singleKeyTable;
		/// <summary> get an instance of the keytable (singleton pattern) </summary>
		public static KeyTable getKeyTable() {
			if (singleKeyTable == null) {
				singleKeyTable = new KeyTable();
			}
			return singleKeyTable;
		}
		/// <summary> get a keySym for the key </summary>
		public uint getKeySym(KeyEventArgs e) {
			// index calculated form e.KeyValue and e.Shift
			uint index = (uint)e.KeyValue;
			if (e.Shift) { index = index + 256; }
			return table[index];
		}
		
	}


}