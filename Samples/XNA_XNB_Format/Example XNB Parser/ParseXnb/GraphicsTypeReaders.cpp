#include "stdafx.h"
#include "ContentReader.h"
#include "GraphicsTypeReaders.h"
#include "MathTypeReaders.h"


static char* SurfaceFormatEnumValues[] =
{
    "Color",
    "Bgr565",
    "Bgra5551",
    "Bgra4444",
    "Dxt1",
    "Dxt3",
    "Dxt5",
    "NormalizedByte2",
    "NormalizedByte4",
    "Rgba1010102",
    "Rg32",
    "Rgba64",
    "Alpha8",
    "Single",
    "Vector2",
    "Vector4",
    "HalfSingle",
    "HalfVector2",
    "HalfVector4",
    "HdrBlendable",
    nullptr
};


static char* VertexElementFormatEnumValues[] =
{
    "Single",
    "Vector2",
    "Vector3",
    "Vector4",
    "Color",
    "Byte4",
    "Short2",
    "Short4",
    "NormalizedShort2",
    "NormalizedShort4",
    "HalfVector2",
    "HalfVector4",
    nullptr
};


static char* VertexElementUsageEnumValues[] =
{
    "Position",
    "Color",
    "TextureCoordinate",
    "Normal",
    "Binormal",
    "Tangent",
    "BlendIndices",
    "BlendWeight",
    "Depth",
    "Fog",
    "PointSize",
    "Sample",
    "TessellateFactor",
    nullptr
};


static char* CompareFunctionEnumValues[] =
{
    "Always",
    "Never",
    "Less",
    "LessEqual",
    "Equal",
    "GreaterEqual",
    "Greater",
    "NotEqual",
    nullptr
};


void TextureReader::Read(ContentReader*)
{
    throw exception("TextureReader should never be invoked directly.");
}


void Texture2DReader::Read(ContentReader* reader)
{
    reader->Log.WriteEnum("Format", reader->ReadInt32(), SurfaceFormatEnumValues);
    reader->Log.WriteLine("Width: %u", reader->ReadUInt32());
    reader->Log.WriteLine("Height: %u", reader->ReadUInt32());

    uint32_t mipCount = reader->ReadUInt32();
    reader->Log.WriteLine("Mip count: %u", mipCount);

    for (uint32_t i = 0; i < mipCount; i++)
    {
        reader->Log.Write("Mip %u", i);

        uint32_t dataSize = reader->ReadUInt32();
        reader->Log.WriteBytes("", reader->ReadBytes(dataSize));
    }
}


void Texture3DReader::Read(ContentReader* reader)
{
    reader->Log.WriteEnum("Format", reader->ReadInt32(), SurfaceFormatEnumValues);
    reader->Log.WriteLine("Width: %u", reader->ReadUInt32());
    reader->Log.WriteLine("Height: %u", reader->ReadUInt32());
    reader->Log.WriteLine("Depth: %u", reader->ReadUInt32());

    uint32_t mipCount = reader->ReadUInt32();
    reader->Log.WriteLine("Mip count: %u", mipCount);

    for (uint32_t i = 0; i < mipCount; i++)
    {
        reader->Log.Write("Mip %u", i);

        uint32_t dataSize = reader->ReadUInt32();
        reader->Log.WriteBytes("", reader->ReadBytes(dataSize));
    }
}


void TextureCubeReader::Read(ContentReader* reader)
{
    reader->Log.WriteEnum("Format", reader->ReadInt32(), SurfaceFormatEnumValues);
    reader->Log.WriteLine("Size: %u", reader->ReadUInt32());

    uint32_t mipCount = reader->ReadUInt32();
    reader->Log.WriteLine("Mip count: %u", mipCount);

    for (int face = 0; face < 6; face++)
    {
        for (uint32_t i = 0; i < mipCount; i++)
        {
            reader->Log.Write("Face %d mip %u", face, i);

            uint32_t dataSize = reader->ReadUInt32();
            reader->Log.WriteBytes("", reader->ReadBytes(dataSize));
        }
    }
}


void IndexBufferReader::Read(ContentReader* reader)
{
    reader->Log.WriteLine("Index format: %s", reader->ReadBoolean() ? "16 bit" : "32 bit");

    uint32_t dataSize = reader->ReadUInt32();
    reader->Log.WriteBytes("Index data", reader->ReadBytes(dataSize));
}


static uint32_t ReadVertexDeclaration(ContentReader* reader)
{
    uint32_t vertexStride = reader->ReadUInt32();
    reader->Log.WriteLine("Vertex stride: %u", vertexStride);

    uint32_t elementCount = reader->ReadUInt32();
    reader->Log.WriteLine("Element count: %u", elementCount);

    for (uint32_t i = 0; i < elementCount; i++)
    {
        reader->Log.WriteLine("Element %u:", i);
        reader->Log.Indent();

        reader->Log.WriteLine("Offset: %u", reader->ReadUInt32());
        reader->Log.WriteEnum("Element format", reader->ReadInt32(), VertexElementFormatEnumValues);
        reader->Log.WriteEnum("Element usage", reader->ReadInt32(), VertexElementUsageEnumValues);
        reader->Log.WriteLine("Usage index: %u", reader->ReadUInt32());

        reader->Log.Unindent();
    }

    return vertexStride;
}


void VertexBufferReader::Read(ContentReader* reader)
{
    reader->Log.WriteLine("Vertex declaration:");
    reader->Log.Indent();

    uint32_t vertexStride = ReadVertexDeclaration(reader);

    reader->Log.Unindent();

    uint32_t vertexCount = reader->ReadUInt32();
    reader->Log.WriteLine("Vertex count: %u", vertexCount);

    reader->Log.WriteBytes("Vertex data", reader->ReadBytes(vertexCount * vertexStride));
}


void VertexDeclarationReader::Read(ContentReader* reader)
{
    ReadVertexDeclaration(reader);
}


void EffectReader::Read(ContentReader* reader)
{
    uint32_t size = reader->ReadUInt32();

    reader->Log.WriteBytes("Effect bytecode", reader->ReadBytes(size));
}


void EffectMaterialReader::Read(ContentReader* reader)
{
    reader->Log.WriteLine("Effect reference: '%S'", reader->ReadString().c_str());
    
    reader->Log.WriteLine("Parameters:");
    reader->ReadObject();
}


void BasicEffectReader::Read(ContentReader* reader)
{
    reader->Log.WriteLine("Texture reference: '%S'", reader->ReadString().c_str());

    reader->Log.Write("Diffuse color: ");
    Vector3Reader().Read(reader);

    reader->Log.Write("Emissive color: ");
    Vector3Reader().Read(reader);

    reader->Log.Write("Specular color: ");
    Vector3Reader().Read(reader);

    reader->Log.WriteLine("Specular power: %g", reader->ReadSingle());
    reader->Log.WriteLine("Alpha: %g", reader->ReadSingle());
    reader->Log.WriteLine("Vertex color enabled: %s", reader->ReadBoolean() ? "true" : "false");
}


void AlphaTestEffectReader::Read(ContentReader* reader)
{
    reader->Log.WriteLine("Texture reference: '%S'", reader->ReadString().c_str());

    reader->Log.WriteEnum("Compare function", reader->ReadInt32(), CompareFunctionEnumValues);
    reader->Log.WriteLine("Reference alpha: %u", reader->ReadUInt32());

    reader->Log.Write("Diffuse color: ");
    Vector3Reader().Read(reader);

    reader->Log.WriteLine("Alpha: %g", reader->ReadSingle());
    reader->Log.WriteLine("Vertex color enabled: %s", reader->ReadBoolean() ? "true" : "false");
}


void DualTextureEffectReader::Read(ContentReader* reader)
{
    reader->Log.WriteLine("Texture 1 reference: '%S'", reader->ReadString().c_str());
    reader->Log.WriteLine("Texture 2 reference: '%S'", reader->ReadString().c_str());

    reader->Log.Write("Diffuse color: ");
    Vector3Reader().Read(reader);

    reader->Log.WriteLine("Alpha: %g", reader->ReadSingle());
    reader->Log.WriteLine("Vertex color enabled: %s", reader->ReadBoolean() ? "true" : "false");
}


void EnvironmentMapEffectReader::Read(ContentReader* reader)
{
    reader->Log.WriteLine("Texture reference: '%S'", reader->ReadString().c_str());
    reader->Log.WriteLine("Environment map reference: '%S'", reader->ReadString().c_str());

    reader->Log.WriteLine("Environment map amount: %g", reader->ReadSingle());

    reader->Log.Write("Environment map specular: ");
    Vector3Reader().Read(reader);

    reader->Log.WriteLine("Fresnel factor: %g", reader->ReadSingle());

    reader->Log.Write("Diffuse color: ");
    Vector3Reader().Read(reader);

    reader->Log.Write("Emissive color: ");
    Vector3Reader().Read(reader);

    reader->Log.WriteLine("Alpha: %g", reader->ReadSingle());
}


void SkinnedEffectReader::Read(ContentReader* reader)
{
    reader->Log.WriteLine("Texture reference: '%S'", reader->ReadString().c_str());

    reader->Log.WriteLine("Weights per vertex: %u", reader->ReadUInt32());

    reader->Log.Write("Diffuse color: ");
    Vector3Reader().Read(reader);

    reader->Log.Write("Emissive color: ");
    Vector3Reader().Read(reader);

    reader->Log.Write("Specular color: ");
    Vector3Reader().Read(reader);

    reader->Log.WriteLine("Specular power: %g", reader->ReadSingle());
    reader->Log.WriteLine("Alpha: %g", reader->ReadSingle());
}


void SpriteFontReader::Read(ContentReader* reader)
{
    reader->Log.WriteLine("Texture:");
    reader->ReadObject();

    reader->Log.WriteLine("Glyphs:");
    reader->ReadObject();

    reader->Log.WriteLine("Cropping:");
    reader->ReadObject();

    reader->Log.WriteLine("Character map:");
    reader->ReadObject();

    reader->Log.WriteLine("Vertical line spacing: %d", reader->ReadInt32());
    reader->Log.WriteLine("Horizontal spacing: %g", reader->ReadSingle());

    reader->Log.WriteLine("Kerning:");
    reader->ReadObject();

    reader->Log.Write("Default character: ");

    if (reader->ReadBoolean())
    {
        reader->Log.WriteLine("U+%04hX", reader->ReadChar());
    }
    else
    {
        reader->Log.WriteLine("null");
    }
}


static void ReadBoneReference(ContentReader* reader, uint32_t boneCount)
{
    uint32_t boneId;

    // Read the bone ID, which may be encoded as either an 8 or 32 bit value.
    if (boneCount < 255)
    {
        boneId = reader->ReadByte();
    }
    else
    {
        boneId = reader->ReadUInt32();
    }

    // Print out the bone ID.
    if (boneId)
    {
        reader->Log.WriteLine("bone #%u", boneId - 1);
    }
    else
    {
        reader->Log.WriteLine("null");
    }
}


void ModelReader::Read(ContentReader* reader)
{
    // Read the bone names and transforms.
    uint32_t boneCount = reader->ReadUInt32();
    reader->Log.WriteLine("Bone count: %u", boneCount);

    for (uint32_t i = 0; i < boneCount; i++)
    {
        reader->Log.WriteLine("Bone %u:", i);
        reader->Log.Indent();

        reader->Log.WriteLine("Name:");
        reader->ReadObject();

        reader->Log.WriteLine("Transform:");
        reader->Log.Indent();
        MatrixReader().Read(reader);
        reader->Log.Unindent();

        reader->Log.Unindent();
    }

    // Read the bone hierarchy.
    for (uint32_t i = 0; i < boneCount; i++)
    {
        reader->Log.WriteLine("Bone %u hierarchy:", i);
        reader->Log.Indent();

        // Read the parent bone reference.
        reader->Log.Write("Parent: ");
        ReadBoneReference(reader, boneCount);

        // Read the child bone references.
        uint32_t childCount = reader->ReadUInt32();

        if (childCount)
        {
            reader->Log.WriteLine("Children:");
            reader->Log.Indent();

            for (uint32_t j = 0; j < childCount; j++)
            {
                ReadBoneReference(reader, boneCount);
            }

            reader->Log.Unindent();
        }

        reader->Log.Unindent();
    }

    // Read the mesh data.
    uint32_t meshCount = reader->ReadUInt32();
    reader->Log.WriteLine("Mesh count: %u", meshCount);

    for (uint32_t i = 0; i < meshCount; i++)
    {
        reader->Log.WriteLine("Mesh %u", i);
        reader->Log.Indent();

        reader->Log.WriteLine("Mesh name:");
        reader->ReadObject();

        reader->Log.Write("Mesh parent: ");
        ReadBoneReference(reader, boneCount);

        reader->Log.WriteLine("Mesh bounds:");
        reader->Log.Indent();
        BoundingSphereReader().Read(reader);
        reader->Log.Unindent();

        reader->Log.WriteLine("Mesh tag:");
        reader->ReadObject();

        // Read the mesh part data.
        uint32_t partCount = reader->ReadUInt32();
        reader->Log.WriteLine("Mesh part count: %u", partCount);

        for (uint32_t j = 0; j < partCount; j++)
        {
            reader->Log.WriteLine("Mesh part %u", j);
            reader->Log.Indent();

            reader->Log.WriteLine("Vertex offset: %d", reader->ReadInt32());
            reader->Log.WriteLine("Num vertices: %d", reader->ReadInt32());
            reader->Log.WriteLine("Start index: %d", reader->ReadInt32());
            reader->Log.WriteLine("Primitive count: %d", reader->ReadInt32());

            reader->Log.WriteLine("Mesh part tag:");
            reader->ReadObject();

            reader->Log.Write("Vertex buffer: ");
            reader->ReadSharedResource();

            reader->Log.Write("Index buffer: ");
            reader->ReadSharedResource();

            reader->Log.Write("Effect: ");
            reader->ReadSharedResource();
            
            reader->Log.Unindent();
        }

        reader->Log.Unindent();
    }

    // Read the final pieces of model data.
    reader->Log.Write("Model root: ");
    ReadBoneReference(reader, boneCount);

    reader->Log.WriteLine("Model tag:");
    reader->ReadObject();
}
