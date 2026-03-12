using System.Text.RegularExpressions;

namespace Markdown2Html;

public static partial class MarkdownParser
{
    public static MarkdownDocument Parse(string markdown)
    {
        var normalized = Normalize(markdown);
        var lines = normalized.Split('\n');
        var blocks = ParseBlocks(lines, 0, lines.Length, 0);
        return new MarkdownDocument(blocks);
    }

    private static List<BlockNode> ParseBlocks(string[] lines, int start, int end, int currentIndent)
    {
        var blocks = new List<BlockNode>();
        var index = start;

        while (index < end)
        {
            if (string.IsNullOrWhiteSpace(lines[index]))
            {
                index++;
                continue;
            }

            if (CountIndentation(lines[index]) < currentIndent)
            {
                break;
            }

            if (TryParseCodeBlock(lines, ref index, end, currentIndent, out var codeBlock))
            {
                blocks.Add(codeBlock);
                continue;
            }

            var currentLine = TrimIndent(lines[index], currentIndent);

            if (TryParseHeading(currentLine, out var headingBlock))
            {
                blocks.Add(headingBlock);
                index++;
                continue;
            }

            if (TryParseTable(lines, ref index, end, currentIndent, out var tableBlock))
            {
                blocks.Add(tableBlock);
                continue;
            }

            if (TryParseBlockQuote(lines, ref index, end, currentIndent, out var blockQuote))
            {
                blocks.Add(blockQuote);
                continue;
            }

            if (TryParseList(lines, ref index, end, currentIndent, out var listBlock))
            {
                blocks.Add(listBlock);
                continue;
            }

            blocks.Add(ParseParagraph(lines, ref index, end, currentIndent));
        }

        return blocks;
    }

    private static bool TryParseCodeBlock(string[] lines, ref int index, int end, int currentIndent, out CodeBlock block)
    {
        block = null!;
        if (CountIndentation(lines[index]) != currentIndent)
        {
            return false;
        }

        var line = TrimIndent(lines[index], currentIndent);

        if (!line.StartsWith("```", StringComparison.Ordinal))
        {
            return false;
        }

        var language = line[3..].Trim();
        var codeLines = new List<string>();
        index++;

        while (index < end && !TrimIndent(lines[index], currentIndent).StartsWith("```", StringComparison.Ordinal))
        {
            codeLines.Add(TrimIndent(lines[index], currentIndent));
            index++;
        }

        if (index < end)
        {
            index++;
        }

        block = new CodeBlock(string.IsNullOrWhiteSpace(language) ? null : language, string.Join("\n", codeLines));
        return true;
    }

    private static bool TryParseHeading(string line, out HeadingBlock block)
    {
        block = null!;
        var match = HeadingPattern().Match(line);

        if (!match.Success)
        {
            return false;
        }

        var level = match.Groups[1].Value.Length;
        var content = match.Groups[2].Value.Trim();
        block = new HeadingBlock(level, InlineParser.Parse(content));
        return true;
    }

    private static bool TryParseTable(string[] lines, ref int index, int end, int currentIndent, out TableBlock block)
    {
        block = null!;

        if (index + 1 >= end || CountIndentation(lines[index]) != currentIndent || CountIndentation(lines[index + 1]) != currentIndent)
        {
            return false;
        }

        var headerLine = TrimIndent(lines[index], currentIndent);
        var separatorLine = TrimIndent(lines[index + 1], currentIndent);

        if (!LooksLikeTableRow(headerLine) || !TryParseTableSeparator(separatorLine, out var alignments))
        {
            return false;
        }

        var headerCells = ParseTableRow(headerLine);
        if (headerCells.Count != alignments.Count)
        {
            return false;
        }

        index += 2;
        var rows = new List<TableRow>();
        while (index < end)
        {
            if (string.IsNullOrWhiteSpace(lines[index]) || CountIndentation(lines[index]) != currentIndent)
            {
                break;
            }

            var rowLine = TrimIndent(lines[index], currentIndent);
            if (!LooksLikeTableRow(rowLine) || TryParseTableSeparator(rowLine, out _))
            {
                break;
            }

            var parsedCells = NormalizeTableCells(ParseTableRow(rowLine), alignments.Count);
            rows.Add(new TableRow(parsedCells.Select(static cell => new TableCell(InlineParser.Parse(cell))).ToList()));
            index++;
        }

        block = new TableBlock(
            alignments,
            headerCells.Select(static cell => new TableCell(InlineParser.Parse(cell))).ToList(),
            rows);

        return true;
    }

    private static bool TryParseBlockQuote(string[] lines, ref int index, int end, int currentIndent, out BlockQuoteBlock block)
    {
        block = null!;

        if (CountIndentation(lines[index]) != currentIndent)
        {
            return false;
        }

        var line = TrimIndent(lines[index], currentIndent);

        if (!line.StartsWith('>'))
        {
            return false;
        }

        var quoteLines = new List<string>();

        while (index < end && CountIndentation(lines[index]) == currentIndent)
        {
            var currentLine = TrimIndent(lines[index], currentIndent);
            if (!currentLine.StartsWith('>'))
            {
                break;
            }

            var content = currentLine.Length > 1 && currentLine[1] == ' '
                ? currentLine[2..]
                : currentLine[1..];
            quoteLines.Add(content);
            index++;
        }

        block = new BlockQuoteBlock(ParseBlocks(quoteLines.ToArray(), 0, quoteLines.Count, 0));
        return true;
    }

    private static bool TryParseList(string[] lines, ref int index, int end, int currentIndent, out ListBlock block)
    {
        block = null!;

        if (!TryMatchListItem(lines[index], out var firstMatch) || firstMatch.Indent != currentIndent)
        {
            return false;
        }

        var ordered = firstMatch.Ordered;
        var items = new List<ListItemBlock>();

        while (index < end && TryMatchListItem(lines[index], out var itemMatch) && itemMatch.Indent == currentIndent && itemMatch.Ordered == ordered)
        {
            var segments = new List<string>();
            if (!string.IsNullOrWhiteSpace(itemMatch.Content))
            {
                segments.Add(itemMatch.Content);
            }

            var children = new List<BlockNode>();
            index++;

            while (index < end)
            {
                var currentLine = lines[index];
                if (string.IsNullOrWhiteSpace(currentLine))
                {
                    index++;
                    continue;
                }

                if (TryMatchListItem(currentLine, out var siblingMatch) && siblingMatch.Indent == currentIndent && siblingMatch.Ordered == ordered)
                {
                    break;
                }

                if (TryMatchListItem(currentLine, out var nestedMatch) && nestedMatch.Indent > currentIndent)
                {
                    if (TryParseList(lines, ref index, end, nestedMatch.Indent, out var nestedList))
                    {
                        children.Add(nestedList);
                        continue;
                    }
                }

                if (CountIndentation(currentLine) < currentIndent || IsNonListBlockBoundary(lines, index, currentIndent))
                {
                    break;
                }

                segments.Add(TrimIndent(currentLine, currentIndent).Trim());
                index++;
            }

            items.Add(
                new ListItemBlock(
                    InlineParser.Parse(string.Join(" ", segments.Where(static segment => !string.IsNullOrWhiteSpace(segment)))),
                    itemMatch.IsTask ? itemMatch.IsChecked : null,
                    children));
        }

        block = new ListBlock(ordered, items);
        return true;
    }

    private static ParagraphBlock ParseParagraph(string[] lines, ref int index, int end, int currentIndent)
    {
        var paragraphLines = new List<string>();

        while (index < end)
        {
            var line = lines[index];

            if (string.IsNullOrWhiteSpace(line))
            {
                break;
            }

            if (CountIndentation(line) < currentIndent)
            {
                break;
            }

            if (paragraphLines.Count > 0 && IsBlockBoundary(lines, index, currentIndent))
            {
                break;
            }

            paragraphLines.Add(TrimIndent(line, currentIndent).Trim());
            index++;
        }

        return new ParagraphBlock(InlineParser.Parse(string.Join(" ", paragraphLines)));
    }

    private static bool IsBlockBoundary(string[] lines, int index, int currentIndent)
    {
        if (TryMatchListItem(lines[index], out var listMatch) && listMatch.Indent >= currentIndent)
        {
            return true;
        }

        return IsNonListBlockBoundary(lines, index, currentIndent);
    }

    private static bool IsNonListBlockBoundary(string[] lines, int index, int currentIndent)
    {
        if (CountIndentation(lines[index]) != currentIndent)
        {
            return false;
        }

        var line = TrimIndent(lines[index], currentIndent);
        return line.StartsWith("```", StringComparison.Ordinal)
            || line.StartsWith('>')
            || HeadingPattern().IsMatch(line)
            || IsTableStart(lines, index, currentIndent);
    }

    private static bool TryMatchListItem(string line, out ListItemMatch match)
    {
        var indent = CountIndentation(line);
        var content = TrimIndent(line, indent);

        var unordered = UnorderedListPattern().Match(content);
        if (unordered.Success)
        {
            match = CreateListItemMatch(indent, false, unordered.Groups[1].Value.Trim());
            return true;
        }

        var ordered = OrderedListPattern().Match(content);
        if (ordered.Success)
        {
            match = CreateListItemMatch(indent, true, ordered.Groups[1].Value.Trim());
            return true;
        }

        match = default;
        return false;
    }

    private static string Normalize(string markdown)
    {
        return markdown
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Trim('\uFEFF');
    }

    private static bool IsTableStart(string[] lines, int index, int currentIndent)
    {
        return index + 1 < lines.Length
            && CountIndentation(lines[index]) == currentIndent
            && CountIndentation(lines[index + 1]) == currentIndent
            && LooksLikeTableRow(TrimIndent(lines[index], currentIndent))
            && TryParseTableSeparator(TrimIndent(lines[index + 1], currentIndent), out _);
    }

    private static bool LooksLikeTableRow(string line)
    {
        return line.Contains('|', StringComparison.Ordinal);
    }

    private static bool TryParseTableSeparator(string line, out IReadOnlyList<TableAlignment> alignments)
    {
        alignments = Array.Empty<TableAlignment>();
        var cells = ParseTableRow(line);
        if (cells.Count == 0)
        {
            return false;
        }

        var parsed = new List<TableAlignment>(cells.Count);
        foreach (var cell in cells)
        {
            var trimmed = cell.Trim();
            if (!TableSeparatorPattern().IsMatch(trimmed))
            {
                return false;
            }

            var alignment = trimmed.StartsWith(':') && trimmed.EndsWith(':')
                ? TableAlignment.Center
                : trimmed.StartsWith(':')
                    ? TableAlignment.Left
                    : trimmed.EndsWith(':')
                        ? TableAlignment.Right
                        : TableAlignment.None;
            parsed.Add(alignment);
        }

        alignments = parsed;
        return true;
    }

    private static List<string> ParseTableRow(string line)
    {
        var trimmed = line.Trim();
        if (trimmed.StartsWith('|'))
        {
            trimmed = trimmed[1..];
        }

        if (trimmed.EndsWith('|'))
        {
            trimmed = trimmed[..^1];
        }

        return trimmed.Split('|', StringSplitOptions.None).Select(static part => part.Trim()).ToList();
    }

    private static IReadOnlyList<string> NormalizeTableCells(IReadOnlyList<string> cells, int count)
    {
        var normalized = cells.Take(count).ToList();
        while (normalized.Count < count)
        {
            normalized.Add(string.Empty);
        }

        return normalized;
    }

    private static int CountIndentation(string line)
    {
        var width = 0;
        foreach (var character in line)
        {
            if (character == ' ')
            {
                width++;
                continue;
            }

            if (character == '\t')
            {
                width += 4;
                continue;
            }

            break;
        }

        return width;
    }

    private static string TrimIndent(string line, int indentation)
    {
        var consumedWidth = 0;
        var index = 0;

        while (index < line.Length && consumedWidth < indentation)
        {
            if (line[index] == ' ')
            {
                consumedWidth++;
                index++;
                continue;
            }

            if (line[index] == '\t')
            {
                consumedWidth += 4;
                index++;
                continue;
            }

            break;
        }

        return line[index..];
    }

    private static ListItemMatch CreateListItemMatch(int indent, bool ordered, string content)
    {
        var taskMatch = TaskListPattern().Match(content);
        if (taskMatch.Success)
        {
            return new ListItemMatch(
                indent,
                ordered,
                taskMatch.Groups[2].Value.Trim(),
                true,
                taskMatch.Groups[1].Value.Equals("x", StringComparison.OrdinalIgnoreCase));
        }

        return new ListItemMatch(indent, ordered, content, false, false);
    }

    [GeneratedRegex("^(#{1,6})\\s+(.*)$")]
    private static partial Regex HeadingPattern();

    [GeneratedRegex("^[-*+]\\s+(.*)$")]
    private static partial Regex UnorderedListPattern();

    [GeneratedRegex("^\\d+\\.\\s+(.*)$")]
    private static partial Regex OrderedListPattern();

    [GeneratedRegex("^\\[( |x|X)\\]\\s+(.*)$")]
    private static partial Regex TaskListPattern();

    [GeneratedRegex("^:?-{3,}:?$")]
    private static partial Regex TableSeparatorPattern();

    private readonly record struct ListItemMatch(int Indent, bool Ordered, string Content, bool IsTask, bool IsChecked);
}