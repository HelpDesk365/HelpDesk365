#using <mscorlib.dll>
#using <system.dll>
#using <system.drawing.dll>
#using <system.windows.forms.dll>


#include <ddraw.h>
#include <dxerr8.h>
#include <malloc.h>
#include <string.h>
#include <tchar.h>


using namespace System::Runtime::InteropServices;

// author: Dominic Ullmann, dominic_ullmann@swissonline.ch
// Version: 1.0

// this libary supports using direct-X with the .NET Framework
namespace DirectXLIB {

	
	
	__value public enum CONST_DDSCLFLAGS { DdSCL_NORMAL = DDSCL_NORMAL, 
										   DdSCL_EXCLUSIVE = DDSCL_EXCLUSIVE,
										   DdSCL_FULLSCREEN = DDSCL_FULLSCREEN };
	__value public enum CONST_DDSDMFLAGS { DdSDM_DEFAULT = 0 };
	__value public enum CONST_DDSCAPSFLAGS { DdSCAPS_OFFSCREENPLAIN = DDSCAPS_OFFSCREENPLAIN, 
							  DdSCAPS_PRIMARYSURFACE = DDSCAPS_PRIMARYSURFACE,
							  DdSCAPS_BACKBUFFER = DDSCAPS_BACKBUFFER,
							  DdSCAPS_SYSTEMMEMORY = DDSCAPS_SYSTEMMEMORY
							};
	__value public enum CONST_DDSURFACEDESCFLAGS { DdSD_BACKBUFFERCOUNT = DDSD_BACKBUFFERCOUNT,
									DdSD_WIDTH = DDSD_WIDTH,
									DdSD_HEIGHT = DDSD_HEIGHT,
									DdSD_CAPS = DDSD_CAPS,
									DdSD_LPSURFACE = DDSD_LPSURFACE
							};
	__value public enum CONST_DDBLTFASTFLAGS {  DdBLTFAST_WAIT = DDBLTFAST_WAIT,
												DdBLTFAST_DONOTWAIT = DDBLTFAST_DONOTWAIT							
							};
	__value public enum CONST_DDBLTFLAGS { DdBLT_WAIT = DDBLT_WAIT,
									  	   DdBLT_DONOTWAIT = DDBLT_DONOTWAIT,
								  	   	   DdBLT_COLORFILL = DDBLT_COLORFILL
							};
	__value public enum CONST_DDLOCKFLAGS { DdLOCK_DONOTWAIT = DDLOCK_DONOTWAIT,
											DdLOCK_EVENT = DDLOCK_EVENT,
											DdLOCK_NOSYSLOCK = DDLOCK_NOSYSLOCK,
											DdLOCK_READONLY = DDLOCK_READONLY,
											DdLOCK_SURFACEMEMORYPTR = DDLOCK_SURFACEMEMORYPTR,
											DdLOCK_WAIT = DDLOCK_WAIT,
											DdLOCK_WRITEONLY = DDLOCK_WRITEONLY			
							};
	
	
	
	__gc public class DirectDrawSurface; // forward declaration
	__gc public class DirectDrawClipper; // forward declaration
	__value public struct STRUCT_DDSURFACEDESC2; // forward declaration
	__value public struct STRUCT_DDBLTFX; // forward declaration
	
	__gc public class DirectDraw : public System::ComponentModel::Component
	{

            protected:
                IDirectDraw7 __nogc *m_pDDraw; 

            public:
                DirectDraw();
                ~DirectDraw();

                IDirectDraw7 __nogc *  GetDirectDraw7();
                
                void setCooperativeLevel(int handle, CONST_DDSCLFLAGS flags);
                void setDisplayMode(int width, int height, int bitsPerPixel, int ref, CONST_DDSDMFLAGS flags);
                DirectDrawSurface * createSurface(STRUCT_DDSURFACEDESC2 & desc);
                DirectDrawClipper * createClipper(int flags);



		

	};
	
	
	 
	
	
	
	__gc public class DirectX : public System::ComponentModel::Component
        {

            public:
                DirectX();
                ~DirectX();
		
				DirectDraw* createDirectDraw7();
                

        };


	__gc public class DirectDrawSurface : public System::ComponentModel::Component
	{
		protected:
			DirectDraw * ddraw;
            IDirectDrawSurface7 __nogc *m_pSurface; 
		
		public:
			DirectDrawSurface(DirectDraw * ddraw, STRUCT_DDSURFACEDESC2 & desc);
			~DirectDrawSurface();
			void cleanUpResources();
			void handleDispose(Object* sender, System::EventArgs* e);
			
            IDirectDrawSurface7 __nogc *  GetDirectDrawSurface7();
            int getDC();
            void releaseDC(int dc);
            void bltFast(int dx, int dy, DirectDrawSurface* srcSurface, System::Drawing::Rectangle region, CONST_DDBLTFASTFLAGS flags);
            void blt(System::Drawing::Rectangle destRect, DirectDrawSurface* srcSurface, System::Drawing::Rectangle srcRect, CONST_DDBLTFLAGS flags, STRUCT_DDBLTFX & ddblt);
            void blt(System::Drawing::Rectangle destRect, DirectDrawSurface* srcSurface, System::Drawing::Rectangle srcRect, CONST_DDBLTFLAGS flags);

			void lockSurface(System::Drawing::Rectangle region, STRUCT_DDSURFACEDESC2 & desc, CONST_DDLOCKFLAGS flags, int hnd); 
            void unlock(System::Drawing::Rectangle region); 
            
            void setClipper(DirectDrawClipper * clipper);
            
            bool isLost();
            void restore();
            
            
            
            void drawArrayDataToSurface(byte buffer __gc [], int width, int height, int bytePerPixel);
    
    
    	private:
    		// RECT * locked;        
	
	};
	
	
	
	
	__gc public class DirectDrawClipper : public System::ComponentModel::Component
	{	
		protected:
			IDirectDrawClipper __nogc * m_pClipper;

		public:
			DirectDrawClipper(DirectDraw * ddraw, int flags);
			~DirectDrawClipper();
			
			IDirectDrawClipper __nogc * getClipper();
			
			int getHWnd();
    		void setHWnd(int hwindow);
	};
	
	__value public struct STRUCT_DDSURFACEDESC2 {
			CONST_DDSURFACEDESCFLAGS flags; // determines what fields are valid
			int dwHeight; // height of surface to be created
			int dwWidth; // width of input surface
			int dwBackBufferCount; // number of back buffers requested
			System::IntPtr lpSurface; // pointer to the associated surface memory
		    CONST_DDSCAPSFLAGS ddsCaps; // direct draw surface capabilities
	}; 
	
	__value public struct STRUCT_DDBLTFX {
			int dwFillColor;
	};
		
		

	

	__gc public class DirectXException : public System::Exception {
		
		int exceptionNr;
		
		public:
			DirectXException(int exceptionNr) {
				(*this).exceptionNr = exceptionNr;
			}
			
			int getExceptionNr() {
				return exceptionNr;
			}
		
	};


	
	
	// ***************************************** Direct X *******************************
	
	DirectDraw* DirectX::createDirectDraw7() {
		return new DirectDraw();	
	}
	
	DirectX::DirectX() {
	}
	
	DirectX::~DirectX() {
	}


	// ***************************************** Direct Draw ****************************


	DirectDraw::DirectDraw()
	{ 
		
		IDirectDraw7 __nogc *pDDraw;

/*		HRESULT res = DirectDrawCreate(NULL, (LPDIRECTDRAW*)&pDDraw, NULL);
		m_pDDraw = pDDraw;
		
		if (m_pDDraw == NULL) {
			throw new DirectXException(res);
		} */

		
		HRESULT res = DirectDrawCreateEx(NULL, (VOID**)&pDDraw,
                                         IID_IDirectDraw7, NULL ); 
        
        m_pDDraw = pDDraw;
		
		if (m_pDDraw == NULL) {
			throw new DirectXException(res);
		}
		// System::Console::WriteLine("Direct Draw created ok");
	} 



	DirectDraw::~DirectDraw()
	{
    	if (m_pDDraw != NULL)
    	{
       		m_pDDraw->Release();
       		m_pDDraw = NULL;
   		}
	}
	
	
	IDirectDraw7 __nogc *  DirectDraw::GetDirectDraw7() {
		return m_pDDraw;
	
	}


	void DirectDraw::setCooperativeLevel(int handle, CONST_DDSCLFLAGS flags) {
		HRESULT res = m_pDDraw->SetCooperativeLevel((HWND)handle,flags);
		// const char * msg = DXGetErrorDescription8(res);
		// System::Console::WriteLine(msg);
		if (FAILED(res)) Marshal::ThrowExceptionForHR(res);
	}
	
	void DirectDraw::setDisplayMode(int width, int height, int bitsPerPixel, int ref, CONST_DDSDMFLAGS flags) {
		HRESULT res = m_pDDraw->SetDisplayMode(width,height,bitsPerPixel,ref,flags);
		if (FAILED(res)) Marshal::ThrowExceptionForHR(res);
		
	}
	
	DirectDrawSurface* DirectDraw::createSurface(STRUCT_DDSURFACEDESC2 & desc) {
		return new DirectDrawSurface(this, desc);
	}
	
    DirectDrawClipper * DirectDraw::createClipper(int flags) {
    	return new DirectDrawClipper(this,flags);
    	
    }


// *************************************** Direct Draw Surface *****************************************
	

	DirectDrawSurface::DirectDrawSurface(DirectDraw * ddraw, STRUCT_DDSURFACEDESC2 & desc) {
		(*this).ddraw = ddraw;
		IDirectDrawSurface7 __nogc *pSurface;

		DDSURFACEDESC2 ddsd;
	    ZeroMemory( &ddsd, sizeof( ddsd ) );
	    ddsd.dwSize            = sizeof( ddsd );
    	ddsd.dwFlags           = desc.flags;
    	ddsd.ddsCaps.dwCaps    = desc.ddsCaps;
    	ddsd.dwBackBufferCount = desc.dwBackBufferCount;
    	ddsd.dwWidth = desc.dwWidth;
    	ddsd.dwHeight = desc.dwHeight;
	    
		HRESULT res = ddraw->GetDirectDraw7()->CreateSurface(&ddsd, (LPDIRECTDRAWSURFACE7*)&pSurface, NULL);
		/* const char * msg = DXGetErrorDescription8(res);
		System::Console::WriteLine(msg); */
		
		if (FAILED(res)) { Marshal::ThrowExceptionForHR(res); }
		
		desc.lpSurface = __nogc new System::IntPtr(ddsd.lpSurface);
		m_pSurface = pSurface;
		Disposed += new System::EventHandler(this, &DirectDrawSurface::handleDispose);
	
	}
	DirectDrawSurface::~DirectDrawSurface() {
		cleanUpResources();
	}
	
	void DirectDrawSurface::handleDispose(Object* sender, System::EventArgs* e) {
		cleanUpResources();
	}
	
	void DirectDrawSurface::cleanUpResources() {
		if (m_pSurface != NULL) {
			m_pSurface->Release();
			m_pSurface = NULL;
		}
	}
	

	IDirectDrawSurface7 __nogc *  DirectDrawSurface::GetDirectDrawSurface7() {
		return m_pSurface;
	}
	
    int DirectDrawSurface::getDC() {
    	HDC dc;
    	HRESULT res = m_pSurface->GetDC(&dc);
    	// const char * msg = DXGetErrorDescription8(res);
		// System::Console::WriteLine(msg);

    	System::IntPtr * resPtr = (__nogc new System::IntPtr(dc));
    	int result = resPtr->ToInt32();
    	delete resPtr; // cleanup

    	if (FAILED(res)) { Marshal::ThrowExceptionForHR(res); }
    	return result;
    }
    
    void DirectDrawSurface::releaseDC(int dc) {
    	System::IntPtr * dcPtr = (__nogc new System::IntPtr(dc));
    	HDC hdc = (HDC) (dcPtr->ToPointer());
    	HRESULT res = m_pSurface->ReleaseDC(hdc);
    	delete dcPtr;
    	if (FAILED(res)) { Marshal::ThrowExceptionForHR(res); }
    }
    
    void DirectDrawSurface::bltFast(int dx, int dy, DirectDrawSurface* srcSurface, System::Drawing::Rectangle region, CONST_DDBLTFASTFLAGS flags) {
    	
    	// unclipped, be careful to not blit outside destinationsurface
    	
    	RECT * rect = new RECT();
    	rect->left = region.X;
    	rect->top= region.Y;
    	rect->right=region.X+region.Width;
    	rect->bottom=region.Y+region.Height;
    	    	
    	IDirectDrawSurface7 * src = srcSurface->GetDirectDrawSurface7();
    	HRESULT res = m_pSurface->BltFast(dx,dy,src,rect,flags);
    	// const char * msg = DXGetErrorDescription8(res);
		// System::Console::WriteLine(msg);
    	
    	// cleanup:
    	delete rect;
    	if (FAILED(res)) { Marshal::ThrowExceptionForHR(res); }
    }
    
    void DirectDrawSurface::blt(System::Drawing::Rectangle destRect, DirectDrawSurface* srcSurface, System::Drawing::Rectangle srcRect, CONST_DDBLTFLAGS flags) {
    	
    	// clipped blitting
    	
    	RECT * rectDest = new RECT();
    	rectDest->left = destRect.X;
    	rectDest->top= destRect.Y;
    	rectDest->right=destRect.X+destRect.Width;
    	rectDest->bottom=destRect.Y+destRect.Height;
    	
    	RECT * rectSrc = new RECT();
    	rectSrc->left = srcRect.X;
    	rectSrc->top= srcRect.Y;
    	rectSrc->right=srcRect.X+srcRect.Width;
    	rectSrc->bottom=srcRect.Y+srcRect.Height;
    	
    	IDirectDrawSurface7 * src = srcSurface->GetDirectDrawSurface7();
    	HRESULT res = m_pSurface->Blt(rectDest,src,rectSrc,flags,NULL);
    	// const char * msg = DXGetErrorDescription8(res);
		// System::Console::WriteLine(msg);
    	
		// cleanup:
    	delete rectDest;
    	delete rectSrc;
    	if (FAILED(res)) { Marshal::ThrowExceptionForHR(res); }
    }
    
    void DirectDrawSurface::blt(System::Drawing::Rectangle destRect, DirectDrawSurface* srcSurface, System::Drawing::Rectangle srcRect, CONST_DDBLTFLAGS flags, STRUCT_DDBLTFX __gc & ddblt) {
    
    	// clipped blitting
    	
    	RECT * rectDest = new RECT();
    	rectDest->left = destRect.X;
    	rectDest->top= destRect.Y;
    	rectDest->right=destRect.X+destRect.Width;
    	rectDest->bottom=destRect.Y+destRect.Height;
    	
    	RECT * rectSrc = new RECT();
    	rectSrc->left = srcRect.X;
    	rectSrc->top= srcRect.Y;
    	rectSrc->right=srcRect.X+srcRect.Width;
    	rectSrc->bottom=srcRect.Y+srcRect.Height;
    	
    	
		DDBLTFX * ddbl = new DDBLTFX();
		ddbl->dwFillColor = ddblt.dwFillColor;
		ddbl->dwSize = sizeof (DDBLTFX);
		
    	IDirectDrawSurface7 * src;
    	if (srcSurface != NULL) {
    	 	src = srcSurface->GetDirectDrawSurface7();
    	} else {
    		src = NULL;
    		delete rectSrc; // without this: a memory leak!
    		rectSrc = NULL;
    	}
    	
    	HRESULT res = m_pSurface->Blt(rectDest,src,rectSrc,flags,(LPDDBLTFX)ddbl);
    	/* const char * msg = DXGetErrorDescription8(res);
		System::Console::WriteLine(msg);
		// delete msg;
		 */

		
		// cleanup:
    	delete rectDest;
    	delete rectSrc;
    	delete ddbl;

    
    	// if (FAILED(res)) { Marshal::ThrowExceptionForHR(res); }
    }
        
	void DirectDrawSurface::lockSurface(System::Drawing::Rectangle region, STRUCT_DDSURFACEDESC2 & desc, CONST_DDLOCKFLAGS flags, int hnd) {
		RECT * locked = new RECT();
    	locked->left = region.X;
    	locked->top= region.Y;
    	locked->right=region.X+region.Width;
    	locked->bottom=region.Y+region.Height;

		DDSURFACEDESC2 ddsd;
	    ZeroMemory( &ddsd, sizeof( ddsd ) );
	    ddsd.dwSize            = sizeof( ddsd );
    	ddsd.dwFlags           = desc.flags;
    	ddsd.ddsCaps.dwCaps    = desc.ddsCaps;
    	ddsd.dwBackBufferCount = desc.dwBackBufferCount;
    	ddsd.dwWidth = desc.dwWidth;
    	ddsd.dwHeight = desc.dwHeight;

    	HRESULT res = m_pSurface->Lock(locked,&ddsd,flags,(HANDLE)((__nogc new System::IntPtr(hnd))->ToPointer()));
    	if (FAILED(res)) { Marshal::ThrowExceptionForHR(res); }

		desc.lpSurface = __nogc new System::IntPtr(ddsd.lpSurface);
		
	}
    void DirectDrawSurface::unlock(System::Drawing::Rectangle region) {
    	System::Console::WriteLine("create rect ");
    	RECT * locked = new RECT();
    	System::Console::WriteLine("rect created ");
    	
    	locked->left = region.X;
    	locked->top= region.Y;
    	locked->right=region.X+region.Width;
    	locked->bottom=region.Y+region.Height;
    	HRESULT res = m_pSurface->Unlock(locked);
    	if (FAILED(res)) { Marshal::ThrowExceptionForHR(res); }
    }
    
    
    void DirectDrawSurface::drawArrayDataToSurface(byte buffer __gc [], int width, int height, int bytePerPixel) {
    	
    		System::Drawing::Rectangle region;
    		region.X = 0;
    		region.Y = 0;
    		region.Width = width;
    		region.Height = height;
    	
	    	STRUCT_DDSURFACEDESC2 lockDesc;
	    	ZeroMemory( &lockDesc, sizeof( lockDesc ) );
	    	
	    	CONST_DDLOCKFLAGS flags;
	    	flags = (CONST_DDLOCKFLAGS) (
	    			((CONST_DDLOCKFLAGS)CONST_DDLOCKFLAGS::DdLOCK_SURFACEMEMORYPTR) |
		    		((CONST_DDLOCKFLAGS)CONST_DDLOCKFLAGS::DdLOCK_WRITEONLY) |
		    		((CONST_DDLOCKFLAGS)CONST_DDLOCKFLAGS::DdLOCK_WAIT)
		    		);
	    	this->lockSurface(region, lockDesc, flags, 0);

			// copying data to surface:
			byte * ptr = (byte *) lockDesc.lpSurface.ToPointer();
			
			// doesn't work, but why ???
			memcpy(lockDesc.lpSurface.ToPointer(), &buffer, width*height);
			
			this->unlock(region);
    }
    
    
    void DirectDrawSurface::setClipper(DirectDrawClipper * clipper) {
    	HRESULT res = m_pSurface->SetClipper(clipper->getClipper());
    	if (FAILED(res)) { Marshal::ThrowExceptionForHR(res); }    	
    }
    
    
    bool DirectDrawSurface::isLost() {
    	HRESULT res = m_pSurface->IsLost();
    	if (res == 0) { return false; } else { return true; }
    }
    void DirectDrawSurface::restore() {
       	HRESULT res = m_pSurface->Restore();
    	if (FAILED(res)) { Marshal::ThrowExceptionForHR(res); }    	
    }



// ************************************* Direct Draw Clipper *************************************

	DirectDrawClipper::DirectDrawClipper(DirectDraw * ddraw, int flags) {

		IDirectDrawClipper __nogc *pClipper;
		HRESULT res = ddraw->GetDirectDraw7()->CreateClipper(flags, &pClipper, NULL);
		// const char * msg = DXGetErrorDescription8(res);
		// System::Console::WriteLine(msg);
		
		if (FAILED(res)) { Marshal::ThrowExceptionForHR(res); }
		m_pClipper = pClipper;
	}
	DirectDrawClipper::~DirectDrawClipper() {
		if (m_pClipper != NULL) {
			m_pClipper->Release();
			m_pClipper = NULL;
		}
	}
		
	int DirectDrawClipper::getHWnd() {
		HWND hwindow;
		HRESULT res = m_pClipper->GetHWnd(&hwindow);
    	if (FAILED(res)) { Marshal::ThrowExceptionForHR(res); }		
		return ((int) hwindow);		
	}
	
	void DirectDrawClipper::setHWnd(int hwindow) {
		HRESULT res = m_pClipper->SetHWnd(0, (HWND)hwindow);
	   	if (FAILED(res)) { Marshal::ThrowExceptionForHR(res); }
	}

	IDirectDrawClipper __nogc * DirectDrawClipper::getClipper() {
		return m_pClipper;
	}


	

}

