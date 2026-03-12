namespace Markdown2Html;

public static class AnsiConsoleStyler
{
    private const string Reset = "\u001b[0m";
    private const string BoldCyan = "\u001b[1;36m";
    private const string BoldYellow = "\u001b[1;33m";
    private const string Green = "\u001b[32m";
    private const string Dim = "\u001b[2m";
    private const string Magenta = "\u001b[35m";

    public static string Heading(string value) => Wrap(BoldCyan, value);

    public static string Section(string value) => Wrap(BoldYellow, value);

    public static string Example(string value) => Wrap(Green, value);

    public static string Accent(string value) => Wrap(Magenta, value);

    public static string Subtle(string value) => Wrap(Dim, value);

    private static string Wrap(string code, string value) => $"{code}{value}{Reset}";
}