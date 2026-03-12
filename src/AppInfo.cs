using System.Reflection;

namespace Markdown2Html;

public static class AppInfo
{
    public const string CompanyName = "T1/Abra ehf";

    public const string CompanyWebsite = "https://abra.is";

    public const string DownloadsPage = "https://github.com/styrmir/Markdown2HTML/releases";

    public static string Version
    {
        get
        {
            var informationalVersion = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;

            if (!string.IsNullOrWhiteSpace(informationalVersion))
            {
                return informationalVersion.Split('+', 2)[0];
            }

            return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
        }
    }
}