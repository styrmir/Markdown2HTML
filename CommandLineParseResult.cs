namespace Markdown2Html;

public sealed class CommandLineParseResult
{
    private CommandLineParseResult(bool success, CliOptions? options, string? errorMessage)
    {
        Success = success;
        Options = options;
        ErrorMessage = errorMessage;
    }

    public bool Success { get; }

    public CliOptions? Options { get; }

    public string? ErrorMessage { get; }

    public static CommandLineParseResult SuccessResult(CliOptions options) => new(true, options, null);

    public static CommandLineParseResult Failure(string errorMessage) => new(false, null, errorMessage);
}