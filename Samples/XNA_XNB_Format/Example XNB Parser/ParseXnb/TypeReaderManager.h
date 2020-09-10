#pragma once

#include "TypeReader.h"
#include "GenericTypeReader.h"


// Keeps track of all the available TypeReader implementations.
class TypeReaderManager
{
public:
    virtual ~TypeReaderManager();

    void RegisterStandardTypes();

    TypeReader* GetByReaderName(wstring const& readerName);
    TypeReader* GetByTargetType(wstring const& targetType);


    template<typename T> void RegisterTypeReader()
    {
        static_assert(!is_base_of<GenericTypeReader, T>::value, "Generic reader types should use RegisterGenericTypeReader.");

        typeReaders.push_back(new T);
    }


    template<typename T> void RegisterGenericReader()
    {
        genericReaders.push_back(new GenericTypeReaderFactoryT<T>);
    }


private:
    static wstring StripAssemblyVersion(wstring typeName);
    static bool SplitGenericTypeName(wstring const& typeName, wstring* genericName, vector<wstring>* genericArguments);

    vector<TypeReader*> typeReaders;
    vector<GenericTypeReaderFactory*> genericReaders;
};
