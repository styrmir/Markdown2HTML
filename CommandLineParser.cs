namespace Markdown2Html;

public static class CommandLineParser
{
    public static CommandLineParseResult Parse(string[] args)
    {
        string? inputPath = null;
        string? outputPath = null;

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
                ShowHelp = false
            });
    }

    public static string GetHelpText()
    {
        return """
            Markdown2Html

            Convert markdown from a file or stdin into a full HTML document.

            Usage:
                            markdown2html --input README.md --output README.html
                            markdown2html README.md > README.html
                            type README.md | markdown2html

            Options:
              -i, --input <file>    Read markdown from a file.
              -o, --output <file>   Write HTML to a file. Defaults to stdout.
              -h, --help            Show this help text.
            """;
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