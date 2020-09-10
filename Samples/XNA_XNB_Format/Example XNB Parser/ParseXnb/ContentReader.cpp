#include "stdafx.h"
#include "ContentReader.h"


ContentReader::ContentReader(FILE* file, TypeReaderManager* typeReaderManager)
  : BinaryReader(file),
    typeReaderManager(typeReaderManager)
{
}


// Parses the entire contents of an XNB file.
void ContentReader::ReadXnb()
{
    // Read the XNB header.
    uint32_t endPosition = ReadHeader();

    ReadTypeManifest();

    uint32_t sharedResourceCount = Read7BitEncodedInt();

    // Read the primary asset data.
    Log.WriteLine("Asset:");

    ReadObject();

    // Read any shared resource instances.
    for (uint32_t i = 0 ; i < sharedResourceCount; i++)
    {
        Log.WriteLine("Shared resource %d:", i);
        
        ReadObject();
    }

    // Make sure we read the amount of data that the file header said we should.
    if (FilePosition() != endPosition)
    {
        throw exception("End position does not match XNB header: unexpected amount of data was read.");
    }
}


// Reads the XNB file header (version number, size, etc.).
uint32_t ContentReader::ReadHeader()
{
    uint32_t startPosition = FilePosition();

    // Magic number.
    uint8_t magic1 = ReadByte();
    uint8_t magic2 = ReadByte();
    uint8_t magic3 = ReadByte();

    if (magic1 != 'X' ||
        magic2 != 'N' ||
        magic3 != 'B')
    {
        throw exception("Not an XNB file.");
    }

    // Target platform.
    uint8_t targetPlatform = ReadByte();

    switch (targetPlatform)
    {
        case 'w': Log.WriteLine("Target platform: Windows");                   break;
        case 'm': Log.WriteLine("Target platform: Windows Phone");             break;
        case 'x': Log.WriteLine("Target platform: Xbox 360");                  break;
        default:  Log.WriteLine("Unknown target platform %d", targetPlatform); break;
    }

    // Format version.
    uint8_t formatVersion = ReadByte();

    if (formatVersion != 5)
    {
        Log.WriteLine("Warning: not an XNA Game Studio version 4.0 XNB file. Parsing may fail unexpectedly.");
    }

    // Flags.
    uint8_t flags = ReadByte();

    if (flags & 1)
    {
        Log.WriteLine("Graphics profile: HiDef");
    }
    else
    {
        Log.WriteLine("Graphics profile: Reach");
    }

    bool isCompressed = (flags & 0x80) != 0;

    // File size.
    uint32_t sizeOnDisk = ReadUInt32();

    if (startPosition + sizeOnDisk > FileSize())
    {
        throw exception("XNB file has been truncated.");
    }

    if (isCompressed)
    {
        uint32_t decompressedSize = ReadUInt32();
        uint32_t compressedSize = startPosition + sizeOnDisk - FilePosition();

        Log.WriteLine("%d bytes of asset data are compressed into %d", decompressedSize, compressedSize);

        throw exception("Don't support reading the contents of compressed XNB files.");
    }

    return startPosition + sizeOnDisk;
}


// Reads the manifest of what types are contained in this XNB file.
void ContentReader::ReadTypeManifest()
{
    Log.WriteLine("Type readers:");
    Log.Indent();

    // How many type readers does this .xnb use?
    uint32_t typeReaderCount = Read7BitEncodedInt();

    typeReaders.clear();

    for (uint32_t i = 0; i < typeReaderCount; i++)
    {
        // Read the type reader metadata.
        wstring readerName = ReadString();
        int32_t readerVersion = ReadInt32();

        Log.WriteLine("%S (version %d)", readerName.c_str(), readerVersion);

        // Look up and store this type reader implementation class.
        TypeReader* reader = typeReaderManager->GetByReaderName(readerName);

        typeReaders.push_back(reader);
    }

    // Initialize the readers in a separate pass after they are all registered, in case there are
    // circular dependencies between them (eg. an array of classes which themselves contain arrays).
    for each (TypeReader* reader in typeReaders)
    {
        reader->Initialize(typeReaderManager);
    }

    Log.Unindent();
}


// Reads a single polymorphic object from the current location.
void ContentReader::ReadObject()
{
    Log.Indent();

    // What type of object is this?
    TypeReader* typeReader = ReadTypeId();
    
    if (typeReader)
    {
        Log.WriteLine("Type: %S", typeReader->TargetType().c_str());

        // Call into the appropriate TypeReader to parse the object data.
        typeReader->Read(this);
    }
    else
    {
        Log.WriteLine("null");
    }

    Log.Unindent();
}


// Reads either a raw value or polymorphic object, depending on whether the specified typeReader represents a value type.
void ContentReader::ReadValueOrObject(TypeReader* typeReader)
{
    if (typeReader->IsValueType())
    {
        // Read a value type.
        Log.Indent();

        typeReader->Read(this);

        Log.Unindent();
    }
    else
    {
        // Read a reference type.
        ReadObject();
    }
}


// Reads the typeId from the start of a polymorphic object, and looks up the appropriate TypeReader implementation.
TypeReader* ContentReader::ReadTypeId()
{
    uint32_t typeId = Read7BitEncodedInt();

    if (typeId > 0)
    {
        // Look up the reader for this type of object.
        typeId--;

        if (typeId >= typeReaders.size())
        {
            throw exception("Invalid XNB file: typeId is out of range.");
        }

        return typeReaders[typeId];
    }
    else
    {
        // A zero typeId indicates a null object.
        return nullptr;
    }
}


// Reads a typeId, and validates that it is the expected type.
void ContentReader::ValidateTypeId(wstring const& expectedType)
{
    TypeReader* reader = ReadTypeId();

    if (!reader || reader->TargetType() != expectedType)
    {
        throw exception("Invalid XNB file: got an unexpected typeId.");
    }
}


// Reads a shared resource ID, which indexes into the table of shared object instances that come after the primary asset.
void ContentReader::ReadSharedResource()
{
    uint32_t resourceId = Read7BitEncodedInt();

    if (resourceId)
    {
        Log.WriteLine("shared resource #%u", resourceId - 1);
    }
    else
    {
        Log.WriteLine("null");
    }
}
