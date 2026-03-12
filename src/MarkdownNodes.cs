namespace Markdown2Html;

public sealed record MarkdownDocument(IReadOnlyList<BlockNode> Blocks);

public abstract record BlockNode;

public sealed record HeadingBlock(int Level, IReadOnlyList<InlineNode> Inlines) : BlockNode;

public sealed record ParagraphBlock(IReadOnlyList<InlineNode> Inlines) : BlockNode;

public sealed record CodeBlock(string? Language, string Code) : BlockNode;

public sealed record ListBlock(bool Ordered, IReadOnlyList<ListItemBlock> Items) : BlockNode;

public sealed record ListItemBlock(IReadOnlyList<InlineNode> Inlines, bool? IsChecked, IReadOnlyList<BlockNode> Children);

public sealed record BlockQuoteBlock(IReadOnlyList<BlockNode> Blocks) : BlockNode;

public sealed record TableBlock(
	IReadOnlyList<TableAlignment> Alignments,
	IReadOnlyList<TableCell> Headers,
	IReadOnlyList<TableRow> Rows) : BlockNode;

public sealed record TableRow(IReadOnlyList<TableCell> Cells);

public sealed record TableCell(IReadOnlyList<InlineNode> Inlines);

public enum TableAlignment
{
	None,
	Left,
	Center,
	Right
}

public abstract record InlineNode;

public sealed record TextInline(string Text) : InlineNode;

public sealed record EmphasisInline(IReadOnlyList<InlineNode> Children) : InlineNode;

public sealed record StrongInline(IReadOnlyList<InlineNode> Children) : InlineNode;

public sealed record CodeInline(string Code) : InlineNode;

public sealed record LinkInline(string Href, IReadOnlyList<InlineNode> Children) : InlineNode;