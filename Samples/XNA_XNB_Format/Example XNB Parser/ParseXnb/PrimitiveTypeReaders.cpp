#include "stdafx.h"
#include "ContentReader.h"
#include "PrimitiveTypeReaders.h"


void ByteReader::Read(ContentReader* reader)
{
    reader->Log.WriteLine("%u", (uint32_t)reader->ReadByte());
}


void SByteReader::Read(ContentReader* reader)
{
    reader->Log.WriteLine("%d", (int32_t)reader->ReadSByte());
}


void Int16Reader::Read(ContentReader* reader)
{
    reader->Log.WriteLine("%hd", reader->ReadInt16());
}


void UInt16Reader::Read(ContentReader* reader)
{
    reader->Log.WriteLine("%hu", reader->ReadUInt16());
}


void Int32Reader::Read(ContentReader* reader)
{
    reader->Log.WriteLine("%d", reader->ReadInt32());
}


void UInt32Reader::Read(ContentReader* reader)
{
    reader->Log.WriteLine("%u", reader->ReadUInt32());
}


void Int64Reader::Read(ContentReader* reader)
{
    reader->Log.WriteLine("%lld", reader->ReadInt64());
}


void UInt64Reader::Read(ContentReader* reader)
{
    reader->Log.WriteLine("%llu", reader->ReadUInt64());
}


void SingleReader::Read(ContentReader* reader)
{
    reader->Log.WriteLine("%g", reader->ReadSingle());
}


void DoubleReader::Read(ContentReader* reader)
{
    reader->Log.WriteLine("%g", reader->ReadDouble());
}


void BooleanReader::Read(ContentReader* reader)
{
    reader->Log.WriteLine(reader->ReadBoolean() ? "true" : "false");
}


void CharReader::Read(ContentReader* reader)
{
    wchar_t value = reader->ReadChar();

    // Take care not to accidentally print out control character codes!
    if (iswprint(value))
    {
        reader->Log.WriteLine("U+%04hX '%C'", value, value);
    }
    else
    {
        reader->Log.WriteLine("U+%04hX", value);
    }
}


void StringReader::Read(ContentReader* reader)
{
    wstring value = reader->ReadString();

    reader->Log.Write("'");

    for each (wchar_t ch in value)
    {
        // Take care not to accidentally print out control character codes!
        if (iswprint(ch))
        {
            reader->Log.Write("%C", ch);
        }
        else
        {
            reader->Log.Write("\\U+%04hX", ch);
        }
    }

    reader->Log.WriteLine("'");
}


void ObjectReader::Read(ContentReader*)
{
    throw exception("ObjectReader should never be invoked directly.");
}
