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
    public int MahaSankrantaDay { get; init; }
    public TimeSpan MahaSankrantaTime { get; init; }
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
