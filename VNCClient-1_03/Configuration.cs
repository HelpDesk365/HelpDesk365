using System;
using System.Xml;
using System.Xml.Schema;
using System.Collections;



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




namespace VNC.Config {

	/// <summary>
	/// this class reads in, handles and contains the configuration for the VNCClient
	/// </summary>
	public class VNCConfiguration {
	
		/// <summary> the configuration file </summary>
		private const string configFile = "VNCClient.config.xml";
		/// <summary> the schema for the configuration file </summary>
		private const string configXSD = "VNCClient.config.xsd";

	
		/// <summary>
		/// reads in the configuration from VNCClient.config.xml and validates 
		///	against schemaFile VNCClient.config.xsd
		/// </summary>
		public VNCConfiguration() {
			try {
				readConfig();
				validateConfig();
			} catch (System.Xml.Schema.XmlSchemaException schemaEx) {
				Console.WriteLine("validation problem: " + schemaEx);
				throw new Exception("config XML-File is not valid against the schema: " + schemaEx);
			} catch (System.Xml.XmlException xmlEx) {
				Console.WriteLine("config-file problem: " + xmlEx);
				throw new Exception("config XML-File contains syntactic errors: " + xmlEx);
			}
		}
		
		private DrawTypes drawType;
		/// <summary> the drawing type: use DirectDraw or pure GDIPlus </summary>
		public DrawTypes DrawType {
			get {
				return drawType;
			}
		}
		
		private int nrOfViews;
		/// <summary> the nr of views to display, >1 only supported for GDI+-mode
		/// </summary>
		public int NrOfViews {
			get {
				return nrOfViews;
			}
		}
		
		private ArrayList decoders = new ArrayList();
		/// <summary> the configured decoders </summary>
		public ArrayList Decoders {
			get {
				return decoders;
			}
		}
		
		 /// <summary> read in the configuration information </summary>
 		private void readConfig() {
 			XmlTextReader reader = new XmlTextReader(configFile);
 			XmlValidatingReader valReader = new XmlValidatingReader(reader);
 			XmlSchemaCollection xsc = new XmlSchemaCollection();
 			XmlSchema schema = XmlSchema.Read(new XmlTextReader(configXSD), null);
 			xsc.Add(schema);
 			valReader.Schemas.Add(xsc);

			// process the xml file, validate against schema
 			XmlDocument doc = new XmlDocument();
 			doc.Load(valReader);
			// read in config parameters
			XmlNode drawTypeNode = doc.SelectSingleNode("/config/graphics/drawingType");
			XmlNode nrOfViewsNode = doc.SelectSingleNode("/config/graphics/nrOfViews");
			string drawTypeAsString = drawTypeNode.FirstChild.Value;
			drawType = getDrawType(drawTypeAsString);
			nrOfViews = Int32.Parse(nrOfViewsNode.FirstChild.Value);
			
			// reading in the the decoders
			readDecoders(doc);
 		}
 		
 		/// <summary> get a DrawTypes for a drawType as string </summary>
 		private DrawTypes getDrawType(string drawTypeAsString) {
 			if (drawTypeAsString.Equals("DirectDraw")) {
 				return DrawTypes.DirectDraw;
 			} else if (drawTypeAsString.Equals("GDIPlus")) {
 				return DrawTypes.GDIPlus;
 			} else {
				// this should not be the case, because of validation
 				Console.WriteLine("config not ok: drawType = {0} unknown", drawTypeAsString);
 				throw new Exception("config not ok: drawType unknown");
 			}
 		}
 		
 		/// <summary> reads in the configured decoders </summary>
 		private void readDecoders(XmlDocument configDoc) {
 			XmlNodeList decoderList = configDoc.SelectNodes("/config/decoders/decoder");
			// because of schema validation, should get >= 1 decoder
			if (decoderList.Count == 0) { throw new Exception("no decoders in config-file, check config/decoders in VNCClient.config.xml"); }

			decoders.Clear();
			foreach (XmlNode dec in decoderList) {
				// adding the decoder to the decoder list
				decoders.Add(dec.FirstChild.Value);
	 		}
 		}
 		
 		/// <summary> validate the config-Information </summary>
 		private void validateConfig() {
 			if (drawType == DrawTypes.DirectDraw) {
 				if (nrOfViews > 1) {
 					Console.WriteLine("for DirectDrawMode only one view allowed!, setting to one");
 					nrOfViews = 1;
 				}
 			}
 		}
	
	}
	
	
	/// <summary> enumeration of the possible draw types </summary>
	public enum DrawTypes {
		/// <summary> using DirectDraw for drawing </summary>
		DirectDraw = 0,
		/// <summary> using GDIPlus for drawing </summary>
		GDIPlus = 1
	}

}