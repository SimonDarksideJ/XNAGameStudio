#pragma once

#include "GenericTypeReader.h"


class EnumReader : public GenericTypeReader
{
public:
    static wstring GenericTargetType() { return L"System.Enum"; }
    static wstring GenericReaderName() { return L"Microsoft.Xna.Framework.Content.EnumReader"; }

    virtual wstring TargetType() const { return GenericArgument(0); }

    virtual bool IsValueType() const { return true; }
    
    virtual void Read(ContentReader* reader);
};


class NullableReader : public GenericTypeReader
{
public:
    static wstring GenericTargetType() { return L"System.Nullable"; }
    static wstring GenericReaderName() { return L"Microsoft.Xna.Framework.Content.NullableReader"; }

    virtual bool IsValueType() const { return true; }
    
    virtual void Initialize(TypeReaderManager* typeReaderManager);
    virtual void Read(ContentReader* reader);

private:
    TypeReader* valueReader;
};


class ArrayReader : public GenericTypeReader
{
public:
    static wstring GenericTargetType() { return L"System.Array"; }
    static wstring GenericReaderName() { return L"Microsoft.Xna.Framework.Content.ArrayReader"; }

    virtual wstring TargetType() const { return GenericArgument(0) + L"[]"; }
    
    virtual void Initialize(TypeReaderManager* typeReaderManager);
    virtual void Read(ContentReader* reader);

private:
    TypeReader* elementReader;
};


class ListReader : public GenericTypeReader
{
public:
    static wstring GenericTargetType() { return L"System.Collections.Generic.List"; }
    static wstring GenericReaderName() { return L"Microsoft.Xna.Framework.Content.ListReader"; }
    
    virtual void Initialize(TypeReaderManager* typeReaderManager);
    virtual void Read(ContentReader* reader);

private:
    TypeReader* elementReader;
};


class DictionaryReader : public GenericTypeReader
{
public:
    static wstring GenericTargetType() { return L"System.Collections.Generic.Dictionary"; }
    static wstring GenericReaderName() { return L"Microsoft.Xna.Framework.Content.DictionaryReader"; }
    
    virtual void Initialize(TypeReaderManager* typeReaderManager);
    virtual void Read(ContentReader* reader);

private:
    TypeReader* keyReader;
    TypeReader* valueReader;
};


class TimeSpanReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"System.TimeSpan"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.TimeSpanReader"; }
    
    virtual bool IsValueType() const { return true; }

    virtual void Read(ContentReader* reader);
};


class DateTimeReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"System.DateTime"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.DateTimeReader"; }
    
    virtual bool IsValueType() const { return true; }

    virtual void Read(ContentReader* reader);
};


class DecimalReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"System.Decimal"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.DecimalReader"; }
    
    virtual bool IsValueType() const { return true; }

    virtual void Read(ContentReader* reader);
};


class ExternalReferenceReader : public TypeReader
{
public:
    virtual wstring TargetType() const { return L"ExternalReference"; }
    virtual wstring ReaderName() const { return L"Microsoft.Xna.Framework.Content.ExternalReferenceReader"; }
    
    virtual void Read(ContentReader* reader);
};


class ReflectiveReader : public GenericTypeReader
{
public:
    static wstring GenericTargetType() { return L"System.Object"; }
    static wstring GenericReaderName() { return L"Microsoft.Xna.Framework.Content.ReflectiveReader"; }

    virtual wstring TargetType() const { return GenericArgument(0); }

    virtual void Read(ContentReader* reader);
};
