#include "stdafx.h"
#include "ContentReader.h"
#include "SystemTypeReaders.h"


void EnumReader::Read(ContentReader* reader)
{
    reader->Log.WriteLine("Enum value: %d", reader->ReadInt32());
}


void NullableReader::Initialize(TypeReaderManager* typeReaderManager)
{
    // When specializing the generic Nullable<T> reader, look up how to read our specific value type T.
    valueReader = typeReaderManager->GetByTargetType(GenericArgument(0));
}


void NullableReader::Read(ContentReader* reader)
{
    if (reader->ReadBoolean())
    {
        valueReader->Read(reader);
    }
    else
    {
        reader->Log.WriteLine("null");
    }
}


void ArrayReader::Initialize(TypeReaderManager* typeReaderManager)
{
    // When specializing the generic T[] reader, look up how to read our specific element type T.
    elementReader = typeReaderManager->GetByTargetType(GenericArgument(0));
}


void ArrayReader::Read(ContentReader* reader)
{
    uint32_t elementCount = reader->ReadUInt32();

    reader->Log.WriteLine("Element count: %u", elementCount);

    for (uint32_t i = 0; i < elementCount; i++)
    {
        reader->Log.WriteLine("Element %u:", i);

        reader->ReadValueOrObject(elementReader);
    }
}


void ListReader::Initialize(TypeReaderManager* typeReaderManager)
{
    // When specializing the generic List<T> reader, look up how to read our specific element type T.
    elementReader = typeReaderManager->GetByTargetType(GenericArgument(0));
}


void ListReader::Read(ContentReader* reader)
{
    uint32_t elementCount = reader->ReadUInt32();

    reader->Log.WriteLine("Element count: %u", elementCount);

    for (uint32_t i = 0; i < elementCount; i++)
    {
        reader->Log.WriteLine("Element %u:", i);

        reader->ReadValueOrObject(elementReader);
    }
}


void DictionaryReader::Initialize(TypeReaderManager* typeReaderManager)
{
    // When specializing the generic Dictionary<K, V> reader, look up how to read our specific types K and V.
    keyReader = typeReaderManager->GetByTargetType(GenericArgument(0));
    valueReader = typeReaderManager->GetByTargetType(GenericArgument(1));
}


void DictionaryReader::Read(ContentReader* reader)
{
    uint32_t elementCount = reader->ReadUInt32();

    reader->Log.WriteLine("Element count: %u", elementCount);

    for (uint32_t i = 0; i < elementCount; i++)
    {
        reader->Log.WriteLine("Element %u:", i);
        reader->Log.Indent();

        reader->Log.WriteLine("Key:");
        reader->ReadValueOrObject(keyReader);
        
        reader->Log.WriteLine("Value:");
        reader->ReadValueOrObject(valueReader);

        reader->Log.Unindent();
    }
}


void TimeSpanReader::Read(ContentReader* reader)
{
    int64_t ticks = reader->ReadInt64();

    // Handle negative tick counts.
    if (ticks < 0)
    {
        ticks = -ticks;
        reader->Log.Write("-");
    }

    // Split into days, hours, minutes, seconds, and remaining fractional ticks.
    const int64_t ticksPerSecond = 10000000;
    const int64_t ticksPerMinute = ticksPerSecond * 60;
    const int64_t ticksPerHour = ticksPerMinute * 60;
    const int64_t ticksPerDay = ticksPerHour * 24;

    int64_t days = ticks / ticksPerDay;
    int32_t hours = (int32_t)((ticks % ticksPerDay) / ticksPerHour);
    int32_t minutes = (int32_t)((ticks % ticksPerHour) / ticksPerMinute);
    int32_t seconds = (int32_t)((ticks % ticksPerMinute) / ticksPerSecond);
    ticks = ticks % ticksPerSecond;

    // Only write the day count if non-zero.
    if (days)
    {
        reader->Log.Write("%lld.", days);
    }

    // Write hours, minutes, and seconds.
    reader->Log.Write("%d:%d:%d", hours, minutes, seconds);

    // Only write the fractional ticks if non-zero.
    if (ticks)
    {
        char formattedTicks[16];

        sprintf_s(formattedTicks, "%.7f", (double)ticks / ticksPerSecond);

        reader->Log.Write(strchr(formattedTicks, '.'));
    }

    reader->Log.WriteLine("");
}


void DateTimeReader::Read(ContentReader* reader)
{
    uint64_t value = reader->ReadUInt64();

    int32_t kind = value >> 62;
    uint64_t ticks = value & ~(3LL << 62);

    reader->Log.WriteLine("DateTimeKind: %d", kind);
    reader->Log.WriteLine("Ticks: %llu", ticks);
}


void DecimalReader::Read(ContentReader* reader)
{
    uint32_t a = reader->ReadUInt32();
    uint32_t b = reader->ReadUInt32();
    uint32_t c = reader->ReadUInt32();
    uint32_t d = reader->ReadUInt32();

    reader->Log.WriteLine("%08X:%08X.%08X.%08X", d, c, b, a);
}


void ExternalReferenceReader::Read(ContentReader* reader)
{
    reader->Log.WriteLine("'%S'", reader->ReadString().c_str());
}


void ReflectiveReader::Read(ContentReader*)
{
    printf("\n");
    printf("This C++ XNB loader implementation does not support ReflectiveReader.\n");
    printf("(think about it: it's not really possible to implement this without .NET style reflection)\n");
    printf("\n");
    printf("To load this file, you must manually implement a TypeReader subclass, fill in its Read method\n");
    printf("to load the contents of your custom object, and call TypeReaderManager::RegisterTypeReader\n");
    printf("to register this new custom loader.\n");
    printf("\n");
    printf("Your custom TypeReader should specify:\n");
    printf("    TargetType = '%S'\n", TargetType().c_str());
    printf("    ReaderName = '%S'\n", ReaderName().c_str());
    printf("\n");

    throw exception("Cannot parse XNB files that use automatic serialization.");
}
