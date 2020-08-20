#pragma once

#include "TypeReader.h"


class TextureReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Graphics.Texture"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.TextureReader"; }

    virtual void Read(ContentReader* reader);
};


class Texture2DReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Graphics.Texture2D"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.Texture2DReader"; }

    virtual void Read(ContentReader* reader);
};


class Texture3DReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Graphics.Texture3D"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.Texture3DReader"; }

    virtual void Read(ContentReader* reader);
};


class TextureCubeReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Graphics.TextureCube"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.TextureCubeReader"; }

    virtual void Read(ContentReader* reader);
};


class IndexBufferReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Graphics.IndexBuffer"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.IndexBufferReader"; }

    virtual void Read(ContentReader* reader);
};


class VertexBufferReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Graphics.VertexBuffer"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.VertexBufferReader"; }

    virtual void Read(ContentReader* reader);
};


class VertexDeclarationReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Graphics.VertexDeclaration"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.VertexDeclarationReader"; }

    virtual void Read(ContentReader* reader);
};


class EffectReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Graphics.Effect"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.EffectReader"; }

    virtual void Read(ContentReader* reader);
};


class EffectMaterialReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Graphics.EffectMaterial"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.EffectMaterialReader"; }

    virtual void Read(ContentReader* reader);
};


class BasicEffectReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Graphics.BasicEffect"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.BasicEffectReader"; }

    virtual void Read(ContentReader* reader);
};


class AlphaTestEffectReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Graphics.AlphaTestEffect"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.AlphaTestEffectReader"; }

    virtual void Read(ContentReader* reader);
};


class DualTextureEffectReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Graphics.DualTextureEffect"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.DualTextureEffectReader"; }

    virtual void Read(ContentReader* reader);
};


class EnvironmentMapEffectReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Graphics.EnvironmentMapEffect"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.EnvironmentMapEffectReader"; }

    virtual void Read(ContentReader* reader);
};


class SkinnedEffectReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Graphics.SkinnedEffect"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.SkinnedEffectReader"; }

    virtual void Read(ContentReader* reader);
};


class SpriteFontReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Graphics.SpriteFont"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.SpriteFontReader"; }

    virtual void Read(ContentReader* reader);
};


class ModelReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Graphics.Model"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.ModelReader"; }

    virtual void Read(ContentReader* reader);
};
