using System.Text;
using Xunit;

namespace Markdown2Html.Tests;

public sealed class AppTests
{
    [Fact]
    public async Task RunAsync_ConvertsFileInputToHtmlDocument()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"md2html-{Guid.NewGuid():N}.md");
        var outputFile = Path.ChangeExtension(tempFile, ".html");
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
            Assert.Equal(string.Empty, standardOutput.ToString());
            var html = await File.ReadAllTextAsync(outputFile);
            Assert.Contains("<!doctype html>", html);
            Assert.Contains("<title>md2html-", html, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("<h1>Sample Title</h1>", html);
            Assert.Contains("<p>Paragraph with <strong>bold</strong> text.</p>", html);
            Assert.Contains(AppInfo.CompanyName, html);
            Assert.Contains(AppInfo.CompanyWebsite, html);
            Assert.True(
                html.IndexOf(AppInfo.CompanyName, StringComparison.Ordinal)
                > html.IndexOf("</main>", StringComparison.Ordinal));
        }
        finally
        {
            File.Delete(tempFile);
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }
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
        var inputFile = Path.Combine(Path.GetTempPath(), $"md2html-input-{Guid.NewGuid():N}.md");
        var outputFile = Path.Combine(Path.GetTempPath(), $"md2html-output-{Guid.NewGuid():N}.html");
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
    public async Task RunAsync_WithPositionalInput_WritesSiblingHtmlByDefault()
    {
        var inputFile = Path.Combine(Path.GetTempPath(), $"md2html-positional-{Guid.NewGuid():N}.md");
        var outputFile = Path.ChangeExtension(inputFile, ".html");

        await File.WriteAllTextAsync(inputFile, "# Positional Test");

        try
        {
            var exitCode = await App.RunAsync(
                [inputFile],
                new StringReader(string.Empty),
                new StringWriter(),
                new StringWriter(),
                isInputRedirected: false);

            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(outputFile));
            Assert.Contains("<h1>Positional Test</h1>", await File.ReadAllTextAsync(outputFile));
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
        Assert.Contains($"Version {AppInfo.Version}", standardOutput.ToString());
        Assert.Contains(AppInfo.DownloadsPage, standardOutput.ToString());
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
        Assert.Contains("md2html", standardError.ToString());
        Assert.Contains($"Version {AppInfo.Version}", standardError.ToString());
        Assert.Contains(AppInfo.CompanyName, standardError.ToString());
        Assert.Contains(AppInfo.CompanyWebsite, standardError.ToString());
        Assert.Contains(AppInfo.DownloadsPage, standardError.ToString());
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
        Assert.Contains("The --open option requires a file-based output. Use an input file or specify --output <file>.", standardError.ToString());
        Assert.Contains("md2html", standardError.ToString());
    }

    [Fact]
    public async Task RunAsync_WithOpenAndPositionalInput_OpensDerivedOutputFile()
    {
        var inputFile = Path.Combine(Path.GetTempPath(), $"md2html-open-{Guid.NewGuid():N}.md");
        var outputFile = Path.ChangeExtension(inputFile, ".html");
        string? openedPath = null;

        await File.WriteAllTextAsync(inputFile, "# Open Derived Test");

        try
        {
            var exitCode = await App.RunAsync(
                [inputFile, "--open"],
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
    public void Render_ProducesExpectedHtmlForSupportedBlocks()
    {
        const string markdown = """
            > Quoted *text*

            - first
            - second

            ```csharp
            public static string Name = "Sample";
            ```
            """;

        var document = MarkdownParser.Parse(markdown);
        var html = HtmlRenderer.Render(document);

        Assert.Contains("<blockquote><p>Quoted <em>text</em></p></blockquote>", html);
        Assert.Contains("<ul><li>first</li><li>second</li></ul>", html);
        Assert.Contains("<pre><code class=\"language-csharp\">", html);
        Assert.Contains("<span class=\"tok-keyword\">public</span>", html);
        Assert.Contains("<span class=\"tok-type\">string</span>", html);
        Assert.Contains("<span class=\"tok-string\">&quot;Sample&quot;</span>", html);
    }

    [Fact]
    public void Render_HighlightsHtmlAndJsonCodeBlocks()
    {
        const string markdown = """
            ```html
            <div class="note">Hello</div>
            ```

            ```json
            { "name": "md2html", "enabled": true }
            ```
            """;

        var document = MarkdownParser.Parse(markdown);
        var html = HtmlRenderer.Render(document);

        Assert.Contains("<span class=\"tok-tag\">div</span>", html);
        Assert.Contains("<span class=\"tok-attr-name\">class</span>", html);
        Assert.Contains("<span class=\"tok-attr-value\">&quot;note&quot;</span>", html);
        Assert.Contains("<span class=\"tok-property\">&quot;name&quot;</span>", html);
        Assert.Contains("<span class=\"tok-literal\">true</span>", html);
    }

    [Fact]
    public void Render_FallsBackToPlainCodeForUnsupportedLanguages()
    {
        const string markdown = """
            ```brainfuck
            ++>---.
            ```
            """;

        var document = MarkdownParser.Parse(markdown);
        var html = HtmlRenderer.Render(document);

        Assert.Contains("<pre><code class=\"language-brainfuck\">&#x2B;&#x2B;&gt;---.</code></pre>", html);
        Assert.DoesNotContain("tok-keyword", html);
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

    [Fact]
    public void Render_GitHubStyle_AddsTaskListClasses()
    {
        const string markdown = """
            - [x] done
            - [ ] todo
            """;

        var document = MarkdownParser.Parse(markdown);
        var html = HtmlRenderer.Render(document, gitHubStyle: true);

        Assert.Contains("<ul class=\"contains-task-list\">", html);
        Assert.Contains("<li class=\"task-list-item\">", html);
    }

    [Fact]
    public async Task RunAsync_WithGitHubStyle_UsesGitHubTheme()
    {
        var standardOutput = new StringWriter(new StringBuilder());
        var standardError = new StringWriter();

        var exitCode = await App.RunAsync(
            ["--github-style"],
            new StringReader("# Hello"),
            standardOutput,
            standardError,
            isInputRedirected: true);

        Assert.Equal(0, exitCode);
        var html = standardOutput.ToString();
        Assert.Contains("border-bottom: 1px solid", html);
        Assert.Contains("BlinkMacSystemFont", html);
    }
}