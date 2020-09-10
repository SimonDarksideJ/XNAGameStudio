#pragma once

#include "TypeReader.h"


class Vector2Reader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Vector2"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.Vector2Reader"; }
    
    virtual bool IsValueType() const { return true; }

    virtual void Read(ContentReader* reader);
};


class Vector3Reader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Vector3"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.Vector3Reader"; }
    
    virtual bool IsValueType() const { return true; }

    virtual void Read(ContentReader* reader);
};


class Vector4Reader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Vector4"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.Vector4Reader"; }
    
    virtual bool IsValueType() const { return true; }

    virtual void Read(ContentReader* reader);
};


class MatrixReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Matrix"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.MatrixReader"; }
    
    virtual bool IsValueType() const { return true; }

    virtual void Read(ContentReader* reader);
};


class QuaternionReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Quaternion"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.QuaternionReader"; }
    
    virtual bool IsValueType() const { return true; }

    virtual void Read(ContentReader* reader);
};


class ColorReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Color"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.ColorReader"; }
    
    virtual bool IsValueType() const { return true; }

    virtual void Read(ContentReader* reader);
};


class PlaneReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Plane"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.PlaneReader"; }
    
    virtual bool IsValueType() const { return true; }

    virtual void Read(ContentReader* reader);
};


class PointReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Point"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.PointReader"; }
    
    virtual bool IsValueType() const { return true; }

    virtual void Read(ContentReader* reader);
};


class RectangleReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Rectangle"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.RectangleReader"; }
    
    virtual bool IsValueType() const { return true; }

    virtual void Read(ContentReader* reader);
};


class BoundingBoxReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.BoundingBox"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.BoundingBoxReader"; }
    
    virtual bool IsValueType() const { return true; }

    virtual void Read(ContentReader* reader);
};


class BoundingSphereReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.BoundingSphere"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.BoundingSphereReader"; }
    
    virtual bool IsValueType() const { return true; }

    virtual void Read(ContentReader* reader);
};


class BoundingFrustumReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.BoundingFrustum"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.BoundingFrustumReader"; }
    
    virtual void Read(ContentReader* reader);
};


class RayReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Ray"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.RayReader"; }
    
    virtual bool IsValueType() const { return true; }

    virtual void Read(ContentReader* reader);
};


class CurveReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"Microsoft.Xna.Framework.Curve"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.CurveReader"; }
    
    virtual void Read(ContentReader* reader);
};
