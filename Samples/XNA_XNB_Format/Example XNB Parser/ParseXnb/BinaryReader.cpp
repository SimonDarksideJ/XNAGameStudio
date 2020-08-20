#include "stdafx.h"
#include "BinaryReader.h"


BinaryReader::BinaryReader(FILE* file)
  : file(file)
{
}


uint8_t BinaryReader::ReadByte()
{
    int value = fgetc(file);

    if (value == EOF)
    {
        throw exception("Error reading file.");
    }

    return (uint8_t)value;
}


uint16_t BinaryReader::ReadUInt16()
{
    uint8_t b1 = ReadByte();
    uint8_t b2 = ReadByte();

    return uint16_t(b1) |
           uint16_t(b2) << 8;
}


uint32_t BinaryReader::ReadUInt32()
{
    uint8_t b1 = ReadByte();
    uint8_t b2 = ReadByte();
    uint8_t b3 = ReadByte();
    uint8_t b4 = ReadByte();

    return uint32_t(b1)       |
           uint32_t(b2) << 8  |
           uint32_t(b3) << 16 |
           uint32_t(b4) << 24;
}


uint64_t BinaryReader::ReadUInt64()
{
    uint8_t b1 = ReadByte();
    uint8_t b2 = ReadByte();
    uint8_t b3 = ReadByte();
    uint8_t b4 = ReadByte();
    uint8_t b5 = ReadByte();
    uint8_t b6 = ReadByte();
    uint8_t b7 = ReadByte();
    uint8_t b8 = ReadByte();

    return uint64_t(b1)       |
           uint64_t(b2) << 8  |
           uint64_t(b3) << 16 |
           uint64_t(b4) << 24 |
           uint64_t(b5) << 32 |
           uint64_t(b6) << 40 |
           uint64_t(b7) << 48 |
           uint64_t(b8) << 56;
}


int8_t BinaryReader::ReadSByte()
{
    return (int8_t)ReadByte();
}


int16_t BinaryReader::ReadInt16()
{
    return (int16_t)ReadUInt16();
}


int32_t BinaryReader::ReadInt32()
{
    return (int32_t)ReadUInt32();
}


int64_t BinaryReader::ReadInt64()
{
    return (int64_t)ReadUInt64();
}


float BinaryReader::ReadSingle()
{
    uint32_t value = ReadUInt32();

    return *(float*)&value;
}


double BinaryReader::ReadDouble()
{
    uint64_t value = ReadUInt64();

    return *(double*)&value;
}


bool BinaryReader::ReadBoolean()
{
    return ReadByte() ? true : false;
}


wchar_t BinaryReader::ReadChar()
{
   wchar_t result = ReadByte();

    // Decode UTF-8.
   if (result & 0x80)
   {
       int byteCount = 1;

       while (result & (0x80 >> byteCount))
       {
           byteCount++;
       }

       result &= (1 << (8 - byteCount)) - 1;

       while (--byteCount)
       {
           result <<= 6;
           result |= ReadByte() & 0x3F;
       }
   }

   return result;
}


wstring BinaryReader::ReadString()
{
    uint32_t stringLength = Read7BitEncodedInt();

    uint32_t endOfString = FilePosition() + stringLength;

    wstring result;

    while (FilePosition() < endOfString)
    {
        result += ReadChar();
    }

    return result;
}


uint32_t BinaryReader::Read7BitEncodedInt()
{
    uint32_t result = 0;
    uint32_t bitsRead = 0;
    uint32_t value;

    do
    {
        value = ReadByte();
        result |= (value & 0x7f) << bitsRead;
        bitsRead += 7;
    }
    while (value & 0x80);

    return result;
}


vector<uint8_t> BinaryReader::ReadBytes(uint32_t count)
{
    vector<uint8_t> result;

    result.reserve(count);

    while (count--)
    {
        result.push_back(ReadByte());
    }

    return result;
}


uint32_t BinaryReader::FilePosition()
{
    return ftell(file);
}


uint32_t BinaryReader::FileSize()
{
    uint32_t currentPosition = FilePosition();

    // Seek to the end of the file.
    if (fseek(file, 0, SEEK_END) != 0)
    {
        throw exception("Seek failed.");
    }

    // Query the file size.
    uint32_t size = FilePosition();
    
    // Restore the original position.
    if (fseek(file, currentPosition, SEEK_SET) != 0)
    {
        throw exception("Seek failed.");
    }

    return size;
}
