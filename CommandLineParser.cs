namespace Markdown2Html;

public static class CommandLineParser
{
    public static CommandLineParseResult Parse(string[] args)
    {
        string? inputPath = null;
        string? outputPath = null;
        var openWhenDone = false;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];

            switch (arg)
            {
                case "-h":
                case "--help":
                    return CommandLineParseResult.SuccessResult(new CliOptions { ShowHelp = true });

                case "-i":
                case "--input":
                    if (!TryReadValue(args, ref index, out var explicitInput))
                    {
                        return CommandLineParseResult.Failure("Missing value for --input.");
                    }

                    inputPath = explicitInput;
                    break;

                case "-o":
                case "--output":
                    if (!TryReadValue(args, ref index, out var explicitOutput))
                    {
                        return CommandLineParseResult.Failure("Missing value for --output.");
                    }

                    outputPath = explicitOutput;
                    break;

                case "-open":
                case "--open":
                    openWhenDone = true;
                    break;

                default:
                    if (arg.StartsWith("-", StringComparison.Ordinal))
                    {
                        return CommandLineParseResult.Failure($"Unknown option: {arg}");
                    }

                    if (inputPath is not null)
                    {
                        return CommandLineParseResult.Failure("Only one input file can be specified.");
                    }

                    inputPath = arg;
                    break;
            }
        }

        return CommandLineParseResult.SuccessResult(
            new CliOptions
            {
                InputPath = inputPath,
                OutputPath = outputPath,
                OpenWhenDone = openWhenDone,
                ShowHelp = false
            });
    }

    public static string GetHelpText(bool colorize = false)
    {
        var appName = colorize ? AnsiConsoleStyler.Heading("Markdown2Html") : "Markdown2Html";
        var usageHeading = colorize ? AnsiConsoleStyler.Section("Usage:") : "Usage:";
        var optionsHeading = colorize ? AnsiConsoleStyler.Section("Options:") : "Options:";
        var examplesHeading = colorize ? AnsiConsoleStyler.Section("Examples:") : "Examples:";
        var companyLine = colorize
            ? $"Created by {AnsiConsoleStyler.Accent(AppInfo.CompanyName)}"
            : $"Created by {AppInfo.CompanyName}";
        var websiteLine = colorize ? AnsiConsoleStyler.Subtle(AppInfo.CompanyWebsite) : AppInfo.CompanyWebsite;

        var usageFile = colorize
            ? AnsiConsoleStyler.Example("  markdown2html --input README.md --output README.html")
            : "  markdown2html --input README.md --output README.html";
        var usageStdout = colorize
            ? AnsiConsoleStyler.Example("  markdown2html README.md > README.html")
            : "  markdown2html README.md > README.html";
        var usagePipe = colorize
            ? AnsiConsoleStyler.Example("  type README.md | markdown2html")
            : "  type README.md | markdown2html";

        var exampleOpen = colorize
            ? AnsiConsoleStyler.Example("  markdown2html --input README.md --output README.html --open")
            : "  markdown2html --input README.md --output README.html --open";
        var examplePipe = colorize
            ? AnsiConsoleStyler.Example("  Get-Content notes.md | markdown2html > notes.html")
            : "  Get-Content notes.md | markdown2html > notes.html";
        var examplePositional = colorize
            ? AnsiConsoleStyler.Example("  markdown2html .\\test-md-files\\OutlookISyslu_UMBRA_Briefing.md --output C:\\temp\\briefing.html")
            : "  markdown2html .\\test-md-files\\OutlookISyslu_UMBRA_Briefing.md --output C:\\temp\\briefing.html";

        return
            $"{appName}{Environment.NewLine}{Environment.NewLine}" +
            $"Convert markdown from a file or stdin into a full HTML document.{Environment.NewLine}{Environment.NewLine}" +
            $"{companyLine}{Environment.NewLine}" +
            $"{websiteLine}{Environment.NewLine}{Environment.NewLine}" +
            $"{usageHeading}{Environment.NewLine}" +
            $"{usageFile}{Environment.NewLine}" +
            $"{usageStdout}{Environment.NewLine}" +
            $"{usagePipe}{Environment.NewLine}{Environment.NewLine}" +
            $"{optionsHeading}{Environment.NewLine}" +
            $"  -i, --input <file>    Read markdown from a file.{Environment.NewLine}" +
            $"  -o, --output <file>   Write HTML to a file. Defaults to stdout.{Environment.NewLine}" +
            $"  -open, --open         Open the generated HTML file after conversion.{Environment.NewLine}" +
            $"  -h, --help            Show this help text.{Environment.NewLine}{Environment.NewLine}" +
            $"{examplesHeading}{Environment.NewLine}" +
            $"{exampleOpen}{Environment.NewLine}" +
            $"{examplePipe}{Environment.NewLine}" +
            $"{examplePositional}";
    }

    private static bool TryReadValue(string[] args, ref int index, out string? value)
    {
        if (index + 1 >= args.Length)
        {
            value = null;
            return false;
        }

        index++;
        value = args[index];
        return true;
    }
}