#pragma once

#include "Logger.h"
#include "BinaryReader.h"
#include "TypeReaderManager.h"


// Parses an XNB file, calling into the appropriate TypeReader
// helper to read whatever type(s) of object it contains.
class ContentReader : public BinaryReader
{
public:
    ContentReader(FILE* file, TypeReaderManager* typeReaderManager);

    // Helper for printing out the file contents.
    Logger Log;

    // Parses the entire contents of an XNB file.
    void ReadXnb();

    // Reads a single polymorphic object from the current location.
    void ReadObject();

    // Reads either a raw value or polymorphic object, depending on whether the specified typeReader represents a value type.
    void ReadValueOrObject(TypeReader* typeReader);

    // Reads the typeId from the start of a polymorphic object, and looks up the appropriate TypeReader implementation.
    TypeReader* ReadTypeId();

    // Reads a typeId, and validates that it is the expected type.
    void ValidateTypeId(wstring const& expectedType);

    // Reads a shared resource ID, which indexes into the table of shared object instances that come after the primary asset.
    void ReadSharedResource();

private:
    // Reads the XNB file header (version number, size, etc.).
    uint32_t ReadHeader();

    // Reads the manifest of what types are contained in this XNB file.
    void ReadTypeManifest();

    // Manager provides reader implementations for all supported data types.
    TypeReaderManager* typeReaderManager;

    // Table of the readers used by this particular .xnb file.
    vector<TypeReader*> typeReaders;
};
