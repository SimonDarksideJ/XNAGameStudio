//-----------------------------------------------------------------------------
// ProfileChecker.h
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#pragma once

namespace XnaGraphicsProfileChecker
{
    using namespace System;
    using namespace System::Collections::Generic;
    using namespace System::Collections::ObjectModel;
    using namespace Microsoft::Xna::Framework::Graphics;

    ref class ProfileCapabilities;


    // Checks whether the current graphics hardware meets XNA Framework profile requirements.
    ref class ProfileChecker
    {
    public:
        ProfileChecker(GraphicsProfile profile);


        // Does the current hardware support all the necessary features?
        property bool IsSupported
        {
            bool get() { return (errors->Count == 0); }
        };


        // If not, which features are missing?
        property ReadOnlyCollection<String^>^ Errors
        {
            ReadOnlyCollection<String^>^ get() { return gcnew ReadOnlyCollection<String^>(errors); }
        };


    private:
        List<String^>^ errors;

        void CheckProfileSupport(GraphicsProfile graphicsProfile, IDirect3D9* pD3D);
        void CheckTextureFormat(ProfileCapabilities^ profileCapabilities, IDirect3D9* pD3D, D3DRESOURCETYPE resourceType, SurfaceFormat format);
        void CheckVertexTextureFormat(ProfileCapabilities^ profileCapabilities, IDirect3D9* pD3D, SurfaceFormat format);
        void CheckRenderTargetFormat(ProfileCapabilities^ profileCapabilities, IDirect3D9* pD3D, SurfaceFormat format);

        static D3DFORMAT ConvertXnaFormatToDx(SurfaceFormat format);

        static String^ FormatResourceType(D3DRESOURCETYPE resourceType);
        static String^ FormatShaderVersion(unsigned shaderVersion);
    };
}
