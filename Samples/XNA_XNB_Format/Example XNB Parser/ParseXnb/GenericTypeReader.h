#pragma once

#include "TypeReader.h"


// Extend TypeReader to support generic specialization, for types such as List<T>.
class GenericTypeReader : public TypeReader
{
public:
    void Specialize(wstring const& targetType, wstring const& readerName, vector<wstring> const& genericArguments);

    virtual wstring TargetType() const { return targetType; }
    virtual wstring ReaderName() const { return readerName; }

    virtual wstring GenericArgument(int i) const { return genericArguments[i]; }

private:
    wstring targetType;
    wstring readerName;

    vector<wstring> genericArguments;
};


// Factory represents an open generic reader for a whole category of types,
// eg. List<>. This can be used to create specialized GenericTypeReader
// instances for specific type parameters such as List<int> or List<string>.
class GenericTypeReaderFactory
{
public:
    virtual ~GenericTypeReaderFactory() { }

    virtual wstring GenericTargetType() const = 0;
    virtual wstring GenericReaderName() const = 0;

    GenericTypeReader* CreateTypeReader(vector<wstring> const& genericArguments);

protected:
    virtual GenericTypeReader* CreateTypeReader() = 0;
};


// Customize GenericTypeReaderFactory to instantiate a specific GenericTypeReader subclass.
template<typename T> class GenericTypeReaderFactoryT : public GenericTypeReaderFactory
{
public:
    virtual wstring GenericTargetType() const { return T::GenericTargetType(); }
    virtual wstring GenericReaderName() const { return T::GenericReaderName(); }


protected:
    virtual GenericTypeReader* CreateTypeReader()
    {
        return new T;
    }
};
