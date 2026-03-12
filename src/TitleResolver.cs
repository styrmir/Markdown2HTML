namespace Markdown2Html;

public static class TitleResolver
{
    public static string Resolve(string? inputPath)
    {
        if (string.IsNullOrWhiteSpace(inputPath))
        {
            return "Document";
        }

        return Path.GetFileNameWithoutExtension(inputPath);
    }
}