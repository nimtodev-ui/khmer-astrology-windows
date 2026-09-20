using System.Drawing.Text;

namespace KhmerAstrology.WinForms;

/// <summary>
/// Loads the supplied Khmer fonts from the application folder without requiring
/// the fonts to be installed in Windows.
/// </summary>
public sealed class UiFontProvider : IDisposable
{
    private readonly PrivateFontCollection _privateFonts = new();
    private readonly FontFamily _bodyFontFamily;
    private readonly FontFamily _displayFontFamily;

    public UiFontProvider()
    {
        _bodyFontFamily = LoadFontFamily(
            "KhmerOSSiemreap-Regular.ttf",
            FontFamily.GenericSansSerif);
        _displayFontFamily = LoadFontFamily(
            "TOATHMOR WAT LANGKA OLDIE.otf",
            _bodyFontFamily);
    }

    public Font CreateBody(float size, FontStyle style = FontStyle.Regular) =>
        new(_bodyFontFamily, size, style);

    public Font CreateDisplay(float size, FontStyle style = FontStyle.Regular) =>
        new(_displayFontFamily, size, style);

    private FontFamily LoadFontFamily(string fileName, FontFamily fallback)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fonts", fileName);
        if (!File.Exists(path))
        {
            return fallback;
        }

        try
        {
            _privateFonts.AddFontFile(path);
            return _privateFonts.Families[^1];
        }
        catch
        {
            return fallback;
        }
    }

    public void Dispose()
    {
        _privateFonts.Dispose();
    }
}
