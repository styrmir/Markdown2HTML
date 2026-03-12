using System.Text;
using System.Text.Encodings.Web;

namespace Markdown2Html;

public static class HtmlDocumentBuilder
{
    public static string Build(string title, string htmlFragment)
    {
        var encodedTitle = HtmlEncoder.Default.Encode(title);
        var encodedCompanyName = HtmlEncoder.Default.Encode(AppInfo.CompanyName);
        var encodedCompanyWebsite = HtmlEncoder.Default.Encode(AppInfo.CompanyWebsite);
        var builder = new StringBuilder();

        builder.AppendLine("<!doctype html>");
        builder.AppendLine("<html lang=\"en\">");
        builder.AppendLine("<head>");
        builder.AppendLine("  <meta charset=\"utf-8\">");
        builder.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
        builder.Append("  <title>").Append(encodedTitle).AppendLine("</title>");
        builder.AppendLine("  <style>");
        builder.AppendLine("    :root { color-scheme: light; }");
        builder.AppendLine("    body { font-family: Georgia, 'Times New Roman', serif; margin: 2rem auto; max-width: 72ch; line-height: 1.6; padding: 0 1rem; color: #1f2328; }");
        builder.AppendLine("    pre { background: #f6f8fa; border-radius: 8px; overflow-x: auto; padding: 1rem; }");
        builder.AppendLine("    code { font-family: Consolas, 'Courier New', monospace; }");
        builder.AppendLine("    .tok-keyword { color: #0f3d91; font-weight: 600; }");
        builder.AppendLine("    .tok-string { color: #8f4e08; }");
        builder.AppendLine("    .tok-comment { color: #5f6b7a; font-style: italic; }");
        builder.AppendLine("    .tok-number { color: #7c2d8c; }");
        builder.AppendLine("    .tok-type { color: #005e7a; }");
        builder.AppendLine("    .tok-literal { color: #7d330f; font-weight: 600; }");
        builder.AppendLine("    .tok-tag { color: #0550ae; }");
        builder.AppendLine("    .tok-attr-name { color: #6f42c1; }");
        builder.AppendLine("    .tok-attr-value { color: #0a7f45; }");
        builder.AppendLine("    .tok-property { color: #953800; }");
        builder.AppendLine("    blockquote { border-left: 4px solid #d0d7de; margin-left: 0; padding-left: 1rem; color: #57606a; }");
        builder.AppendLine("    table { border-collapse: collapse; margin: 1.5rem 0; width: 100%; }");
        builder.AppendLine("    th, td { border: 1px solid #d0d7de; padding: 0.6rem 0.75rem; vertical-align: top; }");
        builder.AppendLine("    th { background: #f6f8fa; } ");
        builder.AppendLine("    input[type=checkbox] { margin-right: 0.45rem; }");
        builder.AppendLine("    footer { border-top: 1px solid #d0d7de; color: #6e7781; font-size: 0.8rem; margin-top: 2rem; padding-top: 0.85rem; } ");
        builder.AppendLine("    .app-meta { margin: 0; } ");
        builder.AppendLine("    a { color: #0969da; }");
        builder.AppendLine("  </style>");
        builder.AppendLine("</head>");
        builder.AppendLine("<body>");
        builder.AppendLine("  <main>");
        builder.AppendLine(Indent(htmlFragment, "    "));
        builder.AppendLine("  </main>");
        builder.Append("  <footer><p class=\"app-meta\">Generated with md2html by ")
            .Append(encodedCompanyName)
            .Append(" · <a href=\"")
            .Append(encodedCompanyWebsite)
            .AppendLine("\">abra.is</a></p></footer>");
        builder.AppendLine("</body>");
        builder.AppendLine("</html>");

        return builder.ToString();
    }

    private static string Indent(string value, string indentation)
    {
        var lines = value.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        return string.Join(Environment.NewLine, lines.Select(static (line, index) => (line, index)).Select(pair => pair.index == lines.Length - 1 && pair.line.Length == 0 ? string.Empty : indentation + pair.line));
    }
}