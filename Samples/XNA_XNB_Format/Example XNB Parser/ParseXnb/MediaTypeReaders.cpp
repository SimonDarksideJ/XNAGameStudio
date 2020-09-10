#include "stdafx.h"
#include "ContentReader.h"
#include "MediaTypeReaders.h"


void SoundEffectReader::Read(ContentReader* reader)
{
    uint32_t formatSize = reader->ReadUInt32();
    reader->Log.WriteBytes("Format", reader->ReadBytes(formatSize));

    uint32_t dataSize = reader->ReadUInt32();
    reader->Log.WriteBytes("Data", reader->ReadBytes(dataSize));

    reader->Log.WriteLine("Loop start: %d", reader->ReadInt32());
    reader->Log.WriteLine("Loop length: %d", reader->ReadInt32());
    reader->Log.WriteLine("Duration: %d ms", reader->ReadInt32());
}


void SongReader::Read(ContentReader* reader)
{
    reader->Log.WriteLine("Streaming filename: '%S'", reader->ReadString().c_str());
    
    reader->ValidateTypeId(L"System.Int32");
    reader->Log.WriteLine("Duration: %d ms", reader->ReadInt32());
}


void VideoReader::Read(ContentReader* reader)
{
    static char* SoundtrackTypeEnumValues[] =
    {
        "Music",
        "Dialog",
        "Music and Dialog",
        nullptr
    };

    reader->ValidateTypeId(L"System.String");
    reader->Log.WriteLine("Streaming filename: '%S'", reader->ReadString().c_str());

    reader->ValidateTypeId(L"System.Int32");
    reader->Log.WriteLine("Duration: %d ms", reader->ReadInt32());

    reader->ValidateTypeId(L"System.Int32");
    reader->Log.WriteLine("Width: %d ms", reader->ReadInt32());

    reader->ValidateTypeId(L"System.Int32");
    reader->Log.WriteLine("Height: %d ms", reader->ReadInt32());

    reader->ValidateTypeId(L"System.Single");
    reader->Log.WriteLine("Frames per second: %g ms", reader->ReadSingle());

    reader->ValidateTypeId(L"System.Int32");
    reader->Log.WriteEnum("Soundtrack type", reader->ReadInt32(), SoundtrackTypeEnumValues);
}
