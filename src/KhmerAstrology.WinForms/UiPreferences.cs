using System.Diagnostics;

namespace KhmerAstrology.WinForms;

/// <summary>
/// Remembers per-user UI choices (currently the display language) between runs.
/// Failures are logged and ignored so a read-only profile never blocks start-up.
/// </summary>
internal static class UiPreferences
{
    private const string KhmerCode = "km";
    private const string EnglishCode = "en";

    private static readonly string LanguageFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "KhmerAstrology",
        "ui-language.txt");

    public static bool LoadIsKhmer()
    {
        try
        {
            return !File.Exists(LanguageFilePath)
                || !string.Equals(File.ReadAllText(LanguageFilePath).Trim(), EnglishCode, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            Trace.WriteLine($"UI language preference could not be read: {exception.Message}");
            return true;
        }
    }

    public static void SaveIsKhmer(bool isKhmer)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LanguageFilePath)!);
            File.WriteAllText(LanguageFilePath, isKhmer ? KhmerCode : EnglishCode);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            Trace.WriteLine($"UI language preference could not be saved: {exception.Message}");
        }
    }
}
