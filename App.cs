using System.Diagnostics;

namespace Markdown2Html;

public static class App
{
    private const string NoInputMessage = "No input was provided. Use --input <file> or pipe markdown through stdin.";
    private const string OpenRequiresOutputMessage = "The --open option requires --output <file>.";

    public static async Task<int> RunAsync(
        string[] args,
        TextReader standardInput,
        TextWriter standardOutput,
        TextWriter standardError,
        bool isInputRedirected,
        Func<string, Task>? openOutputAction = null,
        bool colorizeCli = false)
    {
        var parseResult = CommandLineParser.Parse(args);

        if (!parseResult.Success)
        {
            await standardError.WriteLineAsync(parseResult.ErrorMessage);
            await standardError.WriteLineAsync();
            await standardError.WriteLineAsync(CommandLineParser.GetHelpText(colorizeCli));
            return 1;
        }

        var options = parseResult.Options!;

        if (options.OpenWhenDone && string.IsNullOrWhiteSpace(options.OutputPath))
        {
            await standardError.WriteLineAsync(OpenRequiresOutputMessage);
            await standardError.WriteLineAsync();
            await standardError.WriteLineAsync(CommandLineParser.GetHelpText(colorizeCli));
            return 1;
        }

        if (options.ShowHelp)
        {
            await standardOutput.WriteLineAsync(CommandLineParser.GetHelpText(colorizeCli));
            return 0;
        }

        try
        {
            var markdown = await ReadMarkdownAsync(options, standardInput, isInputRedirected);
            var document = MarkdownParser.Parse(markdown);
            var htmlFragment = HtmlRenderer.Render(document);
            var title = TitleResolver.Resolve(options.InputPath);
            var htmlDocument = HtmlDocumentBuilder.Build(title, htmlFragment);

            if (string.IsNullOrWhiteSpace(options.OutputPath))
            {
                await standardOutput.WriteAsync(htmlDocument);
                return 0;
            }

            await File.WriteAllTextAsync(options.OutputPath, htmlDocument);

            if (options.OpenWhenDone)
            {
                await (openOutputAction ?? OpenOutputFileAsync)(options.OutputPath);
            }

            return 0;
        }
        catch (FileNotFoundException exception)
        {
            await standardError.WriteLineAsync($"Input file not found: {exception.FileName}");
            return 1;
        }
        catch (DirectoryNotFoundException exception)
        {
            await standardError.WriteLineAsync(exception.Message);
            return 1;
        }
        catch (IOException exception)
        {
            await standardError.WriteLineAsync(exception.Message);

            if (string.Equals(exception.Message, NoInputMessage, StringComparison.Ordinal))
            {
                await standardError.WriteLineAsync();
                await standardError.WriteLineAsync(CommandLineParser.GetHelpText(colorizeCli));
            }

            return 1;
        }
    }

    private static async Task<string> ReadMarkdownAsync(CliOptions options, TextReader standardInput, bool isInputRedirected)
    {
        if (!string.IsNullOrWhiteSpace(options.InputPath))
        {
            return await File.ReadAllTextAsync(options.InputPath);
        }

        if (!isInputRedirected)
        {
            throw new IOException(NoInputMessage);
        }

        return await standardInput.ReadToEndAsync();
    }

    private static Task OpenOutputFileAsync(string outputPath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = Path.GetFullPath(outputPath),
            UseShellExecute = true
        };

        var process = Process.Start(startInfo);
        if (process is null)
        {
            throw new IOException($"Could not open output file: {outputPath}");
        }

        return Task.CompletedTask;
    }
}