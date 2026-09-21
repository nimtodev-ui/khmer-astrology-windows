using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace KhmerAstrology.WinForms;

/// <summary>
/// Loads the supplied Khmer fonts from the application folder without requiring
/// the fonts to be installed in Windows.
/// Registers fonts with both GDI+ (PrivateFontCollection) and Win32 GDI (AddFontResourceEx)
/// so that TextRenderer, Graphics.DrawString, and standard WinForms controls render
/// Khmer text with genuine font metrics and proper ligature shaping.
/// </summary>
public sealed class UiFontProvider : IDisposable
{
    [DllImport("gdi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern int AddFontResourceEx(string lpszFilename, uint fl, IntPtr pdv);

    [DllImport("gdi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool RemoveFontResourceEx(string lpFileName, uint fl, IntPtr pdv);

    private const uint FR_PRIVATE = 0x10;

    private readonly PrivateFontCollection _privateFonts = new();
    private readonly List<string> _loadedPaths = [];
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
            if (OperatingSystem.IsWindows())
            {
                var added = AddFontResourceEx(path, FR_PRIVATE, IntPtr.Zero);
                if (added > 0)
                {
                    _loadedPaths.Add(path);
                }
            }

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
        if (OperatingSystem.IsWindows())
        {
            foreach (var path in _loadedPaths)
            {
                try
                {
                    RemoveFontResourceEx(path, FR_PRIVATE, IntPtr.Zero);
                }
                catch
                {
                    // Best effort cleanup
                }
            }
        }

        _privateFonts.Dispose();
    }
}
