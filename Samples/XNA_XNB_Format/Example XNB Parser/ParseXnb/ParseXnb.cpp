#include "stdafx.h"
#include "ContentReader.h"


int main(int argc, char const* argv[])
{
    // Make sure we got a single filename commandline argument.
    if (argc != 2)
    {
        printf("Usage: ParseXnb <filename>.xnb\n");
        return 1;
    }

    // Open the file.
    FILE* file;

    if (fopen_s(&file, argv[1], "rb") != 0)
    {
        printf("Error: can't open '%s'.\n", argv[1]);
        return 1;
    }

    // Instantate the XNB reader.
    TypeReaderManager typeReaderManager;

    typeReaderManager.RegisterStandardTypes();

    ContentReader reader(file, &typeReaderManager);

    // Parse the XNB data.
    int exitCode = 0;

    try
    {
        reader.ReadXnb();
    }
    catch (exception& e)
    {
        printf("Error: %s\n", e.what());

        exitCode = 1;
    }

    fclose(file);

    return exitCode;
}
