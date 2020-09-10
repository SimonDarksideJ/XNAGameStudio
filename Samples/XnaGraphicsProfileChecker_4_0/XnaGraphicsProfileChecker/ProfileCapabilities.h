//-----------------------------------------------------------------------------
// ProfileCapabilities.h
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#pragma once

namespace XnaGraphicsProfileChecker
{
    using namespace System::Collections::ObjectModel;
    using namespace Microsoft::Xna::Framework::Graphics;


    // Describes the hardware requirements of an XNA Framework graphics profile.
    ref class ProfileCapabilities
    {
    public:
        ProfileCapabilities(GraphicsProfile graphicsProfile);

        initonly unsigned VertexShaderVersion;
        initonly unsigned PixelShaderVersion;
        
        initonly bool SeparateAlphaBlend;
        initonly bool DestBlendSrcAlphaSat;

        initonly int MaxPrimitiveCount;
        initonly bool IndexElementSize32;
        initonly int MaxVertexStreams;
        initonly int MaxStreamStride;

        initonly int MaxTextureSize;
        initonly int MaxCubeSize;
        initonly int MaxVolumeExtent;
        initonly int MaxTextureAspectRatio;
        initonly int MaxVertexSamplers;
        initonly int MaxRenderTargets;
        
        initonly bool NonPow2Unconditional;
        initonly bool NonPow2Cube;
        initonly bool NonPow2Volume;

        initonly ReadOnlyCollection<SurfaceFormat>^ ValidTextureFormats;
        initonly ReadOnlyCollection<SurfaceFormat>^ ValidCubeFormats;
        initonly ReadOnlyCollection<SurfaceFormat>^ ValidVolumeFormats;
        initonly ReadOnlyCollection<SurfaceFormat>^ ValidVertexTextureFormats;
        initonly ReadOnlyCollection<SurfaceFormat>^ InvalidFilterFormats;
        initonly ReadOnlyCollection<SurfaceFormat>^ InvalidBlendFormats;
        initonly ReadOnlyCollection<VertexElementFormat>^ ValidVertexFormats;
    };
}
