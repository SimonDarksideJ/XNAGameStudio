#pragma once


// Helper for reading strongly typed binary data from an input file.
class BinaryReader
{
public:
    BinaryReader(FILE* file);

    virtual ~BinaryReader() { }

    uint8_t ReadByte();
    uint16_t ReadUInt16();
    uint32_t ReadUInt32();
    uint64_t ReadUInt64();
    
    int8_t ReadSByte();
    int16_t ReadInt16();
    int32_t ReadInt32();
    int64_t ReadInt64();

    float ReadSingle();
    double ReadDouble();

    bool ReadBoolean();

    wchar_t ReadChar();
    wstring ReadString();

    uint32_t Read7BitEncodedInt();

    vector<uint8_t> ReadBytes(uint32_t count);

    uint32_t FilePosition();
    uint32_t FileSize();

private:
    FILE* file;
};
