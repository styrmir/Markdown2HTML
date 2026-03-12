using System.Text;
using System.Text.Encodings.Web;

namespace Markdown2Html;

public static class HtmlRenderer
{
    public static string Render(MarkdownDocument document, bool gitHubStyle = false)
    {
        var builder = new StringBuilder();

        for (var index = 0; index < document.Blocks.Count; index++)
        {
            if (index > 0)
            {
                builder.AppendLine();
            }

            RenderBlock(builder, document.Blocks[index], gitHubStyle);
        }

        return builder.ToString();
    }

    private static void RenderBlock(StringBuilder builder, BlockNode block, bool gitHubStyle)
    {
        switch (block)
        {
            case HeadingBlock heading:
                builder.Append('<').Append('h').Append(heading.Level).Append('>');
                RenderInlines(builder, heading.Inlines);
                builder.Append("</h").Append(heading.Level).Append('>');
                break;

            case ParagraphBlock paragraph:
                builder.Append("<p>");
                RenderInlines(builder, paragraph.Inlines);
                builder.Append("</p>");
                break;

            case CodeBlock codeBlock:
                builder.Append("<pre><code");
                if (!string.IsNullOrWhiteSpace(codeBlock.Language))
                {
                    builder.Append(" class=\"language-")
                        .Append(HtmlEncoder.Default.Encode(codeBlock.Language))
                        .Append("\"");
                }

                builder.Append('>');

                var highlightedCode = SyntaxHighlighter.HighlightCode(codeBlock.Code, codeBlock.Language);
                builder.Append(highlightedCode ?? HtmlEncoder.Default.Encode(codeBlock.Code));
                builder.Append("</code></pre>");
                break;

            case ListBlock list:
                var isTaskList = gitHubStyle && list.Items.Any(static item => item.IsChecked is not null);
                if (list.Ordered)
                {
                    builder.Append("<ol>");
                }
                else
                {
                    builder.Append(isTaskList ? "<ul class=\"contains-task-list\">" : "<ul>");
                }

                foreach (var item in list.Items)
                {
                    builder.Append(isTaskList && item.IsChecked is not null ? "<li class=\"task-list-item\">" : "<li>");
                    if (item.IsChecked is not null)
                    {
                        builder.Append("<input type=\"checkbox\" disabled");
                        if (item.IsChecked.Value)
                        {
                            builder.Append(" checked");
                        }

                        builder.Append('>');
                        if (item.Inlines.Count > 0)
                        {
                            builder.Append(' ');
                        }
                    }

                    RenderInlines(builder, item.Inlines);

                    foreach (var child in item.Children)
                    {
                        RenderBlock(builder, child, gitHubStyle);
                    }

                    builder.Append("</li>");
                }

                builder.Append(list.Ordered ? "</ol>" : "</ul>");
                break;

            case BlockQuoteBlock quote:
                builder.Append("<blockquote>");
                for (var index = 0; index < quote.Blocks.Count; index++)
                {
                    if (index > 0)
                    {
                        builder.AppendLine();
                    }

                    RenderBlock(builder, quote.Blocks[index], gitHubStyle);
                }

                builder.Append("</blockquote>");
                break;

            case TableBlock table:
                builder.Append("<table><thead><tr>");
                for (var column = 0; column < table.Headers.Count; column++)
                {
                    builder.Append("<th");
                    AppendAlignment(builder, table.Alignments[column]);
                    builder.Append('>');
                    RenderInlines(builder, table.Headers[column].Inlines);
                    builder.Append("</th>");
                }

                builder.Append("</tr></thead>");
                if (table.Rows.Count > 0)
                {
                    builder.Append("<tbody>");
                    foreach (var row in table.Rows)
                    {
                        builder.Append("<tr>");
                        for (var column = 0; column < row.Cells.Count; column++)
                        {
                            builder.Append("<td");
                            AppendAlignment(builder, table.Alignments[column]);
                            builder.Append('>');
                            RenderInlines(builder, row.Cells[column].Inlines);
                            builder.Append("</td>");
                        }

                        builder.Append("</tr>");
                    }

                    builder.Append("</tbody>");
                }

                builder.Append("</table>");
                break;

            default:
                throw new InvalidOperationException($"Unsupported block node type: {block.GetType().Name}");
        }
    }

    private static void RenderInlines(StringBuilder builder, IReadOnlyList<InlineNode> inlines)
    {
        foreach (var inline in inlines)
        {
            switch (inline)
            {
                case TextInline text:
                    builder.Append(HtmlEncoder.Default.Encode(text.Text));
                    break;

                case EmphasisInline emphasis:
                    builder.Append("<em>");
                    RenderInlines(builder, emphasis.Children);
                    builder.Append("</em>");
                    break;

                case StrongInline strong:
                    builder.Append("<strong>");
                    RenderInlines(builder, strong.Children);
                    builder.Append("</strong>");
                    break;

                case CodeInline code:
                    builder.Append("<code>")
                        .Append(HtmlEncoder.Default.Encode(code.Code))
                        .Append("</code>");
                    break;

                case LinkInline link:
                    builder.Append("<a href=\"")
                        .Append(HtmlEncoder.Default.Encode(SanitizeHref(link.Href)))
                        .Append("\">");
                    RenderInlines(builder, link.Children);
                    builder.Append("</a>");
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported inline node type: {inline.GetType().Name}");
            }
        }
    }

    private static string SanitizeHref(string href)
    {
        return href.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase)
            ? "#"
            : href;
    }

    private static void AppendAlignment(StringBuilder builder, TableAlignment alignment)
    {
        switch (alignment)
        {
            case TableAlignment.Left:
                builder.Append(" style=\"text-align:left\"");
                break;

            case TableAlignment.Center:
                builder.Append(" style=\"text-align:center\"");
                break;

            case TableAlignment.Right:
                builder.Append(" style=\"text-align:right\"");
                break;
        }
    }
}