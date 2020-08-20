#pragma once


// Each TypeReader subclass is responsible for reading a specific type of object from XNB format.
class TypeReader
{
public:
    virtual ~TypeReader() { }

    virtual wstring TargetType() const = 0;
    virtual wstring ReaderName() const = 0;
    
    virtual bool IsValueType() const { return false; }

    virtual void Initialize(class TypeReaderManager*) { }

    virtual void Read(class ContentReader* reader) = 0;
};
