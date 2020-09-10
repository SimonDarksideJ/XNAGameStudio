#include "stdafx.h"
#include "TypeReaderManager.h"
#include "PrimitiveTypeReaders.h"
#include "SystemTypeReaders.h"
#include "MathTypeReaders.h"
#include "GraphicsTypeReaders.h"
#include "MediaTypeReaders.h"


TypeReaderManager::~TypeReaderManager()
{
    for each (TypeReader* reader in typeReaders)
    {
        delete reader;
    }

    for each (GenericTypeReaderFactory* factory in genericReaders)
    {
        delete factory;
    }
}


void TypeReaderManager::RegisterStandardTypes()
{
    // Primitive types.
    RegisterTypeReader<ByteReader>();
    RegisterTypeReader<SByteReader>();
    RegisterTypeReader<Int16Reader>();
    RegisterTypeReader<UInt16Reader>();
    RegisterTypeReader<Int32Reader>();
    RegisterTypeReader<UInt32Reader>();
    RegisterTypeReader<Int64Reader>();
    RegisterTypeReader<UInt64Reader>();
    RegisterTypeReader<SingleReader>();
    RegisterTypeReader<DoubleReader>();
    RegisterTypeReader<BooleanReader>();
    RegisterTypeReader<CharReader>();
    RegisterTypeReader<StringReader>();
    RegisterTypeReader<ObjectReader>();

    // System types.
    RegisterGenericReader<EnumReader>();
    RegisterGenericReader<NullableReader>();
    RegisterGenericReader<ArrayReader>();
    RegisterGenericReader<ListReader>();
    RegisterGenericReader<DictionaryReader>();
    RegisterTypeReader<TimeSpanReader>();
    RegisterTypeReader<DateTimeReader>();
    RegisterTypeReader<DecimalReader>();
    RegisterTypeReader<ExternalReferenceReader>();
    RegisterGenericReader<ReflectiveReader>();

    // Math types.
    RegisterTypeReader<Vector2Reader>();
    RegisterTypeReader<Vector3Reader>();
    RegisterTypeReader<Vector4Reader>();
    RegisterTypeReader<MatrixReader>();
    RegisterTypeReader<QuaternionReader>();
    RegisterTypeReader<ColorReader>();
    RegisterTypeReader<PlaneReader>();
    RegisterTypeReader<PointReader>();
    RegisterTypeReader<RectangleReader>();
    RegisterTypeReader<BoundingBoxReader>();
    RegisterTypeReader<BoundingSphereReader>();
    RegisterTypeReader<BoundingFrustumReader>();
    RegisterTypeReader<RayReader>();
    RegisterTypeReader<CurveReader>();

    // Graphics types.
    RegisterTypeReader<TextureReader>();
    RegisterTypeReader<Texture2DReader>();
    RegisterTypeReader<Texture3DReader>();
    RegisterTypeReader<TextureCubeReader>();
    RegisterTypeReader<IndexBufferReader>();
    RegisterTypeReader<VertexBufferReader>();
    RegisterTypeReader<VertexDeclarationReader>();
    RegisterTypeReader<EffectReader>();
    RegisterTypeReader<EffectMaterialReader>();
    RegisterTypeReader<BasicEffectReader>();
    RegisterTypeReader<AlphaTestEffectReader>();
    RegisterTypeReader<DualTextureEffectReader>();
    RegisterTypeReader<EnvironmentMapEffectReader>();
    RegisterTypeReader<SkinnedEffectReader>();
    RegisterTypeReader<SpriteFontReader>();
    RegisterTypeReader<ModelReader>();

    // Media types.
    RegisterTypeReader<SoundEffectReader>();
    RegisterTypeReader<SongReader>();
    RegisterTypeReader<VideoReader>();
}


TypeReader* TypeReaderManager::GetByReaderName(wstring const& readerName)
{
    wstring wanted = StripAssemblyVersion(readerName);

    // Look for a type reader with this name.
    for each (TypeReader* reader in typeReaders)
    {
        if (reader->ReaderName() == wanted)
        {
            return reader;
        }
    }

    // Could this be a specialization of a generic reader?
    wstring genericReaderName;
    vector<wstring> genericArguments;

    if (SplitGenericTypeName(wanted, &genericReaderName, &genericArguments))
    {
        // Look for a generic reader factory with this name.
        for each (GenericTypeReaderFactory* factory in genericReaders)
        {
            if (factory->GenericReaderName() == genericReaderName)
            {
                // Create a specialized generic reader instance.
                GenericTypeReader* reader = factory->CreateTypeReader(genericArguments);

                assert(reader->ReaderName() == wanted);

                typeReaders.push_back(reader);

                return reader;
            }
        }
    }

    // Fatal error if we cannot find a suitable reader.
    char message[256];

    sprintf_s(message, "Can't find type reader '%S'.", wanted.c_str());

    throw exception(message);
}


TypeReader* TypeReaderManager::GetByTargetType(wstring const& targetType)
{
    wstring wanted = StripAssemblyVersion(targetType);

    // Look for a reader with this target type name.
    for each (TypeReader* reader in typeReaders)
    {
        if (reader->TargetType() == wanted)
        {
            return reader;
        }
    }

    // Fatal error if we cannot find a suitable reader.
    char message[256];

    sprintf_s(message, "Can't find reader for target type '%S'.", wanted.c_str());

    throw exception(message);
}


wstring TypeReaderManager::StripAssemblyVersion(wstring typeName)
{
	// Maps "foo, key=bar" -> "foo"
	// Maps "foo[bar, key=baz], key=barg" -> "foo[bar]"
	
	size_t commaIndex = 0;

	while ((commaIndex = typeName.find(',', commaIndex)) != wstring::npos)
	{
        if (commaIndex + 1 < typeName.npos && typeName[commaIndex + 1] == '[')
        {
            // Skip past the comma in the ],[ part of a generic type argument list.
            commaIndex++;
        }
        else
        {
            // Strip trailing assembly version information after other commas.
		    size_t closeBracket = typeName.find(']', commaIndex);

		    if (closeBracket != wstring::npos)
            {
			    typeName.erase(commaIndex, closeBracket - commaIndex);
            }
		    else
            {
			    typeName.erase(commaIndex);
            }
        }
	}

    return typeName;
}


bool TypeReaderManager::SplitGenericTypeName(wstring const& typeName, wstring* genericName, vector<wstring>* genericArguments)
{
    // Splits "foo`2[[bar],[baz]]" into genericName = "foo", genericArguments = { "bar", "baz" }

    // Look for the ` generic marker character.
    size_t pos = typeName.find('`');

    if (pos == wstring::npos)
        return false;

    // Everything to the left of ` is the generic type name.
    *genericName = typeName.substr(0, pos);

    // Advance to the start of the generic argument list.
    pos++;

    while (pos < typeName.size() && iswdigit(typeName[pos]))
        pos++;

    while (pos < typeName.size() && typeName[pos] == '[')
        pos++;

    // Split up the list of generic type arguments.
    while (pos < typeName.size() && typeName[pos] != ']')
    {
        // Locate the end of the current type name argument.
        int nesting = 0;
        size_t end;

        for (end = pos; end < typeName.size(); end++)
        {
            // Handle nested types in case we have eg. "List`1[[List`1[[Int]]]]".
            if (typeName[end] == '[')
            {
                nesting++;
            }
            else if (typeName[end] == ']')
            {
                if (nesting > 0)
                    nesting--;
                else
                    break;
            }
        }

        // Extract the type name argument.
        genericArguments->push_back(typeName.substr(pos, end - pos));

        // Skip past the type name, plus any subsequent "],[" goo.
        pos = end;

        if (pos < typeName.size() && typeName[pos] == ']')
            pos++;

        if (pos < typeName.size() && typeName[pos] == ',')
            pos++;

        if (pos < typeName.size() && typeName[pos] == '[')
            pos++;
    }

    return true;
}
