using System.Windows.Forms;
using System.Drawing;
using System;
using System.Security.Cryptography;

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


namespace VNC.RFBProtocolHandling.Authenticate {

	/// <summary>
	/// this class implements the authentication with DES in the VNC-Protocol.
	/// </summary>
	class DESAuthenticatior {
	
		/// <summary>
		/// this method encrypts the challange sent by the VNC-Server to
		/// prove the knowledge of the key
		/// </summary>
		public static byte[] encryptChallenge(byte[] challenge, string password) {
			
			DES des = new DESCryptoServiceProvider();
			
			// Derive Key from Password
			byte[] desKey = new byte[8];
			if (password.Length >= 8) {
				System.Text.Encoding.ASCII.GetBytes(password, 0, 8, desKey,0);
			} else {
				System.Text.Encoding.ASCII.GetBytes(password, 0, password.Length, desKey, 0);
			}			

			// because of Bug in VNC-DES Implementation (a key changement is used before using DES:
			desKey = compensateKeyBug(desKey);
			
			des.FeedbackSize = 0;
			des.Padding = PaddingMode.None;
			des.Mode = CipherMode.ECB;
			
			ICryptoTransform enc = des.CreateEncryptor(desKey, null); 
			byte[] response = new byte[16];
			enc.TransformBlock(challenge, 0, challenge.Length, response, 0);

			return response;
		}
		/// <summary>
		/// this method compensates the difference in the vnc DES key usage and
		/// the standard DES key usage.
		///	</summary>
		private static byte[] compensateKeyBug(byte[] desKey) {			
			byte[] newKey = new byte[8];

			for (int i = 0; i < 8; i++) {

				// revert desKey[i]:
				newKey[i] = (byte) (	((desKey[i] & 0x01) << 7) |  
							((desKey[i] & 0x02) << 5) |
							((desKey[i] & 0x04) << 3) |
							((desKey[i] & 0x08) << 1) |
							((desKey[i] & 0x10) >> 1) |
							((desKey[i] & 0x20) >> 3) |
							((desKey[i] & 0x40) >> 5) |
							((desKey[i] & 0x80) >> 7)
						   );
			}
			return newKey;
		}

	}

	/// <summary>
	/// this class represents a form for entering the password
	/// </summary>
	class AuthenticationForm : Form {
		private Label info;
		private TextBox password;
		private Button ok;
		private Button cancel;		
	
		public AuthenticationForm() {
			FormBorderStyle = FormBorderStyle.FixedDialog;
			Text = "Authentication needed";
			info = new Label();
			info.Text = "Please provide Password";
			info.Location = new Point(10, 10);
			info.Size = new Size(300, 30);
			
			password = new TextBox();
			password.Text = "";
			password.ReadOnly = false;
			password.Location = new Point(10, 50);
			password.Size = new Size(300, 30);
			password.PasswordChar = '*';

			ok = new Button();
			ok.Location = new Point(10, 100);
			ok.DialogResult = DialogResult.OK;
			ok.Text = "Ok";
			AcceptButton = ok;
			cancel = new Button();
			cancel.Location = new Point(100, 100);
			cancel.DialogResult = DialogResult.Cancel;
			cancel.Text = "Abort";

			Controls.Add(info);
			Controls.Add(password);
			Controls.Add(ok);
			Controls.Add(cancel);
		}

		public string getPassword() {
			return password.Text;
		}

		protected override Size DefaultSize { 
			get { return new Size(400, 200); }
		}
		
	}
	

}