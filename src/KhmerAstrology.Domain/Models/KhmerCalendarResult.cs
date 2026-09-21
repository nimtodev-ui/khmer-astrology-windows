namespace KhmerAstrology.Domain.Models;

public sealed record KhmerCalendarResult
{
    public int AstronomicalYear { get; init; }
    public int KhmerYear { get; init; }
    public int BuddhistYear { get; init; }
    public double Ahargana { get; init; }
    public int AharganaDay { get; init; }
    public double Kammaja { get; init; }
    public double SolarYearFraction { get; init; }

    public int Uccabal { get; init; }
    public int Avamana { get; init; }
    public int Masakendra { get; init; }
    public int BoriTithi { get; init; }
    public int NewEraDay { get; init; }
    public string NewEraWeekday { get; init; } = string.Empty;

    // Maha Sankranta (មហាសង្ក្រាន្ត) — Excel cells B31, B32, B39, B43, B44, B46
    public int MahaSankrantaDay { get; init; }
    public int MahaSankrantaMonth { get; init; } = 4;
    public TimeSpan MahaSankrantaTime { get; init; }
    public string MahaSankrantaWeekday { get; init; } = string.Empty;
    public int MahaSankrantaWeekdayNumber { get; init; }

    // Rise of Sak (ឡើងស័ក) — Excel cells B30, F7, B38, B41, B42, B45
    public int RiseOfSakDay { get; init; }
    public int RiseOfSakMonth { get; init; } = 4;
    public TimeSpan RiseOfSakTime { get; init; }
    public string RiseOfSakWeekday { get; init; } = string.Empty;
    public int RiseOfSakWeekdayNumber { get; init; }

    // Backward-compatible properties matching former schema
    public int NextMahaSankrantaDay { get; init; }
    public TimeSpan NextMahaSankrantaTime { get; init; }
    public string NextMahaSankrantaWeekday { get; init; } = string.Empty;
    public string YearType { get; init; } = string.Empty;
    public int DaysInYear { get; init; }
    public string LunarYearType { get; init; } = string.Empty;
    public string MonthLengthRule { get; init; } = string.Empty;
    public string WeekdayAdjustment { get; init; } = string.Empty;

    // Date-level fields sourced from the workbook's automatic calendar month
    // code table. They remain separate from the Ahargana intermediates above.
    public string Weekday { get; init; } = string.Empty;
    public int LunarDay { get; init; }
    public int LunarDayInPhase { get; init; }
    public string LunarPhase { get; init; } = string.Empty;
    public string LunarDayDisplay { get; init; } = string.Empty;
    public string TithiName { get; init; } = string.Empty;
    public int LunarMonthNumber { get; init; }
    public string LunarMonthNameEn { get; init; } = string.Empty;
    public string AnimalYear { get; init; } = string.Empty;
    public int SesaKalaYoga { get; init; }
    public int Yuga { get; init; }
    public string TraditionalDate { get; init; } = string.Empty;
}
