using System.Text;
using System.Text.Encodings.Web;

namespace Markdown2Html;

public static class SyntaxHighlighter
{
    private static readonly HtmlEncoder Encoder = HtmlEncoder.Default;

    public static string? HighlightCode(string code, string? language)
    {
        var normalizedLanguage = NormalizeLanguage(language);

        return normalizedLanguage switch
        {
            "csharp" => HighlightScript(code, ScriptLanguage.CSharp),
            "javascript" => HighlightScript(code, ScriptLanguage.JavaScript),
            "typescript" => HighlightScript(code, ScriptLanguage.TypeScript),
            "powershell" => HighlightScript(code, ScriptLanguage.PowerShell),
            "bash" => HighlightScript(code, ScriptLanguage.Bash),
            "python" => HighlightScript(code, ScriptLanguage.Python),
            "json" => HighlightJson(code),
            "html" or "xml" => HighlightMarkup(code),
            _ => null
        };
    }

    private static string? NormalizeLanguage(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return null;
        }

        return language.Trim().ToLowerInvariant() switch
        {
            "cs" or "c#" => "csharp",
            "js" => "javascript",
            "ts" => "typescript",
            "ps" or "ps1" or "pwsh" => "powershell",
            "sh" or "shell" or "zsh" => "bash",
            "htm" => "html",
            _ => language.Trim().ToLowerInvariant()
        };
    }

    private static string HighlightScript(string code, ScriptLanguage language)
    {
        var builder = new StringBuilder();
        var index = 0;

        while (index < code.Length)
        {
            if (TryReadBlockComment(code, ref index, language, out var blockComment))
            {
                AppendToken(builder, "tok-comment", blockComment);
                continue;
            }

            if (TryReadLineComment(code, ref index, language, out var lineComment))
            {
                AppendToken(builder, "tok-comment", lineComment);
                continue;
            }

            if (TryReadString(code, ref index, language, out var stringLiteral))
            {
                AppendToken(builder, "tok-string", stringLiteral);
                continue;
            }

            if (TryReadNumber(code, ref index, out var numberLiteral))
            {
                AppendToken(builder, "tok-number", numberLiteral);
                continue;
            }

            if (TryReadIdentifier(code, ref index, out var identifier))
            {
                if (language.Keywords.Contains(identifier))
                {
                    AppendToken(builder, "tok-keyword", identifier);
                }
                else if (language.Literals.Contains(identifier))
                {
                    AppendToken(builder, "tok-literal", identifier);
                }
                else if (language.Types.Contains(identifier))
                {
                    AppendToken(builder, "tok-type", identifier);
                }
                else
                {
                    builder.Append(Encoder.Encode(identifier));
                }

                continue;
            }

            builder.Append(Encoder.Encode(code[index].ToString()));
            index++;
        }

        return builder.ToString();
    }

    private static string HighlightJson(string code)
    {
        var builder = new StringBuilder();
        var index = 0;

        while (index < code.Length)
        {
            if (char.IsWhiteSpace(code[index]))
            {
                builder.Append(Encoder.Encode(code[index].ToString()));
                index++;
                continue;
            }

            if (TryReadJsonString(code, ref index, out var stringToken))
            {
                var probe = index;
                while (probe < code.Length && char.IsWhiteSpace(code[probe]))
                {
                    probe++;
                }

                AppendToken(builder, probe < code.Length && code[probe] == ':' ? "tok-property" : "tok-string", stringToken);
                continue;
            }

            if (TryReadNumber(code, ref index, out var numberLiteral))
            {
                AppendToken(builder, "tok-number", numberLiteral);
                continue;
            }

            if (TryReadIdentifier(code, ref index, out var identifier))
            {
                if (identifier is "true" or "false" or "null")
                {
                    AppendToken(builder, "tok-literal", identifier);
                }
                else
                {
                    builder.Append(Encoder.Encode(identifier));
                }

                continue;
            }

            builder.Append(Encoder.Encode(code[index].ToString()));
            index++;
        }

        return builder.ToString();
    }

    private static string HighlightMarkup(string code)
    {
        var builder = new StringBuilder();
        var index = 0;

        while (index < code.Length)
        {
            if (code.AsSpan(index).StartsWith("<!--", StringComparison.Ordinal))
            {
                var end = code.IndexOf("-->", index + 4, StringComparison.Ordinal);
                if (end < 0)
                {
                    end = code.Length - 3;
                }

                var comment = code[index..Math.Min(code.Length, end + 3)];
                AppendToken(builder, "tok-comment", comment);
                index += comment.Length;
                continue;
            }

            if (code[index] == '<')
            {
                var tagEnd = code.IndexOf('>', index);
                if (tagEnd < 0)
                {
                    builder.Append(Encoder.Encode(code[index].ToString()));
                    index++;
                    continue;
                }

                HighlightTag(builder, code[index..(tagEnd + 1)]);
                index = tagEnd + 1;
                continue;
            }

            builder.Append(Encoder.Encode(code[index].ToString()));
            index++;
        }

        return builder.ToString();
    }

    private static void HighlightTag(StringBuilder builder, string tag)
    {
        var index = 0;
        builder.Append(Encoder.Encode("<"));
        index++;

        if (index < tag.Length && tag[index] == '/')
        {
            builder.Append(Encoder.Encode("/"));
            index++;
        }

        var tagNameStart = index;
        while (index < tag.Length && IsIdentifierCharacter(tag[index]))
        {
            index++;
        }

        if (index > tagNameStart)
        {
            AppendToken(builder, "tok-tag", tag[tagNameStart..index]);
        }

        while (index < tag.Length - 1)
        {
            if (char.IsWhiteSpace(tag[index]))
            {
                builder.Append(Encoder.Encode(tag[index].ToString()));
                index++;
                continue;
            }

            if (tag[index] == '/' && index == tag.Length - 2)
            {
                builder.Append(Encoder.Encode("/"));
                index++;
                continue;
            }

            var attributeNameStart = index;
            while (index < tag.Length && tag[index] != '=' && !char.IsWhiteSpace(tag[index]) && tag[index] != '>')
            {
                index++;
            }

            if (index > attributeNameStart)
            {
                AppendToken(builder, "tok-attr-name", tag[attributeNameStart..index]);
            }

            while (index < tag.Length && char.IsWhiteSpace(tag[index]))
            {
                builder.Append(Encoder.Encode(tag[index].ToString()));
                index++;
            }

            if (index < tag.Length && tag[index] == '=')
            {
                builder.Append(Encoder.Encode("="));
                index++;
            }

            while (index < tag.Length && char.IsWhiteSpace(tag[index]))
            {
                builder.Append(Encoder.Encode(tag[index].ToString()));
                index++;
            }

            if (index < tag.Length && (tag[index] == '"' || tag[index] == '\''))
            {
                var delimiter = tag[index];
                var valueStart = index;
                index++;

                while (index < tag.Length && tag[index] != delimiter)
                {
                    index++;
                }

                if (index < tag.Length)
                {
                    index++;
                }

                AppendToken(builder, "tok-attr-value", tag[valueStart..index]);
            }
        }

        if (tag.Length > 1)
        {
            builder.Append(Encoder.Encode(tag[^1].ToString()));
        }
    }

    private static bool TryReadBlockComment(string code, ref int index, ScriptLanguage language, out string comment)
    {
        foreach (var marker in language.BlockComments)
        {
            if (!code.AsSpan(index).StartsWith(marker.Start, StringComparison.Ordinal))
            {
                continue;
            }

            var end = code.IndexOf(marker.End, index + marker.Start.Length, StringComparison.Ordinal);
            if (end < 0)
            {
                comment = code[index..];
                index = code.Length;
                return true;
            }

            comment = code[index..(end + marker.End.Length)];
            index = end + marker.End.Length;
            return true;
        }

        comment = string.Empty;
        return false;
    }

    private static bool TryReadLineComment(string code, ref int index, ScriptLanguage language, out string comment)
    {
        foreach (var marker in language.LineComments)
        {
            if (!code.AsSpan(index).StartsWith(marker, StringComparison.Ordinal))
            {
                continue;
            }

            var end = index;
            while (end < code.Length && code[end] != '\r' && code[end] != '\n')
            {
                end++;
            }

            comment = code[index..end];
            index = end;
            return true;
        }

        comment = string.Empty;
        return false;
    }

    private static bool TryReadString(string code, ref int index, ScriptLanguage language, out string stringLiteral)
    {
        stringLiteral = string.Empty;

        if (language.SupportsTripleQuotes && index + 2 < code.Length)
        {
            var tripleDelimiter = code.AsSpan(index, 3).ToString();
            if (tripleDelimiter is "\"\"\"" or "'''")
            {
                var end = code.IndexOf(tripleDelimiter, index + 3, StringComparison.Ordinal);
                if (end < 0)
                {
                    stringLiteral = code[index..];
                    index = code.Length;
                    return true;
                }

                stringLiteral = code[index..(end + 3)];
                index = end + 3;
                return true;
            }
        }

        if (!language.StringDelimiters.Contains(code[index]))
        {
            return false;
        }

        var delimiter = code[index];
        var start = index;
        index++;

        while (index < code.Length)
        {
            if (language.EscapeCharacter is not null && code[index] == language.EscapeCharacter.Value && index + 1 < code.Length)
            {
                index += 2;
                continue;
            }

            if (code[index] == delimiter)
            {
                index++;
                break;
            }

            index++;
        }

        stringLiteral = code[start..index];
        return true;
    }

    private static bool TryReadJsonString(string code, ref int index, out string stringLiteral)
    {
        stringLiteral = string.Empty;
        if (code[index] != '"')
        {
            return false;
        }

        var start = index;
        index++;

        while (index < code.Length)
        {
            if (code[index] == '\\' && index + 1 < code.Length)
            {
                index += 2;
                continue;
            }

            if (code[index] == '"')
            {
                index++;
                break;
            }

            index++;
        }

        stringLiteral = code[start..index];
        return true;
    }

    private static bool TryReadNumber(string code, ref int index, out string numberLiteral)
    {
        numberLiteral = string.Empty;
        if (!char.IsDigit(code[index]))
        {
            return false;
        }

        var start = index;
        index++;
        while (index < code.Length && (char.IsDigit(code[index]) || code[index] is '.' or '_' or 'x' or 'X' or 'a' or 'b' or 'c' or 'd' or 'e' or 'f' or 'A' or 'B' or 'C' or 'D' or 'E' or 'F'))
        {
            index++;
        }

        numberLiteral = code[start..index];
        return true;
    }

    private static bool TryReadIdentifier(string code, ref int index, out string identifier)
    {
        identifier = string.Empty;
        if (!IsIdentifierStart(code[index]))
        {
            return false;
        }

        var start = index;
        index++;
        while (index < code.Length && IsIdentifierCharacter(code[index]))
        {
            index++;
        }

        identifier = code[start..index];
        return true;
    }

    private static bool IsIdentifierStart(char value) => char.IsLetter(value) || value is '_' or '$' or '@';

    private static bool IsIdentifierCharacter(char value) => char.IsLetterOrDigit(value) || value is '_' or '$' or '-' or ':' or '@';

    private static void AppendToken(StringBuilder builder, string cssClass, string value)
    {
        builder.Append("<span class=\"")
            .Append(cssClass)
            .Append("\">")
            .Append(Encoder.Encode(value))
            .Append("</span>");
    }

    private sealed record CommentMarker(string Start, string End);

    private sealed record ScriptLanguage(
        HashSet<string> Keywords,
        HashSet<string> Literals,
        HashSet<string> Types,
        IReadOnlyList<string> LineComments,
        IReadOnlyList<CommentMarker> BlockComments,
        IReadOnlyList<char> StringDelimiters,
        char? EscapeCharacter,
        bool SupportsTripleQuotes = false)
    {
        public static readonly ScriptLanguage CSharp = new(
            ["abstract", "as", "async", "await", "break", "case", "catch", "class", "const", "continue", "do", "else", "enum", "event", "finally", "for", "foreach", "if", "in", "interface", "internal", "namespace", "new", "override", "private", "protected", "public", "record", "return", "sealed", "static", "struct", "switch", "throw", "try", "using", "var", "virtual", "void", "while"],
            ["false", "null", "true"],
            ["bool", "byte", "char", "decimal", "double", "float", "int", "long", "object", "short", "string", "Task"],
            ["//"],
            [new CommentMarker("/*", "*/")],
            ['\'', '"'],
            '\\');

        public static readonly ScriptLanguage JavaScript = new(
            ["await", "break", "case", "catch", "class", "const", "continue", "default", "else", "export", "extends", "finally", "for", "function", "if", "import", "let", "new", "return", "switch", "throw", "try", "typeof", "var", "while"],
            ["false", "null", "true", "undefined"],
            ["Array", "Date", "Promise", "RegExp", "String"],
            ["//"],
            [new CommentMarker("/*", "*/")],
            ['\'', '"', '`'],
            '\\');

        public static readonly ScriptLanguage TypeScript = new(
            ["as", "async", "await", "break", "case", "catch", "class", "const", "continue", "default", "else", "enum", "export", "extends", "finally", "for", "function", "if", "implements", "import", "interface", "let", "new", "private", "protected", "public", "readonly", "return", "switch", "throw", "try", "type", "while"],
            ["false", "null", "true", "undefined"],
            ["Array", "Date", "Promise", "RegExp", "String", "number", "string", "boolean"],
            ["//"],
            [new CommentMarker("/*", "*/")],
            ['\'', '"', '`'],
            '\\');

        public static readonly ScriptLanguage PowerShell = new(
            ["begin", "class", "else", "elseif", "end", "filter", "for", "foreach", "function", "if", "in", "param", "process", "return", "switch", "throw", "trap", "try", "while"],
            ["$false", "$null", "$true"],
            ["[string]", "[int]", "[bool]", "[datetime]"],
            ["#"],
            [new CommentMarker("<#", "#>")],
            ['\'', '"'],
            '`');

        public static readonly ScriptLanguage Bash = new(
            ["case", "do", "done", "elif", "else", "esac", "fi", "for", "function", "if", "in", "local", "return", "then", "until", "while"],
            ["false", "true"],
            ["echo", "printf", "test"],
            ["#"],
            [],
            ['\'', '"'],
            '\\');

        public static readonly ScriptLanguage Python = new(
            ["and", "as", "async", "await", "class", "def", "elif", "else", "except", "finally", "for", "from", "if", "import", "in", "is", "lambda", "not", "or", "pass", "raise", "return", "try", "while", "with", "yield"],
            ["False", "None", "True"],
            ["dict", "int", "list", "set", "str", "tuple"],
            ["#"],
            [],
            ['\'', '"'],
            '\\',
            SupportsTripleQuotes: true);
    }
}