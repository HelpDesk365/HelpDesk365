MSSDKLIBS = C:\Programme\Microsoft SDK\lib
DOTNETLIBS = C:\Programme\Microsoft.NET\FrameworkSDK\Lib
DOTNETVC = C:\Programme\Microsoft Visual Studio .NET\VC7\Lib

MSSDKINC = C:\Programme\Microsoft SDK\Include
VCINC = C:\Programme\Microsoft Visual Studio .NET\Vc7\Include

vncclient: 
	csc $(CSFLAGS) /doc:docfile.xml /out:.\VNCClient.exe VNC.cs RFBSurface.cs RFBView.cs RFBProtocolHandling.cs Authentication.cs DrawingSupport.cs RFBStreamInflator.cs RFBUpdateDecoders.cs PixelDecoders.cs Configuration.cs /R:DirectXLib.dll;NZipLib.dll

directxlib: 
	cl /clr /I"$(MSSDKINC)" /I"$(VCINC)" /FeDirectXLib.dll /LD *.cpp /link "$(DOTNETLIBS)\mscoree.Lib" "$(MSSDKLIBS)\ddraw.lib" "$(MSSDKLIBS)\uuid.lib" "$(DOTNETVC)\libc*.lib" "$(DOTNETVC)\oldnames.lib" "$(MSSDKLIBS)\kernel32.lib" "$(MSSDKLIBS)\dxguid.lib" "$(DOTNETVC)\libc*.lib"
all: directxlib vncclient
