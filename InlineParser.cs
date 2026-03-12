using System.Text;

namespace Markdown2Html;

public static class InlineParser
{
    public static IReadOnlyList<InlineNode> Parse(string text)
    {
        return ParseRange(text, 0, text.Length);
    }

    private static IReadOnlyList<InlineNode> ParseRange(string text, int start, int end)
    {
        var nodes = new List<InlineNode>();
        var buffer = new StringBuilder();
        var index = start;

        while (index < end)
        {
            if (index + 1 < end && text[index] == '*' && text[index + 1] == '*')
            {
                var closingIndex = text.IndexOf("**", index + 2, StringComparison.Ordinal);
                if (closingIndex >= 0 && closingIndex < end)
                {
                    FlushText(nodes, buffer);
                    nodes.Add(new StrongInline(ParseRange(text, index + 2, closingIndex)));
                    index = closingIndex + 2;
                    continue;
                }
            }

            if (text[index] == '*')
            {
                var closingIndex = text.IndexOf('*', index + 1);
                if (closingIndex >= 0 && closingIndex < end)
                {
                    FlushText(nodes, buffer);
                    nodes.Add(new EmphasisInline(ParseRange(text, index + 1, closingIndex)));
                    index = closingIndex + 1;
                    continue;
                }
            }

            if (text[index] == '`')
            {
                var closingIndex = text.IndexOf('`', index + 1);
                if (closingIndex >= 0 && closingIndex < end)
                {
                    FlushText(nodes, buffer);
                    nodes.Add(new CodeInline(text[(index + 1)..closingIndex]));
                    index = closingIndex + 1;
                    continue;
                }
            }

            if (text[index] == '[' && TryParseLink(text, index, end, out var linkInline, out var consumed))
            {
                FlushText(nodes, buffer);
                nodes.Add(linkInline);
                index += consumed;
                continue;
            }

            buffer.Append(text[index]);
            index++;
        }

        FlushText(nodes, buffer);
        return nodes;
    }

    private static bool TryParseLink(string text, int start, int end, out LinkInline linkInline, out int consumed)
    {
        linkInline = null!;
        consumed = 0;

        var closeBracket = text.IndexOf(']', start + 1);
        if (closeBracket < 0 || closeBracket + 1 >= end || text[closeBracket + 1] != '(')
        {
            return false;
        }

        var closeParen = text.IndexOf(')', closeBracket + 2);
        if (closeParen < 0 || closeParen >= end)
        {
            return false;
        }

        var label = text[(start + 1)..closeBracket];
        var href = text[(closeBracket + 2)..closeParen].Trim();
        linkInline = new LinkInline(href, Parse(label));
        consumed = closeParen - start + 1;
        return true;
    }

    private static void FlushText(List<InlineNode> nodes, StringBuilder buffer)
    {
        if (buffer.Length == 0)
        {
            return;
        }

        nodes.Add(new TextInline(buffer.ToString()));
        buffer.Clear();
    }
}