#include "stdafx.h"
#include "ContentReader.h"
#include "MathTypeReaders.h"


void Vector2Reader::Read(ContentReader* reader)
{
    float x = reader->ReadSingle();
    float y = reader->ReadSingle();

    reader->Log.WriteLine("{ %g, %g }", x, y);
}


void Vector3Reader::Read(ContentReader* reader)
{
    float x = reader->ReadSingle();
    float y = reader->ReadSingle();
    float z = reader->ReadSingle();

    reader->Log.WriteLine("{ %g, %g, %g }", x, y, z);
}


void Vector4Reader::Read(ContentReader* reader)
{
    float x = reader->ReadSingle();
    float y = reader->ReadSingle();
    float z = reader->ReadSingle();
    float w = reader->ReadSingle();

    reader->Log.WriteLine("{ %g, %g, %g, %g }", x, y, z, w);
}


void MatrixReader::Read(ContentReader* reader)
{
    float m[16];

    for (int i = 0; i < 16; i++)
    {
        m[i] = reader->ReadSingle();
    }

    reader->Log.WriteLine("{ %g, %g, %g, %g }", m[0],  m[1],  m[2],  m[3]);
    reader->Log.WriteLine("{ %g, %g, %g, %g }", m[4],  m[5],  m[6],  m[7]);
    reader->Log.WriteLine("{ %g, %g, %g, %g }", m[8],  m[9],  m[10], m[11]);
    reader->Log.WriteLine("{ %g, %g, %g, %g }", m[12], m[13], m[14], m[15]);
}


void QuaternionReader::Read(ContentReader* reader)
{
    float x = reader->ReadSingle();
    float y = reader->ReadSingle();
    float z = reader->ReadSingle();
    float w = reader->ReadSingle();

    reader->Log.WriteLine("{ %g, %g, %g, %g }", x, y, z, w);
}


void ColorReader::Read(ContentReader* reader)
{
    int r = reader->ReadByte();
    int g = reader->ReadByte();
    int b = reader->ReadByte();
    int a = reader->ReadByte();

    reader->Log.WriteLine("{ R:%d, G:%d, B:%d, A:%d }", r, g, b, a);
}


void PlaneReader::Read(ContentReader* reader)
{
    reader->Log.Write("Normal: ");
    Vector3Reader().Read(reader);

    reader->Log.WriteLine("D: %g", reader->ReadSingle());
}


void PointReader::Read(ContentReader* reader)
{
    reader->Log.WriteLine("X: %d", reader->ReadInt32());
    reader->Log.WriteLine("Y: %d", reader->ReadInt32());
}


void RectangleReader::Read(ContentReader* reader)
{
    reader->Log.WriteLine("X: %d", reader->ReadInt32());
    reader->Log.WriteLine("Y: %d", reader->ReadInt32());
    reader->Log.WriteLine("Width: %d", reader->ReadInt32());
    reader->Log.WriteLine("Height: %d", reader->ReadInt32());
}


void BoundingBoxReader::Read(ContentReader* reader)
{
    reader->Log.Write("Min: ");
    Vector3Reader().Read(reader);

    reader->Log.Write("Max: ");
    Vector3Reader().Read(reader);
}


void BoundingSphereReader::Read(ContentReader* reader)
{
    reader->Log.Write("Center: ");
    Vector3Reader().Read(reader);

    reader->Log.WriteLine("Radius: %g", reader->ReadSingle());
}


void BoundingFrustumReader::Read(ContentReader* reader)
{
    reader->Log.WriteLine("Bounding frustum matrix:");
    reader->Log.Indent();

    MatrixReader().Read(reader);

    reader->Log.Unindent();
}


void RayReader::Read(ContentReader* reader)
{
    reader->Log.Write("Position: ");
    Vector3Reader().Read(reader);

    reader->Log.Write("Direction: ");
    Vector3Reader().Read(reader);
}


void CurveReader::Read(ContentReader* reader)
{
    static char* LoopEnumValues[] =
    {
        "Constant",
        "Cycle",
        "Cycle Offset",
        "Oscillate",
        "Linear",
        nullptr
    };

    static char* ContinuityEnumValues[] =
    {
        "Smooth",
        "Step",
        nullptr
    };
    
    reader->Log.WriteEnum("Pre loop", reader->ReadInt32(), LoopEnumValues);
    reader->Log.WriteEnum("Post loop", reader->ReadInt32(), LoopEnumValues);

    uint32_t keyCount = reader->ReadUInt32();

    reader->Log.WriteLine("Key count: %u", keyCount);

    for (uint32_t i = 0; i < keyCount; i++)
    {
        reader->Log.WriteLine("Key %u:", i);
        reader->Log.Indent();

        reader->Log.WriteLine("Position: %g", reader->ReadSingle());
        reader->Log.WriteLine("Value: %g", reader->ReadSingle());
        reader->Log.WriteLine("Tangent in: %g", reader->ReadSingle());
        reader->Log.WriteLine("Tangent out: %g", reader->ReadSingle());
        reader->Log.WriteEnum("Continuity", reader->ReadInt32(), ContinuityEnumValues);

        reader->Log.Unindent();
    }
}
