#pragma once


// Helper for writing formatted text to the console output.
class Logger
{
public:
    Logger();

    void Indent()   { indentation++; }
    void Unindent() { indentation--; }

    void Write(_In_z_ _Printf_format_string_ char const* format, ...);
    void WriteLine(_In_z_ _Printf_format_string_ char const* format, ...);
    void WriteBytes(_In_z_ char const* name, vector<uint8_t> const& bytes);
    void WriteEnum(_In_z_ char const* name, int32_t value, _In_z_ _Deref_pre_z_ char const* const* enumValues);

private:
    void Write(_In_z_ _Printf_format_string_ char const* format, va_list args);

    int indentation;
    bool isNewLine;
};
