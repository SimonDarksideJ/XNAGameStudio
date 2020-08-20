#include "stdafx.h"
#include "Logger.h"


Logger::Logger()
{
    indentation = 0;

    isNewLine = false;
}


void Logger::Write(char const* format, ...)
{
    // Write the string.
    va_list args;
    va_start(args, format);

    Write(format, args);

    va_end(args);
}


void Logger::WriteLine(char const* format, ...)
{
    // Write the string.
    va_list args;
    va_start(args, format);

    Write(format, args);

    va_end(args);

    // Write the carriage return.
    printf("\n");

    isNewLine = true;
}


void Logger::Write(char const* format, va_list args)
{
    // Indent if this is the first text on a new line.
    if (isNewLine)
    {
        isNewLine = false;

        for (int i = 0; i < indentation; i++)
        {
            printf("    ");
        }
    }

    // Write the string.
    vprintf(format, args);
}


void Logger::WriteBytes(_In_z_ char const* name, vector<uint8_t> const& bytes)
{
    WriteLine("%s: %u bytes", name, bytes.size());
    
    Indent();

    for (size_t i = 0; i < bytes.size(); i++)
    {
        if (((i & 15) == 15) || (i == bytes.size() - 1))
        {
            WriteLine("%02X", bytes[i]);

            if (i >= 1024 && bytes.size() > 2048)
            {
                WriteLine("{snip: not bothering to print the remaining %d bytes}", bytes.size() - i);
                break;
            }
        }
        else
        {
            Write("%02X, ", bytes[i]);
        }
    }

    Unindent();
}


void Logger::WriteEnum(_In_z_ char const* name, int32_t value, _In_z_ _Deref_pre_z_ char const* const* enumValues)
{
    if ((value >= 0) && (find(enumValues, enumValues + value, nullptr) == enumValues + value))
    {
        WriteLine("%s: %s", name, enumValues[value]);
    }
    else
    {
        WriteLine("%s: unknown enum value %d", name, value);
    }
}
