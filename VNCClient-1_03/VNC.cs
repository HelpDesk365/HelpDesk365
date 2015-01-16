using System;
using System.Windows.Forms;
using System.Drawing;
using VNC.RFBDrawing;
using System.Threading;
using VNC.Config;

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


namespace VNC {

	/// <summary> Application main class </summary>
	public class VNCClient {

		/// <summary> the port to connect to </summary>
		private int port = 5900;
		/// <summary> the server to connect to </summary>
		private string server = "";		
		/// <summary> the configuration information </summary>
		private VNCConfiguration config;

		/// <summary> the application entry point </summary>
		[STAThread]
		public static void Main(string[] args) {
			// Starts message handling loop (events from main window) 
			// and shows main window
			VNCClient client = new VNCClient();
			
			try {
				client.config = new VNCConfiguration();
			} catch (Exception e) {
				Console.WriteLine("error in config-file: " + e);
				Environment.Exit(1);
			}

			client.getConnectionInformation(args);

			try {
				client.connect();				
				Application.Run();
			} catch (Exception e) {
				Console.WriteLine("connection failed: " + e);
			}
 		}
 		
 		/// <summary> retrieve the connection Information </summary>
 		private void getConnectionInformation(string[] args) {
			if (args.Length > 1) {
				port = Int32.Parse(args[1]);
			}
			if (args.Length > 0) {
				server = args[0];
			} else {
				// ask user for VNC-Server address and port
		    	ConnectionForm conn = new ConnectionForm();  	
			
				if (conn.ShowDialog() == DialogResult.OK) {
					server = conn.getServer();
					port = conn.getPort();
					conn.Dispose();
				} else {
			    	conn.Dispose();
					Console.WriteLine("Connect Aborted");
					Environment.Exit(0);
		    	}
			}
 		}
 		
 		/// <summary> connect to the VNC-Server </summary>
 		private void connect() {
			Console.WriteLine("Connect to: " + server + " on port: " + port);
			// create the surface
			RFBSurface surface = new RFBSurface(server, port, config);
				
			for (int i = 1; i <= config.NrOfViews; i++) {
				connectView(surface);	
			}			
 		}
 		
 		/// <summary> create and connect a view to the surface </summary>
 		private void connectView(RFBSurface surface) {
			ClientWin win = new ClientWin();
			RFBView view = new RFBView();
			// calling this to activate the view:
			view.connect(win, surface);
			win.registerView(view);
			win.Show(); 			
 		}
 				
	}


	/// <summary> one of the application windows </summary>
	/// <remarks> this Windows is a container for an RFB-View, an RFB-View
	/// is connected to this window using the registerView method. </remarks>
	public class ClientWin : Form {

		private RFBView view;
		private String serverName;
		
		/// <summary> constructs a Window for showing an RFB-View </summary>
		public ClientWin() {
			AutoScroll = false;
			MinimumSize = new Size(100, 100); // a smaller window doesn't make sense
		}
		
		/// <summary> gets the servername of the vnc-server. </summary>
		/// <remarks> This name is sent during connection handshake. This window got this name from the RFBView it contains.
		/// Getting the value from this property makes only sense after a view is registered. </remarks>
		public String ServerName {
			get { return serverName; }
		}
		
		/// <summary>
		/// register a view for beeing displayed in this window
		///	before calling this, view must be connected to a RFBSurface
		/// </summary>
		public void registerView(RFBView view) {
			if (!view.Connected) { throw new Exception("only possible when view is connected"); }
			this.view = view;
			// need notification on window closing
			Closing += new System.ComponentModel.CancelEventHandler(windowClosing);
			this.serverName = view.ServerName;
			Text = "Connected to: " + serverName;
		}
		
		/// <summary> handles window-close events </summary>
		private void windowClosing(object sender, System.ComponentModel.CancelEventArgs e) {
			// tell the view, that it's no longer drawable
			// we must call disconnectView, if view is no longer visible, otherwise connection to the remote server is not
			// correctly closed ...
			view.disconnectView();
		}

	}


	/// <summary> a from for asking for server-address and server-port </summary>
	public class ConnectionForm : Form {

		private Label info;
		private TextBox server;	
		private Button ok;
		private Button cancel;	
		
		/// <summary> constructor for a connection form </summary>
		public ConnectionForm() {
			FormBorderStyle = FormBorderStyle.FixedDialog;
			Text = "Connect to Server";
			info = new Label();
			info.Text = "Please provide Server-Name";
			info.Location = new Point(10, 10);
			info.Size = new Size(300, 30);
			
			server = new TextBox();
			server.Text = "";
			server.ReadOnly = false;
			server.Location = new Point(10, 50);
			server.Size = new Size(300, 30);

			ok = new Button();
			ok.Location = new Point(10, 100);
			ok.DialogResult = DialogResult.OK;
			ok.Text = "Connect";
			AcceptButton = ok;
			cancel = new Button();
			cancel.Location = new Point(100, 100);
			cancel.DialogResult = DialogResult.Cancel;
			cancel.Text = "Abort";

			Controls.Add(info);
			Controls.Add(server);
			Controls.Add(ok);
			Controls.Add(cancel);
		}
		
		/// <summary> gets the server-identifier part of the input </summary>
		public string getServer() {
			if (server.Text.IndexOf(":") > 0) {
				return server.Text.Substring(0,server.Text.IndexOf(":"));
			} else {	
				return server.Text;
			}
		}
		
		/// <summary> gets the server-port part of the input </summary>
		public int getPort() {
			if (server.Text.IndexOf(":") > 0) {
				return Int32.Parse(server.Text.Substring(server.Text.IndexOf(":") + 1));
			} else {	
				return 5900;
			}	
		}
		
		/// <summary> the default size of this window (overridden) </summary>
		protected override Size DefaultSize { 
			get { return new Size(400,200); }
		} 
			
	}

}