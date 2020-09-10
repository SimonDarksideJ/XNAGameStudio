//-----------------------------------------------------------------------------
// ProfileCapabilities.cpp
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#include "stdafx.h"
#include "ProfileCapabilities.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace XnaGraphicsProfileChecker;


#define STANDARD_TEXTURE_FORMATS \
    SurfaceFormat::Color, \
    SurfaceFormat::Bgr565, \
    SurfaceFormat::Bgra5551, \
    SurfaceFormat::Bgra4444
        
#define COMPRESSED_TEXTURE_FORMATS \
    SurfaceFormat::Dxt1, \
    SurfaceFormat::Dxt3, \
    SurfaceFormat::Dxt5

#define SIGNED_TEXTURE_FORMATS \
    SurfaceFormat::NormalizedByte2, \
    SurfaceFormat::NormalizedByte4

#define HIDEF_TEXTURE_FORMATS \
    SurfaceFormat::Rgba1010102, \
    SurfaceFormat::Rg32, \
    SurfaceFormat::Rgba64, \
    SurfaceFormat::Alpha8
        
#define STANDARD_FLOAT_TEXTURE_FORMATS \
    SurfaceFormat::Single, \
    SurfaceFormat::Vector2, \
    SurfaceFormat::Vector4, \
    SurfaceFormat::HalfSingle, \
    SurfaceFormat::HalfVector2, \
    SurfaceFormat::HalfVector4

#define FLOAT_TEXTURE_FORMATS \
    STANDARD_FLOAT_TEXTURE_FORMATS, \
    SurfaceFormat::HdrBlendable

#define STANDARD_DEPTH_FORMATS \
    DepthFormat::Depth16, \
    DepthFormat::Depth24, \
    DepthFormat::Depth24Stencil8

#define STANDARD_VERTEX_FORMATS \
    VertexElementFormat::Color, \
    VertexElementFormat::Single, \
    VertexElementFormat::Vector2, \
    VertexElementFormat::Vector3, \
    VertexElementFormat::Vector4, \
    VertexElementFormat::Byte4, \
    VertexElementFormat::Short2, \
    VertexElementFormat::Short4, \
    VertexElementFormat::NormalizedShort2, \
    VertexElementFormat::NormalizedShort4

#define HIDEF_VERTEX_FORMATS \
    VertexElementFormat::HalfVector2, \
    VertexElementFormat::HalfVector4


// Helper for filling in lists of enum values.
template<typename T>
ReadOnlyCollection<T>^ MakeList(... array<T>^ values)
{
    return gcnew ReadOnlyCollection<T>(gcnew List<T>(values));
}


ProfileCapabilities::ProfileCapabilities(GraphicsProfile graphicsProfile)
{
    switch (graphicsProfile)
    {
        case GraphicsProfile::Reach:
            // Fill in the Reach profile requirements.
            VertexShaderVersion = 0x200;
            PixelShaderVersion = 0x200;
            
            SeparateAlphaBlend = false;
            DestBlendSrcAlphaSat = false;

            MaxPrimitiveCount = 65535;
            IndexElementSize32 = false;
            MaxVertexStreams = 16;
            MaxStreamStride = 255;
            
            MaxTextureSize = 2048;
            MaxCubeSize = 512;
            MaxVolumeExtent = 0;
            MaxTextureAspectRatio = 2048;
            MaxVertexSamplers = 0;
            MaxRenderTargets = 1;
            
            NonPow2Unconditional = false;
            NonPow2Cube = false;
            NonPow2Volume = false;

            ValidTextureFormats       = MakeList(STANDARD_TEXTURE_FORMATS, COMPRESSED_TEXTURE_FORMATS, SIGNED_TEXTURE_FORMATS);
            ValidCubeFormats          = MakeList(STANDARD_TEXTURE_FORMATS, COMPRESSED_TEXTURE_FORMATS);
            ValidVolumeFormats        = MakeList<SurfaceFormat>();
            ValidVertexTextureFormats = MakeList<SurfaceFormat>();
            InvalidFilterFormats      = MakeList<SurfaceFormat>();
            InvalidBlendFormats       = MakeList<SurfaceFormat>();
            ValidVertexFormats        = MakeList(STANDARD_VERTEX_FORMATS);
            break;
                
        case GraphicsProfile::HiDef:
            // Fill in the HiDef profile requirements.
            VertexShaderVersion = 0x300;
            PixelShaderVersion = 0x300;
            
            SeparateAlphaBlend = true;
            DestBlendSrcAlphaSat = true;
            
            MaxPrimitiveCount = 1048575;
            IndexElementSize32 = true;
            MaxVertexStreams = 16;
            MaxStreamStride = 255;
            
            MaxTextureSize = 4096;
            MaxCubeSize = 4096;
            MaxVolumeExtent = 256;
            MaxTextureAspectRatio = 2048;
            MaxVertexSamplers = 4;
            MaxRenderTargets = 4;

            NonPow2Unconditional = true;
            NonPow2Cube = true;
            NonPow2Volume = true;

            ValidTextureFormats       = MakeList(STANDARD_TEXTURE_FORMATS, COMPRESSED_TEXTURE_FORMATS, SIGNED_TEXTURE_FORMATS, HIDEF_TEXTURE_FORMATS, FLOAT_TEXTURE_FORMATS);
            ValidCubeFormats          = MakeList(STANDARD_TEXTURE_FORMATS, COMPRESSED_TEXTURE_FORMATS, HIDEF_TEXTURE_FORMATS, FLOAT_TEXTURE_FORMATS);
            ValidVolumeFormats        = MakeList(STANDARD_TEXTURE_FORMATS, HIDEF_TEXTURE_FORMATS, FLOAT_TEXTURE_FORMATS);
            ValidVertexTextureFormats = MakeList(FLOAT_TEXTURE_FORMATS);
            InvalidFilterFormats      = MakeList(FLOAT_TEXTURE_FORMATS);
            InvalidBlendFormats       = MakeList(STANDARD_FLOAT_TEXTURE_FORMATS);
            ValidVertexFormats        = MakeList(STANDARD_VERTEX_FORMATS, HIDEF_VERTEX_FORMATS);
            break;
                
        default:
            throw gcnew ArgumentOutOfRangeException("graphicsProfile");
    }
}
