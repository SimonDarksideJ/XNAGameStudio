#pragma once

#include "TypeReader.h"


class ByteReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"System.Byte"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.ByteReader"; }
    
    virtual bool IsValueType() const { return true; }

    virtual void Read(ContentReader* reader);
};


class SByteReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"System.SByte"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.SByteReader"; }
    
    virtual bool IsValueType() const { return true; }

    virtual void Read(ContentReader* reader);
};


class Int16Reader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"System.Int16"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.Int16Reader"; }
    
    virtual bool IsValueType() const { return true; }

    virtual void Read(ContentReader* reader);
};


class UInt16Reader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"System.UInt16"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.UInt16Reader"; }
    
    virtual bool IsValueType() const { return true; }

    virtual void Read(ContentReader* reader);
};


class Int32Reader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"System.Int32"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.Int32Reader"; }
    
    virtual bool IsValueType() const { return true; }

    virtual void Read(ContentReader* reader);
};


class UInt32Reader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"System.UInt32"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.UInt32Reader"; }
    
    virtual bool IsValueType() const { return true; }

    virtual void Read(ContentReader* reader);
};


class Int64Reader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"System.Int64"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.Int64Reader"; }
    
    virtual bool IsValueType() const { return true; }

    virtual void Read(ContentReader* reader);
};


class UInt64Reader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"System.UInt64"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.UInt64Reader"; }
    
    virtual bool IsValueType() const { return true; }

    virtual void Read(ContentReader* reader);
};


class SingleReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"System.Single"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.SingleReader"; }
    
    virtual bool IsValueType() const { return true; }

    virtual void Read(ContentReader* reader);
};


class DoubleReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"System.Double"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.DoubleReader"; }
    
    virtual bool IsValueType() const { return true; }

    virtual void Read(ContentReader* reader);
};


class BooleanReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"System.Boolean"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.BooleanReader"; }
    
    virtual bool IsValueType() const { return true; }

    virtual void Read(ContentReader* reader);
};


class CharReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"System.Char"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.CharReader"; }
    
    virtual bool IsValueType() const { return true; }

    virtual void Read(ContentReader* reader);
};


class StringReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"System.String"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.StringReader"; }
    
    virtual void Read(ContentReader* reader);
};


class ObjectReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"System.Object"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.ObjectReader"; }
    
    virtual void Read(ContentReader* reader);
};
