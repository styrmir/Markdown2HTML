namespace Markdown2Html;

public sealed class CliOptions
{
    public string? InputPath { get; init; }

    public string? OutputPath { get; init; }

    public bool OpenWhenDone { get; init; }

    public bool ShowHelp { get; init; }
}