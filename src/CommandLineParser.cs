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
        var appName = colorize ? AnsiConsoleStyler.Heading("md2html") : "md2html";
        var versionLine = colorize ? AnsiConsoleStyler.Subtle($"Version {AppInfo.Version}") : $"Version {AppInfo.Version}";
        var downloadsLine = colorize ? AnsiConsoleStyler.Subtle($"Downloads {AppInfo.DownloadsPage}") : $"Downloads {AppInfo.DownloadsPage}";
        var usageHeading = colorize ? AnsiConsoleStyler.Section("Usage:") : "Usage:";
        var optionsHeading = colorize ? AnsiConsoleStyler.Section("Options:") : "Options:";
        var examplesHeading = colorize ? AnsiConsoleStyler.Section("Examples:") : "Examples:";
        var companyLine = colorize
            ? $"Created by {AnsiConsoleStyler.Accent(AppInfo.CompanyName)}"
            : $"Created by {AppInfo.CompanyName}";
        var websiteLine = colorize ? AnsiConsoleStyler.Subtle(AppInfo.CompanyWebsite) : AppInfo.CompanyWebsite;

        var usageFile = colorize
            ? AnsiConsoleStyler.Example("  md2html README.md")
            : "  md2html README.md";
        var usageStdout = colorize
            ? AnsiConsoleStyler.Example("  md2html README.md > README.html")
            : "  md2html README.md > README.html";
        var usagePipe = colorize
            ? AnsiConsoleStyler.Example("  type README.md | md2html")
            : "  type README.md | md2html";
        var usageExplicit = colorize
            ? AnsiConsoleStyler.Example("  md2html --input README.md --output README.html")
            : "  md2html --input README.md --output README.html";

        var exampleOpen = colorize
            ? AnsiConsoleStyler.Example("  md2html README.md --open")
            : "  md2html README.md --open";
        var examplePipe = colorize
            ? AnsiConsoleStyler.Example("  Get-Content notes.md | md2html > notes.html")
            : "  Get-Content notes.md | md2html > notes.html";
        var examplePositional = colorize
            ? AnsiConsoleStyler.Example("  md2html .\\docs\\sample.md")
            : "  md2html .\\docs\\sample.md";
        var exampleExplicit = colorize
            ? AnsiConsoleStyler.Example("  md2html --input README.md --output C:\\temp\\README.html")
            : "  md2html --input README.md --output C:\\temp\\README.html";

        return
            $"{appName}{Environment.NewLine}{Environment.NewLine}" +
            $"{versionLine}{Environment.NewLine}{Environment.NewLine}" +
            $"Convert markdown from a file or stdin into a full HTML document.{Environment.NewLine}{Environment.NewLine}" +
            $"{companyLine}{Environment.NewLine}" +
            $"{websiteLine}{Environment.NewLine}" +
            $"{downloadsLine}{Environment.NewLine}{Environment.NewLine}" +
            $"{usageHeading}{Environment.NewLine}" +
            $"{usageFile}{Environment.NewLine}" +
                $"{usageExplicit}{Environment.NewLine}" +
            $"{usageStdout}{Environment.NewLine}" +
            $"{usagePipe}{Environment.NewLine}{Environment.NewLine}" +
            $"{optionsHeading}{Environment.NewLine}" +
                $"  -i, --input <file>    Read markdown from a file. The first positional argument also works.{Environment.NewLine}" +
                $"  -o, --output <file>   Write HTML to a specific file. Defaults to <input>.html for file input.{Environment.NewLine}" +
                $"  -open, --open         Open the generated HTML file after conversion.{Environment.NewLine}" +
            $"  -h, --help            Show this help text.{Environment.NewLine}{Environment.NewLine}" +
            $"{examplesHeading}{Environment.NewLine}" +
            $"{exampleOpen}{Environment.NewLine}" +
            $"{examplePipe}{Environment.NewLine}" +
                $"{examplePositional}{Environment.NewLine}" +
                $"{exampleExplicit}";
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