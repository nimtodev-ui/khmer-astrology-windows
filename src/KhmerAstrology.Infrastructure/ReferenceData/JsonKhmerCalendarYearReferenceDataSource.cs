using System.IO.Compression;
using System.Text;
using KhmerAstrology.Domain.Interfaces;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Infrastructure.ReferenceData;

/// <summary>
/// Loads the workbook's 5,103-year automatic calendar lookup codes.
/// Years covered: -643 to 4459.
/// </summary>
public sealed class JsonKhmerCalendarYearReferenceDataSource : IKhmerCalendarYearReferenceDataSource
{
    private static readonly Lazy<string> CachedYearCodes = new(LoadYearCodes);

    public KhmerCalendarYearReference Get(int astronomicalYear)
    {
        if (astronomicalYear is < -643 or > 4459)
        {
            throw new ArgumentOutOfRangeException(
                nameof(astronomicalYear),
                $"The workbook automatic-calendar reference covers years -643 to 4459. Requested: {astronomicalYear}.");
        }

        var codes = CachedYearCodes.Value;
        int offset;
        if (astronomicalYear <= 2200)
        {
            offset = (astronomicalYear + 643) * 11;
        }
        else if (astronomicalYear <= 3000)
        {
            offset = 31284 + (astronomicalYear - 2201) * 11;
        }
        else
        {
            offset = 31284 + 8800 + (astronomicalYear - 3001) * 11;
        }

        var yearStr = codes.Substring(offset, 11);
        var bq2 = int.Parse(yearStr.AsSpan(0, 4)) - 2000;
        var br2 = int.Parse(yearStr.AsSpan(4, 1));
        var bs2 = int.Parse(yearStr.AsSpan(5, 1));
        var bt2 = int.Parse(yearStr.AsSpan(6, 1));
        var bu2 = int.Parse(yearStr.AsSpan(7, 2));
        var bv2 = int.Parse(yearStr.AsSpan(9, 2));

        return new KhmerCalendarYearReference(
            AstronomicalYear: astronomicalYear,
            KhmerYear: bq2,
            LunarYearType: br2,
            IntercalaryFlag1: bs2,
            IntercalaryFlag2: bt2,
            StartMonthIndex: bu2,
            StartTithiOffset: bv2);
    }

    private static string LoadYearCodes()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "khmer-calendar-year-codes.txt.gz");
        if (!File.Exists(path))
        {
            var fallback = Path.Combine(Directory.GetCurrentDirectory(), "Data", "khmer-calendar-year-codes.txt.gz");
            if (File.Exists(fallback))
            {
                path = fallback;
            }
            else
            {
                throw new FileNotFoundException("The extracted Khmer calendar year reference data is missing.", path);
            }
        }

        using var file = File.OpenRead(path);
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip, Encoding.ASCII);
        return reader.ReadToEnd();
    }
}
