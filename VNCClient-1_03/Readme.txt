VNCClient for .NET (Version 1.01), (C)2002 Dominic Ullmann (dominic_ullmann@swissonline.ch)


This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.



Getting Started
_______________


required
--------

Microsoft .NET Framework v1.0.375 or greater
for compiling DirectDrawLib additionally:
Microsoft Platform SDK (Core, DirectX)



Build (optional)
-----

Use the provided makefile:
- adjust the INCLUDE and LIB-PATH to the Microsoft Platform SDK / .NET SDK
- nmake /f makefile directXLib for building DirectXLib
- nmake /f makefile VNCClient for building VNCClient
(nmake /f makefile all for building both)



Configuration (optional)
-------------

The VNCClient can be configured to use either DirectX/GDI+ or only GDI+,
using the file VNCClient.config.xml:
drawType=DirectDraw or drawType=GDIPlus.
If GDI+ only is used, more than one view is possible (nrOfViews option).

As a standard GDI+ only is used because of a bug in the DirectX/GDI+ solution 
on some systems (see Problems).


Run
---

VNCClient.exe (in connection-iformation window, input server:port)
or
VNCClient.exe server port


Special
-------

use F7 to send the content of the clipboard to the VNC-Server clipboard. A second effect is,
that this content is sent to the server as if the user made this input.


Development of other Decoders
-----------------------------

This VNCClient supports easy development of decoders: For developing a decoder create a subclass of
Decoder with a no-argument constructor. Insert the FUllName of the new Decoder into the
Decoders section in the VNCClient.config.xml file. 


Credits
_______

This Software has been developed during a Term Project at ETH Zurich by
Dominic Ullmann (dominic_ullmann@swissonline.ch).

This Software uses .NET Zip Library #ziplib (www.icsharpcode.net/OpenSource/SharpZipLib/)
in an extension of the standard RFB Protocol.


VNC-Server
__________

This client is optimized for a use with the VNCServer for the Bluebottle OS (http://www.bluebottle.ethz.ch/).
(At the moment the optimized VNC-Server is not included in the current build, the included VNC-server does not work with this client due to a bug in the server-code)

For Windows TightVNC-Server is a good choice, other servers can be found
at http://www.uk.research.att.com/vnc/index.html



Problems, Bugs, ...
___________________

At the moment on some systems the following problem is present:
The colors displayed are not correct, if DirectDraw mode is used and the client 
display is not using a true color mode (24 bit, 32 bit). 
Solution: use GDI+ only

If you encounter some other problems, please let me know.










