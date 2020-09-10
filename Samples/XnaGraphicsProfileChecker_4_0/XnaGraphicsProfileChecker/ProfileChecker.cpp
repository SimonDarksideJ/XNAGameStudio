//-----------------------------------------------------------------------------
// ProfileChecker.cpp
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#include "stdafx.h"
#include "ProfileChecker.h"
#include "ProfileCapabilities.h"

using namespace XnaGraphicsProfileChecker;


// Constructor performs the caps check.
ProfileChecker::ProfileChecker(GraphicsProfile profile)
{
    errors = gcnew List<String^>();

    IDirect3D9* pD3D = Direct3DCreate9(D3D_SDK_VERSION);

    if (pD3D)
    {
        try
        {
            CheckProfileSupport(profile, pD3D);
        }
        finally
        {
            pD3D->Release();
        }
    }
    else
    {
        errors->Add("Direct3DCreate9 failed");
    }
}


// D3D query APIs need an adapter format, but this isn't actually relevant to what they
// return on any modern hardware, so we just pass this default to keep the API happy.
#define IRRELEVANT_ADAPTER_FORMAT D3DFMT_X8R8G8B8


// Helper for checking a limit such as a max size.
#define CHECK_LIMIT(field, limit)                                       \
{                                                                       \
    if (caps.field < unsigned(limit))                                   \
    {                                                                   \
        errors->Add(String::Format("{0} = {1}", #field, caps.field));   \
    }                                                                   \
}


// Helper for making sure a specific caps bit is set.
#define ENSURE_CAP(field, bit)                                          \
{                                                                       \
    if (!(caps.field & bit))                                            \
    {                                                                   \
        errors->Add(String::Format("No {0}.{1}", #field, #bit));        \
    }                                                                   \
}


// Helper for making sure a specific caps bit is not set.
#define REJECT_CAP(field, bit)                                          \
{                                                                       \
    if (caps.field & bit)                                               \
    {                                                                   \
        errors->Add(String::Format("Unwanted {0}.{1}", #field, #bit));  \
    }                                                                   \
}


void ProfileChecker::CheckProfileSupport(GraphicsProfile graphicsProfile, IDirect3D9* pD3D)
{
    // Look up what caps are required by the requested profile.
    ProfileCapabilities^ profileCapabilities = gcnew ProfileCapabilities(graphicsProfile);

    // Query the D3D caps.
    D3DCAPS9 caps;
    
    if (FAILED(pD3D->GetDeviceCaps(D3DADAPTER_DEFAULT, D3DDEVTYPE_HAL, &caps)))
    {
        errors->Add("GetDeviceCaps failed");
        return;
    }

    // If the hardware lacks vertex processing, we fall back on
    // software vertex shading, so must override the relevant hardware caps.
    if (!(caps.DevCaps & D3DDEVCAPS_HWTRANSFORMANDLIGHT))
    {
        caps.VertexShaderVersion = D3DVS_VERSION(2, 0);

        caps.DeclTypes = D3DDTCAPS_UBYTE4 |
                         D3DDTCAPS_UBYTE4N |
                         D3DDTCAPS_SHORT2N |
                         D3DDTCAPS_SHORT4N;
    }

    // Check the shader version.
    if ((caps.VertexShaderVersion & 0xFFFF) < profileCapabilities->VertexShaderVersion)
    {
        errors->Add("VertexShaderVersion = " + FormatShaderVersion(caps.VertexShaderVersion));
    }

    if ((caps.PixelShaderVersion & 0xFFFF) < profileCapabilities->PixelShaderVersion)
    {
        errors->Add("PixelShaderVersion = " + FormatShaderVersion(caps.PixelShaderVersion));
    }

    // Check basic rendering caps.
    CHECK_LIMIT(MaxPrimitiveCount, profileCapabilities->MaxPrimitiveCount);
    CHECK_LIMIT(MaxStreams, profileCapabilities->MaxVertexStreams);
    CHECK_LIMIT(MaxStreamStride, profileCapabilities->MaxStreamStride);
    CHECK_LIMIT(MaxVertexIndex, profileCapabilities->IndexElementSize32 ? 16777214 : 65534);

    ENSURE_CAP(DevCaps2, D3DDEVCAPS2_CAN_STRETCHRECT_FROM_TEXTURES);
    ENSURE_CAP(DevCaps2, D3DDEVCAPS2_STREAMOFFSET);

    ENSURE_CAP(RasterCaps, D3DPRASTERCAPS_DEPTHBIAS);
    ENSURE_CAP(RasterCaps, D3DPRASTERCAPS_MIPMAPLODBIAS);
    ENSURE_CAP(RasterCaps, D3DPRASTERCAPS_SCISSORTEST);
    ENSURE_CAP(RasterCaps, D3DPRASTERCAPS_SLOPESCALEDEPTHBIAS);
    
    // Ideally we would like to check D3DPRASTERCAPS_ZTEST,
    // but some drivers incorrectly don't report it.

    ENSURE_CAP(ShadeCaps, D3DPSHADECAPS_COLORGOURAUDRGB);
    ENSURE_CAP(ShadeCaps, D3DPSHADECAPS_ALPHAGOURAUDBLEND);

    ENSURE_CAP(PrimitiveMiscCaps, D3DPMISCCAPS_MASKZ);
    ENSURE_CAP(PrimitiveMiscCaps, D3DPMISCCAPS_CULLNONE);
    ENSURE_CAP(PrimitiveMiscCaps, D3DPMISCCAPS_CULLCW);
    ENSURE_CAP(PrimitiveMiscCaps, D3DPMISCCAPS_CULLCCW);
    ENSURE_CAP(PrimitiveMiscCaps, D3DPMISCCAPS_COLORWRITEENABLE);
    ENSURE_CAP(PrimitiveMiscCaps, D3DPMISCCAPS_BLENDOP);

    ENSURE_CAP(LineCaps, D3DLINECAPS_BLEND);
    ENSURE_CAP(LineCaps, D3DLINECAPS_TEXTURE);
    ENSURE_CAP(LineCaps, D3DLINECAPS_ZTEST);

    // Check depth/stencil buffer caps.
    ENSURE_CAP(ZCmpCaps, D3DPCMPCAPS_ALWAYS);
    ENSURE_CAP(ZCmpCaps, D3DPCMPCAPS_EQUAL);
    ENSURE_CAP(ZCmpCaps, D3DPCMPCAPS_GREATER);
    ENSURE_CAP(ZCmpCaps, D3DPCMPCAPS_GREATEREQUAL);
    ENSURE_CAP(ZCmpCaps, D3DPCMPCAPS_LESS);
    ENSURE_CAP(ZCmpCaps, D3DPCMPCAPS_LESSEQUAL);
    ENSURE_CAP(ZCmpCaps, D3DPCMPCAPS_NEVER);
    ENSURE_CAP(ZCmpCaps, D3DPCMPCAPS_NOTEQUAL);

    ENSURE_CAP(StencilCaps, D3DSTENCILCAPS_KEEP);
    ENSURE_CAP(StencilCaps, D3DSTENCILCAPS_ZERO);
    ENSURE_CAP(StencilCaps, D3DSTENCILCAPS_REPLACE);
    ENSURE_CAP(StencilCaps, D3DSTENCILCAPS_INCRSAT);
    ENSURE_CAP(StencilCaps, D3DSTENCILCAPS_DECRSAT);
    ENSURE_CAP(StencilCaps, D3DSTENCILCAPS_INVERT);
    ENSURE_CAP(StencilCaps, D3DSTENCILCAPS_INCR);
    ENSURE_CAP(StencilCaps, D3DSTENCILCAPS_DECR);
    ENSURE_CAP(StencilCaps, D3DSTENCILCAPS_TWOSIDED);

    // Check blending caps.
    ENSURE_CAP(SrcBlendCaps, D3DPBLENDCAPS_BLENDFACTOR);
    ENSURE_CAP(SrcBlendCaps, D3DPBLENDCAPS_DESTALPHA);
    ENSURE_CAP(SrcBlendCaps, D3DPBLENDCAPS_DESTCOLOR);
    ENSURE_CAP(SrcBlendCaps, D3DPBLENDCAPS_INVDESTALPHA);
    ENSURE_CAP(SrcBlendCaps, D3DPBLENDCAPS_INVDESTCOLOR);
    ENSURE_CAP(SrcBlendCaps, D3DPBLENDCAPS_INVSRCALPHA);
    ENSURE_CAP(SrcBlendCaps, D3DPBLENDCAPS_INVSRCCOLOR);
    ENSURE_CAP(SrcBlendCaps, D3DPBLENDCAPS_ONE);
    ENSURE_CAP(SrcBlendCaps, D3DPBLENDCAPS_SRCALPHA);
    ENSURE_CAP(SrcBlendCaps, D3DPBLENDCAPS_SRCALPHASAT);
    ENSURE_CAP(SrcBlendCaps, D3DPBLENDCAPS_SRCCOLOR);
    ENSURE_CAP(SrcBlendCaps, D3DPBLENDCAPS_ZERO);

    ENSURE_CAP(DestBlendCaps, D3DPBLENDCAPS_BLENDFACTOR);
    ENSURE_CAP(DestBlendCaps, D3DPBLENDCAPS_DESTALPHA);
    ENSURE_CAP(DestBlendCaps, D3DPBLENDCAPS_DESTCOLOR);
    ENSURE_CAP(DestBlendCaps, D3DPBLENDCAPS_INVDESTALPHA);
    ENSURE_CAP(DestBlendCaps, D3DPBLENDCAPS_INVDESTCOLOR);
    ENSURE_CAP(DestBlendCaps, D3DPBLENDCAPS_INVSRCALPHA);
    ENSURE_CAP(DestBlendCaps, D3DPBLENDCAPS_INVSRCCOLOR);
    ENSURE_CAP(DestBlendCaps, D3DPBLENDCAPS_ONE);
    ENSURE_CAP(DestBlendCaps, D3DPBLENDCAPS_SRCALPHA);
    ENSURE_CAP(DestBlendCaps, D3DPBLENDCAPS_SRCCOLOR);
    ENSURE_CAP(DestBlendCaps, D3DPBLENDCAPS_ZERO);

    if (profileCapabilities->DestBlendSrcAlphaSat)
    {
        ENSURE_CAP(DestBlendCaps, D3DPBLENDCAPS_SRCALPHASAT);
    }

    if (profileCapabilities->SeparateAlphaBlend)
    {
        ENSURE_CAP(PrimitiveMiscCaps, D3DPMISCCAPS_SEPARATEALPHABLEND);
    }

    // Check multiple rendertargets.
    CHECK_LIMIT(NumSimultaneousRTs, profileCapabilities->MaxRenderTargets);
    
    if (profileCapabilities->MaxRenderTargets > 1)
    {
        ENSURE_CAP(PrimitiveMiscCaps, D3DPMISCCAPS_INDEPENDENTWRITEMASKS);
        ENSURE_CAP(PrimitiveMiscCaps, D3DPMISCCAPS_MRTPOSTPIXELSHADERBLENDING);
    }

    // Check texturing abilities.
    CHECK_LIMIT(MaxTextureWidth, profileCapabilities->MaxTextureSize);
    CHECK_LIMIT(MaxTextureHeight, profileCapabilities->MaxTextureSize);
    
    // Ideally we would like to check MaxCubeSize, but for some reason that isn't part of D3DCAPS9?

    if (caps.MaxTextureAspectRatio > 0)
    {
        // Only check this if MaxTextureAspectRatio > 0, because some drivers erroneously leave this blank.
        CHECK_LIMIT(MaxTextureAspectRatio, profileCapabilities->MaxTextureAspectRatio);
    }
        
    ENSURE_CAP(TextureCaps, D3DPTEXTURECAPS_ALPHA);
    ENSURE_CAP(TextureCaps, D3DPTEXTURECAPS_MIPMAP);
    ENSURE_CAP(TextureCaps, D3DPTEXTURECAPS_CUBEMAP);
    ENSURE_CAP(TextureCaps, D3DPTEXTURECAPS_MIPCUBEMAP);
    ENSURE_CAP(TextureCaps, D3DPTEXTURECAPS_PERSPECTIVE);
    REJECT_CAP(TextureCaps, D3DPTEXTURECAPS_SQUAREONLY);

    ENSURE_CAP(TextureAddressCaps, D3DPTADDRESSCAPS_CLAMP);
    ENSURE_CAP(TextureAddressCaps, D3DPTADDRESSCAPS_WRAP);
    ENSURE_CAP(TextureAddressCaps, D3DPTADDRESSCAPS_MIRROR);
    ENSURE_CAP(TextureAddressCaps, D3DPTADDRESSCAPS_INDEPENDENTUV);

    ENSURE_CAP(TextureFilterCaps, D3DPTFILTERCAPS_MAGFPOINT);
    ENSURE_CAP(TextureFilterCaps, D3DPTFILTERCAPS_MAGFLINEAR);
    ENSURE_CAP(TextureFilterCaps, D3DPTFILTERCAPS_MINFPOINT);
    ENSURE_CAP(TextureFilterCaps, D3DPTFILTERCAPS_MINFLINEAR);
    ENSURE_CAP(TextureFilterCaps, D3DPTFILTERCAPS_MIPFPOINT);
    ENSURE_CAP(TextureFilterCaps, D3DPTFILTERCAPS_MIPFLINEAR);

    ENSURE_CAP(CubeTextureFilterCaps, D3DPTFILTERCAPS_MAGFPOINT);
    ENSURE_CAP(CubeTextureFilterCaps, D3DPTFILTERCAPS_MAGFLINEAR);
    ENSURE_CAP(CubeTextureFilterCaps, D3DPTFILTERCAPS_MINFPOINT);
    ENSURE_CAP(CubeTextureFilterCaps, D3DPTFILTERCAPS_MINFLINEAR);
    ENSURE_CAP(CubeTextureFilterCaps, D3DPTFILTERCAPS_MIPFPOINT);
    ENSURE_CAP(CubeTextureFilterCaps, D3DPTFILTERCAPS_MIPFLINEAR);

    // Volume textures.
    if (profileCapabilities->MaxVolumeExtent > 0)
    {
        CHECK_LIMIT(MaxVolumeExtent, profileCapabilities->MaxVolumeExtent);

        ENSURE_CAP(TextureCaps, D3DPTEXTURECAPS_VOLUMEMAP);
        ENSURE_CAP(TextureCaps, D3DPTEXTURECAPS_MIPVOLUMEMAP);

        ENSURE_CAP(VolumeTextureAddressCaps, D3DPTADDRESSCAPS_CLAMP);
        ENSURE_CAP(VolumeTextureAddressCaps, D3DPTADDRESSCAPS_WRAP);
        ENSURE_CAP(VolumeTextureAddressCaps, D3DPTADDRESSCAPS_MIRROR);
        ENSURE_CAP(VolumeTextureAddressCaps, D3DPTADDRESSCAPS_INDEPENDENTUV);
        
        ENSURE_CAP(VolumeTextureFilterCaps, D3DPTFILTERCAPS_MAGFPOINT);
        ENSURE_CAP(VolumeTextureFilterCaps, D3DPTFILTERCAPS_MAGFLINEAR);
        ENSURE_CAP(VolumeTextureFilterCaps, D3DPTFILTERCAPS_MINFPOINT);
        ENSURE_CAP(VolumeTextureFilterCaps, D3DPTFILTERCAPS_MINFLINEAR);
        ENSURE_CAP(VolumeTextureFilterCaps, D3DPTFILTERCAPS_MIPFPOINT);
        ENSURE_CAP(VolumeTextureFilterCaps, D3DPTFILTERCAPS_MIPFLINEAR);
    }

    // Non-power-of-two textures.
    if (profileCapabilities->NonPow2Unconditional)
    {
        REJECT_CAP(TextureCaps, D3DPTEXTURECAPS_POW2);
    }
    else
    {
        // Conditional non-pow-2 support is expressed oddly in the caps.
        // If the POW2 flag is not set, we are always good. But when POW2
        // is set, we must make sure NONPOW2CONDITIONAL is also set.
        if (caps.TextureCaps & D3DPTEXTURECAPS_POW2)
        {
            ENSURE_CAP(TextureCaps, D3DPTEXTURECAPS_NONPOW2CONDITIONAL);
        }
    }
    
    if (profileCapabilities->NonPow2Cube)
    {
        REJECT_CAP(TextureCaps, D3DPTEXTURECAPS_CUBEMAP_POW2);
    }

    if (profileCapabilities->NonPow2Volume)
    {
        REJECT_CAP(TextureCaps, D3DPTEXTURECAPS_VOLUMEMAP_POW2);
    }

    // Vertex texturing.
    if (profileCapabilities->MaxVertexSamplers > 0)
    {
        ENSURE_CAP(VertexTextureFilterCaps, D3DPTFILTERCAPS_MAGFPOINT);
        ENSURE_CAP(VertexTextureFilterCaps, D3DPTFILTERCAPS_MINFPOINT);
    }

    // Vertex element formats.
    for each (VertexElementFormat format in profileCapabilities->ValidVertexFormats)
    {
        switch (format)
        {
            case VertexElementFormat::Color:            ENSURE_CAP(DeclTypes, D3DDTCAPS_UBYTE4N);   break;
            case VertexElementFormat::Byte4:            ENSURE_CAP(DeclTypes, D3DDTCAPS_UBYTE4);    break;
            case VertexElementFormat::NormalizedShort2: ENSURE_CAP(DeclTypes, D3DDTCAPS_SHORT2N);   break;
            case VertexElementFormat::NormalizedShort4: ENSURE_CAP(DeclTypes, D3DDTCAPS_SHORT4N);   break;
            case VertexElementFormat::HalfVector2:      ENSURE_CAP(DeclTypes, D3DDTCAPS_FLOAT16_2); break;
            case VertexElementFormat::HalfVector4:      ENSURE_CAP(DeclTypes, D3DDTCAPS_FLOAT16_4); break;
        }
    }

    // Texture formats.
    for each (SurfaceFormat format in profileCapabilities->ValidTextureFormats)
    {
        CheckTextureFormat(profileCapabilities, pD3D, D3DRTYPE_TEXTURE, format);
    }

    // Cubemap formats.
    for each (SurfaceFormat format in profileCapabilities->ValidCubeFormats)
    {
        CheckTextureFormat(profileCapabilities, pD3D, D3DRTYPE_CUBETEXTURE, format);
    }

    // Volume texture formats.
    for each (SurfaceFormat format in profileCapabilities->ValidVolumeFormats)
    {
        CheckTextureFormat(profileCapabilities, pD3D, D3DRTYPE_VOLUMETEXTURE, format);
    }

    // Vertex texture formats.
    for each (SurfaceFormat format in profileCapabilities->ValidVertexTextureFormats)
    {
        CheckVertexTextureFormat(profileCapabilities, pD3D, format);
    }

    // Rendertarget formats are mostly optional, but Color must always be available.
    CheckRenderTargetFormat(profileCapabilities, pD3D, SurfaceFormat::Color);

    // HiDef also requires HdrBlendable rendertargets.
    if (profileCapabilities->ValidTextureFormats->Contains(SurfaceFormat::HdrBlendable))
    {
        CheckRenderTargetFormat(profileCapabilities, pD3D, SurfaceFormat::HdrBlendable);
    }
}


void ProfileChecker::CheckTextureFormat(ProfileCapabilities^ profileCapabilities, IDirect3D9* pD3D, D3DRESOURCETYPE resourceType, SurfaceFormat format)
{
    D3DFORMAT d3dFormat = ConvertXnaFormatToDx(format);

    // Is this format supported?    
    if (FAILED(pD3D->CheckDeviceFormat(D3DADAPTER_DEFAULT, D3DDEVTYPE_HAL, IRRELEVANT_ADAPTER_FORMAT, 0, resourceType, d3dFormat)))
    {
        errors->Add(String::Format("No {0} format {1}", FormatResourceType(resourceType), format));
        return;
    }

    // Does this format support mipmapping?
    if (FAILED(pD3D->CheckDeviceFormat(D3DADAPTER_DEFAULT, D3DDEVTYPE_HAL, IRRELEVANT_ADAPTER_FORMAT, D3DUSAGE_QUERY_WRAPANDMIP, resourceType, d3dFormat)))
    {
        errors->Add(String::Format("No mipmapping for {0} format {1}", FormatResourceType(resourceType), format));
    }

    // Does this format support filtering?
    if (!profileCapabilities->InvalidFilterFormats->Contains(format))
    {
        if (FAILED(pD3D->CheckDeviceFormat(D3DADAPTER_DEFAULT, D3DDEVTYPE_HAL, IRRELEVANT_ADAPTER_FORMAT, D3DUSAGE_QUERY_FILTER, resourceType, d3dFormat)))
        {
            errors->Add(String::Format("No filtering for {0} format {1}", FormatResourceType(resourceType), format));
        }
    }
}


void ProfileChecker::CheckVertexTextureFormat(ProfileCapabilities^ profileCapabilities, IDirect3D9* pD3D, SurfaceFormat format)
{
    D3DFORMAT d3dFormat = ConvertXnaFormatToDx(format);

    // What usage flags does this profile require?
    UINT queryUsage = D3DUSAGE_QUERY_VERTEXTEXTURE | D3DUSAGE_QUERY_WRAPANDMIP;
    
    if (!profileCapabilities->InvalidFilterFormats->Contains(format))
    {
        queryUsage |= D3DUSAGE_QUERY_FILTER;
    }

    // 2D vertex texture?
    if (FAILED(pD3D->CheckDeviceFormat(D3DADAPTER_DEFAULT, D3DDEVTYPE_HAL, IRRELEVANT_ADAPTER_FORMAT, queryUsage, D3DRTYPE_TEXTURE, d3dFormat)))
    {
        errors->Add(String::Format("No vertex texture format {0}", format));
        return;
    }

    // Cubemap vertex texture?
    if (FAILED(pD3D->CheckDeviceFormat(D3DADAPTER_DEFAULT, D3DDEVTYPE_HAL, IRRELEVANT_ADAPTER_FORMAT, queryUsage, D3DRTYPE_CUBETEXTURE, d3dFormat)))
    {
        errors->Add(String::Format("No vertex cube texture format {0}", format));
    }

    // Volume vertex texture?
    if (FAILED(pD3D->CheckDeviceFormat(D3DADAPTER_DEFAULT, D3DDEVTYPE_HAL, IRRELEVANT_ADAPTER_FORMAT, queryUsage, D3DRTYPE_VOLUMETEXTURE, d3dFormat)))
    {
        errors->Add(String::Format("No vertex volume texture format {0}", format));
    }
}


void ProfileChecker::CheckRenderTargetFormat(ProfileCapabilities^ profileCapabilities, IDirect3D9* pD3D, SurfaceFormat format)
{
    UINT queryUsage = D3DUSAGE_RENDERTARGET;
    
    if (!profileCapabilities->InvalidBlendFormats->Contains(format))
    {
        queryUsage |= D3DUSAGE_QUERY_POSTPIXELSHADER_BLENDING;
    }

    if (FAILED(pD3D->CheckDeviceFormat(D3DADAPTER_DEFAULT, D3DDEVTYPE_HAL, IRRELEVANT_ADAPTER_FORMAT, queryUsage, D3DRTYPE_SURFACE, ConvertXnaFormatToDx(format))))
    {
        errors->Add(String::Format("No rendertarget format {0}", format));
    }
}


D3DFORMAT ProfileChecker::ConvertXnaFormatToDx(SurfaceFormat format)
{
    switch (format)
    {
        // Note: we map Color to D3DFMT_A8R8G8B8, which uses a BGRA byte ordering, 
        // even though our managed Color type is RGBA. We do this because D3DFMT_A8R8G8B8 
        // is universally supported on all DX9 parts, while D3DFMT_A8B8G8R8 (which 
        // properly matches our Color type) is not always available. The resulting
        // format mismatch is handled internally by the XNA Framework.
        
        case SurfaceFormat::Color:             return D3DFMT_A8R8G8B8;
        case SurfaceFormat::Bgr565:            return D3DFMT_R5G6B5;
        case SurfaceFormat::Bgra5551:          return D3DFMT_A1R5G5B5;
        case SurfaceFormat::Bgra4444:          return D3DFMT_A4R4G4B4;
        case SurfaceFormat::Dxt1:              return D3DFMT_DXT1;
        case SurfaceFormat::Dxt3:              return D3DFMT_DXT3;
        case SurfaceFormat::Dxt5:              return D3DFMT_DXT5;
        case SurfaceFormat::NormalizedByte2:   return D3DFMT_V8U8;
        case SurfaceFormat::NormalizedByte4:   return D3DFMT_Q8W8V8U8;
        case SurfaceFormat::Rgba1010102:       return D3DFMT_A2B10G10R10;
        case SurfaceFormat::Rg32:              return D3DFMT_G16R16;
        case SurfaceFormat::Rgba64:            return D3DFMT_A16B16G16R16;
        case SurfaceFormat::Alpha8:            return D3DFMT_A8;
        case SurfaceFormat::Single:            return D3DFMT_R32F;
        case SurfaceFormat::Vector2:           return D3DFMT_G32R32F;
        case SurfaceFormat::Vector4:           return D3DFMT_A32B32G32R32F;
        case SurfaceFormat::HalfSingle:        return D3DFMT_R16F;
        case SurfaceFormat::HalfVector2:       return D3DFMT_G16R16F;
        case SurfaceFormat::HalfVector4:       return D3DFMT_A16B16G16R16F;
        case SurfaceFormat::HdrBlendable:      return D3DFMT_A16B16G16R16F;
        default:                               return D3DFMT_UNKNOWN;
    }
}


// Converts a resource type enum to readable string format.
String^ ProfileChecker::FormatResourceType(D3DRESOURCETYPE resourceType)
{
    switch (resourceType)
    {
        case D3DRTYPE_TEXTURE:       return "texture";
        case D3DRTYPE_CUBETEXTURE:   return "cube texture";
        case D3DRTYPE_VOLUMETEXTURE: return "volume texture";

        default:
            throw gcnew ArgumentOutOfRangeException("resourceType");
    }
}


// Converts a shader version number to readable string format.
String^ ProfileChecker::FormatShaderVersion(unsigned shaderVersion)
{
    return String::Format("{0}.{1}", (shaderVersion >> 8) & 0xFF, shaderVersion & 0xFF);
}
