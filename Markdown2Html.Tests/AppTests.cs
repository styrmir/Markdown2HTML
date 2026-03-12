using System.Text;
using Xunit;

namespace Markdown2Html.Tests;

public sealed class AppTests
{
    [Fact]
    public async Task RunAsync_ConvertsFileInputToHtmlDocument()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"markdown2html-{Guid.NewGuid():N}.md");
        await File.WriteAllTextAsync(tempFile, "# Sample Title\n\nParagraph with **bold** text.");

        try
        {
            var standardOutput = new StringWriter();
            var standardError = new StringWriter();

            var exitCode = await App.RunAsync(
                ["--input", tempFile],
                new StringReader(string.Empty),
                standardOutput,
                standardError,
                isInputRedirected: false);

            Assert.Equal(0, exitCode);
            Assert.Equal(string.Empty, standardError.ToString());
            Assert.Contains("<!doctype html>", standardOutput.ToString());
            Assert.Contains("<title>markdown2html-", standardOutput.ToString(), StringComparison.OrdinalIgnoreCase);
            Assert.Contains("<h1>Sample Title</h1>", standardOutput.ToString());
            Assert.Contains("<p>Paragraph with <strong>bold</strong> text.</p>", standardOutput.ToString());
            Assert.Contains(AppInfo.CompanyName, standardOutput.ToString());
            Assert.Contains(AppInfo.CompanyWebsite, standardOutput.ToString());
            Assert.True(
                standardOutput.ToString().IndexOf(AppInfo.CompanyName, StringComparison.Ordinal)
                < standardOutput.ToString().IndexOf("<main>", StringComparison.Ordinal));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task RunAsync_ReadsMarkdownFromStandardInput()
    {
        var standardOutput = new StringWriter(new StringBuilder());
        var standardError = new StringWriter();

        var exitCode = await App.RunAsync(
            [],
            new StringReader("A [safe link](https://example.com) and `code`."),
            standardOutput,
            standardError,
            isInputRedirected: true);

        Assert.Equal(0, exitCode);
        Assert.Equal(string.Empty, standardError.ToString());
        Assert.Contains("<title>Document</title>", standardOutput.ToString());
        Assert.Contains("<p>A <a href=\"https://example.com\">safe link</a> and <code>code</code>.</p>", standardOutput.ToString());
    }

    [Fact]
    public async Task RunAsync_WithOpenAndOutput_InvokesOpenAction()
    {
        var inputFile = Path.Combine(Path.GetTempPath(), $"markdown2html-input-{Guid.NewGuid():N}.md");
        var outputFile = Path.Combine(Path.GetTempPath(), $"markdown2html-output-{Guid.NewGuid():N}.html");
        string? openedPath = null;

        await File.WriteAllTextAsync(inputFile, "# Open Test");

        try
        {
            var exitCode = await App.RunAsync(
                ["--input", inputFile, "--output", outputFile, "--open"],
                new StringReader(string.Empty),
                new StringWriter(),
                new StringWriter(),
                isInputRedirected: false,
                openOutputAction: path =>
                {
                    openedPath = path;
                    return Task.CompletedTask;
                });

            Assert.Equal(0, exitCode);
            Assert.Equal(outputFile, openedPath);
            Assert.True(File.Exists(outputFile));
        }
        finally
        {
            if (File.Exists(inputFile))
            {
                File.Delete(inputFile);
            }

            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }
        }
    }

    [Fact]
    public async Task RunAsync_ReturnsErrorForUnknownOption()
    {
        var standardOutput = new StringWriter();
        var standardError = new StringWriter();

        var exitCode = await App.RunAsync(
            ["--unknown"],
            new StringReader(string.Empty),
            standardOutput,
            standardError,
            isInputRedirected: false);

        Assert.Equal(1, exitCode);
        Assert.Equal(string.Empty, standardOutput.ToString());
        Assert.Contains("Unknown option: --unknown", standardError.ToString());
        Assert.Contains("Usage:", standardError.ToString());
    }

    [Fact]
    public async Task RunAsync_HelpIncludesCompanyInformation()
    {
        var standardOutput = new StringWriter();
        var standardError = new StringWriter();

        var exitCode = await App.RunAsync(
            ["--help"],
            new StringReader(string.Empty),
            standardOutput,
            standardError,
            isInputRedirected: false);

        Assert.Equal(0, exitCode);
        Assert.Equal(string.Empty, standardError.ToString());
        Assert.Contains(AppInfo.CompanyName, standardOutput.ToString());
        Assert.Contains(AppInfo.CompanyWebsite, standardOutput.ToString());
    }

    [Fact]
    public async Task RunAsync_WithoutParameters_ShowsNoInputMessageAndBrandedHelp()
    {
        var standardOutput = new StringWriter();
        var standardError = new StringWriter();

        var exitCode = await App.RunAsync(
            [],
            new StringReader(string.Empty),
            standardOutput,
            standardError,
            isInputRedirected: false);

        Assert.Equal(1, exitCode);
        Assert.Equal(string.Empty, standardOutput.ToString());
        Assert.Contains("No input was provided. Use --input <file> or pipe markdown through stdin.", standardError.ToString());
        Assert.Contains("Markdown2Html", standardError.ToString());
        Assert.Contains(AppInfo.CompanyName, standardError.ToString());
        Assert.Contains(AppInfo.CompanyWebsite, standardError.ToString());
    }

    [Fact]
    public async Task RunAsync_WithOpenWithoutOutput_ShowsErrorAndHelp()
    {
        var standardOutput = new StringWriter();
        var standardError = new StringWriter();

        var exitCode = await App.RunAsync(
            ["--open"],
            new StringReader(string.Empty),
            standardOutput,
            standardError,
            isInputRedirected: false);

        Assert.Equal(1, exitCode);
        Assert.Equal(string.Empty, standardOutput.ToString());
        Assert.Contains("The --open option requires --output <file>.", standardError.ToString());
        Assert.Contains("Markdown2Html", standardError.ToString());
    }

    [Fact]
    public void Render_ProducesExpectedHtmlForSupportedBlocks()
    {
        const string markdown = """
            > Quoted *text*

            - first
            - second

            ```csharp
            Console.WriteLine(42);
            ```
            """;

        var document = MarkdownParser.Parse(markdown);
        var html = HtmlRenderer.Render(document);

        Assert.Contains("<blockquote><p>Quoted <em>text</em></p></blockquote>", html);
        Assert.Contains("<ul><li>first</li><li>second</li></ul>", html);
        Assert.Contains("<pre><code class=\"language-csharp\">Console.WriteLine(42);</code></pre>", html);
    }

    [Fact]
    public void Render_ProducesExpectedHtmlForTablesTaskListsAndNestedLists()
    {
        const string markdown = """
            - [x] parent
              - child
            - [ ] next

            | Name | Score |
            | :--- | ---: |
            | Build | 10 |
            | Test | 8 |
            """;

        var document = MarkdownParser.Parse(markdown);
        var html = HtmlRenderer.Render(document);

        Assert.Contains("<ul><li><input type=\"checkbox\" disabled checked> parent<ul><li>child</li></ul></li><li><input type=\"checkbox\" disabled> next</li></ul>", html);
        Assert.Contains("<table><thead><tr><th style=\"text-align:left\">Name</th><th style=\"text-align:right\">Score</th></tr></thead><tbody><tr><td style=\"text-align:left\">Build</td><td style=\"text-align:right\">10</td></tr><tr><td style=\"text-align:left\">Test</td><td style=\"text-align:right\">8</td></tr></tbody></table>", html);
    }
}