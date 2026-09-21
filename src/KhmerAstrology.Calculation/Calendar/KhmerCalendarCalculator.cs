using System.Globalization;
using System.Text;
using KhmerAstrology.Calculation.Interfaces;
using KhmerAstrology.Domain.Interfaces;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Calculation.Calendar;

/// <summary>
/// Ports the calendar/Ahargana chain from the workbook's អដ្ឋភុជ្ជ sheet.
/// The source formulas are recorded in EXCEL_FORMULA_MAPPING.md (B33:B47).
/// </summary>
public sealed class KhmerCalendarCalculator : IKhmerCalendarCalculator
{
    /// <summary>
    /// Parses a CE or BCE year from an input string.
    /// Handles Arabic digits (e.g. 2026, -500), Khmer numerals (e.g. ២០២៦, -៥០០),
    /// Unicode minus/dashes (e.g. −500, –500), and textual tags (e.g. 500 មុន គ.ស., 2026 គ.ស.).
    /// </summary>
    public static bool TryParseYear(string? raw, out int year)
    {
        year = 0;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var text = raw.Trim();
        text = text.Replace('−', '-').Replace('–', '-').Replace('—', '-');
        var isNegative = text.Contains("មុន", StringComparison.OrdinalIgnoreCase) || text.Contains('-');

        var sb = new StringBuilder();
        foreach (var ch in text)
        {
            if (ch is >= '០' and <= '៩')
            {
                sb.Append((char)('0' + (ch - '០')));
            }
            else if (char.IsDigit(ch))
            {
                sb.Append(ch);
            }
        }

        if (sb.Length == 0)
        {
            return false;
        }

        if (!int.TryParse(sb.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var absYear) || absYear == 0)
        {
            return false;
        }

        year = isNegative ? -absYear : absYear;
        return true;
    }
    private static readonly string[] LunarMonthNames =
    [
        "Migasira", "Phussa", "Magha", "Phalguna", "Citta", "Visakha", "Jettha",
        "Asalha", "Savana", "Bhadrapada", "Assayuja", "Kattika", "Kattika",
    ];

    private static readonly string[] LeapLunarMonthNames =
    [
        "Migasira", "Phussa", "Magha", "Phalguna", "Citta", "Visakha", "Jettha",
        "First Asalha", "Second Asalha", "Savana", "Bhadrapada", "Assayuja", "Kattika",
    ];

    private static readonly string[] AnimalYears =
    ["Rat", "Ox", "Tiger", "Rabbit", "Dragon", "Snake", "Horse", "Goat", "Monkey", "Rooster", "Dog", "Pig"];

    private readonly IKhmerCalendarMonthReferenceDataSource? _monthReferenceDataSource;

    public KhmerCalendarCalculator(IKhmerCalendarMonthReferenceDataSource? monthReferenceDataSource = null)
    {
        _monthReferenceDataSource = monthReferenceDataSource;
    }

    private static readonly string[] Weekdays =
    [
        "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday",
    ];

    public KhmerCalendarResult Calculate(BirthInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var astronomicalYear = input.BirthDate.Year;
        var result = CalculateCore(
            astronomicalYear,
            input.BirthDate.Day,
            input.BirthDate.Month,
            input.BirthTime.ToTimeSpan(),
            CalculateBuddhistYear(astronomicalYear, input.BirthDate.Month, input.BirthDate.Day),
            input.BirthDate.DayOfWeek.ToString());

        return _monthReferenceDataSource is null
            ? result
            : ApplyDateLevelCalendar(result, input);
    }

    public KhmerCalendarResult CalculateForYear(
        int ceOrBceYear,
        int day,
        int month,
        TimeOnly time)
    {
        if (ceOrBceYear == 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ceOrBceYear),
                "Year 0 is not valid. Enter a positive CE year or a negative BCE year.");
        }

        if (month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
        }

        var astronomicalYear = ceOrBceYear < 0 ? ceOrBceYear + 1 : ceOrBceYear;
        var daysInMonth = DaysInMonth(astronomicalYear, month);
        if (day is < 1 || day > daysInMonth)
        {
            throw new ArgumentOutOfRangeException(nameof(day), $"Day must be between 1 and {daysInMonth} for the selected month.");
        }

        return CalculateCore(
            astronomicalYear,
            day,
            month,
            time.ToTimeSpan(),
            CalculateBuddhistYear(astronomicalYear, month, day),
            string.Empty);
    }

    private static KhmerCalendarResult CalculateCore(
        int astronomicalYear,
        int day,
        int month,
        TimeSpan time,
        int buddhistYear,
        string weekday)
    {
        // Workbook អដ្ឋភុជ្ជ!B35, using សូរ្យយាត្រ!B4:B9.
        var ahargana = CalculateAhargana(astronomicalYear, day, month, time.TotalDays);
        var aharganaDay = (int)Math.Floor(ahargana);
        var kammaja = IsWholeNumber(ahargana)
            ? 800D
            : 800D - Mod(ahargana, 1D) * 800D;

        var solarYearFraction = Mod((astronomicalYear - 638D) * 292_207D + 373D, 800D);
        var avamana = (int)Mod(aharganaDay * 11D + 650D, 692D);
        var masakendra = (int)Math.Floor((aharganaDay * 703D + 650D) / 20_760D);
        var boriTithi = (int)Mod(aharganaDay + Math.Floor((aharganaDay * 11D + 650D) / 692D), 30D);
        var newEraDay = (int)Mod(aharganaDay, 7D);

        // In workbook អដ្ឋភុជ្ជ:
        // - B38/B41/B42/B45 (correction = 0) is ឡើងស័ក (Rise of Sak / New Era epoch).
        // - B39/B43/B44/B46 (correction = 2.165 days) is មហាសង្ក្រាន្ត (Maha Sankranta).
        var riseOfSak = CalculateSankranta(astronomicalYear, 0D);
        var mahaSankranta = CalculateSankranta(astronomicalYear, 2.165D);
        var yearType = kammaja is >= 1 and <= 207
            ? "Leap Year"
            : kammaja is >= 208 and <= 800
                ? "Common Year"
                : "Check Required";
        var daysInYear = kammaja <= 207 ? 366 : 365;
        var lunarYearType = boriTithi is 24 or 25 or 26 or 27 or 29 or >= 0 and <= 5
            ? "13-Month Lunar Year"
            : boriTithi is >= 6 and <= 25
                ? "12-Month Lunar Year"
                : "Check Required";

        return new KhmerCalendarResult
        {
            AstronomicalYear = astronomicalYear,
            KhmerYear = astronomicalYear - 638,
            BuddhistYear = buddhistYear,
            Ahargana = ahargana,
            AharganaDay = aharganaDay,
            Kammaja = kammaja,
            SolarYearFraction = solarYearFraction,
            Uccabal = (int)Mod(aharganaDay + 2_611D, 3_232D),
            Avamana = avamana,
            Masakendra = masakendra,
            BoriTithi = boriTithi,
            NewEraDay = newEraDay,
            NewEraWeekday = Weekdays[riseOfSak.WeekdayNumber - 1],

            // Maha Sankranta (មហាសង្ក្រាន្ត) — Excel B31, B32, B39, B43, B44, B46
            MahaSankrantaDay = mahaSankranta.Day,
            MahaSankrantaMonth = mahaSankranta.Month,
            MahaSankrantaTime = mahaSankranta.Time,
            MahaSankrantaWeekday = Weekdays[mahaSankranta.WeekdayNumber - 1],
            MahaSankrantaWeekdayNumber = mahaSankranta.WeekdayNumber,

            // Rise of Sak (ឡើងស័ក) — Excel B30, F7, B38, B41, B42, B45
            RiseOfSakDay = riseOfSak.Day,
            RiseOfSakMonth = riseOfSak.Month,
            RiseOfSakTime = riseOfSak.Time,
            RiseOfSakWeekday = Weekdays[riseOfSak.WeekdayNumber - 1],
            RiseOfSakWeekdayNumber = riseOfSak.WeekdayNumber,

            // Backward-compatible properties
            NextMahaSankrantaDay = riseOfSak.Day,
            NextMahaSankrantaTime = riseOfSak.Time,
            NextMahaSankrantaWeekday = Weekdays[riseOfSak.WeekdayNumber - 1],

            YearType = yearType,
            DaysInYear = daysInYear,
            LunarYearType = lunarYearType,
            MonthLengthRule = kammaja <= 207 ? "30-day month" : "29-day month",
            WeekdayAdjustment = kammaja >= 208 ? "Advance 1 Weekday" : "Skip 1 Weekday",
            Weekday = weekday,
        };
    }

    private static int CalculateBuddhistYear(int astronomicalYear, int month, int day)
    {
        var leapYearFlag = IsLeapYear(astronomicalYear) ? 1 : 0;
        var dayOfYear = CalculateDayOfYear(astronomicalYear, month, day);
        return astronomicalYear + 543 + (dayOfYear >= 106 + leapYearFlag ? 1 : 0);
    }

    private static int CalculateDayOfYear(int year, int month, int day)
    {
        var dayOfYear = day;
        for (var currentMonth = 1; currentMonth < month; currentMonth++)
        {
            dayOfYear += DaysInMonth(year, currentMonth);
        }

        return dayOfYear;
    }

    private static int DaysInMonth(int year, int month) => month switch
    {
        2 => IsLeapYear(year) ? 29 : 28,
        4 or 6 or 9 or 11 => 30,
        _ => 31,
    };

    private static bool IsLeapYear(int year) =>
        year % 400 == 0 || year % 4 == 0 && year % 100 != 0;

    private KhmerCalendarResult ApplyDateLevelCalendar(
        KhmerCalendarResult result,
        BirthInput input)
    {
        var reference = _monthReferenceDataSource!.Get(
            result.AstronomicalYear,
            input.BirthDate.Month);
        var lunarDay = Mod(reference.TithiOffset + input.BirthDate.Day, 30);
        if (lunarDay == 0)
        {
            lunarDay = 30;
        }

        var lunarDayInPhase = lunarDay <= 15 ? lunarDay : lunarDay - 15;
        var lunarPhase = lunarDay <= 15 ? "Waxing" : "Waning";
        var lunarMonthNumber = reference.LunarMonthIndex == 13
            ? 1
            : reference.LunarMonthIndex + 1;
        lunarMonthNumber = Math.Clamp(lunarMonthNumber, 1, 13);
        var monthNames = reference.LunarYearType == 2
            ? LeapLunarMonthNames
            : LunarMonthNames;
        var lunarMonthName = monthNames[lunarMonthNumber - 1];
        var tithiName = GetTithiName(lunarDay);
        var animalIndex = Mod(
            result.KhmerYear - (lunarMonthNumber <= 4 ? 3 : 2),
            AnimalYears.Length);
        var dateBasedKhmerSystemYear = result.BuddhistYear - 1_182;
        var sesaKalaYoga = (int)Mod(dateBasedKhmerSystemYear + 1_120D, 7D);
        var yuga = (int)Mod(result.BuddhistYear - 622D + 12D, 60D);
        if (yuga == 0)
        {
            yuga = 60;
        }

        return result with
        {
            Weekday = input.BirthDate.DayOfWeek.ToString(),
            LunarDay = lunarDay,
            LunarDayInPhase = lunarDayInPhase,
            LunarPhase = lunarPhase,
            LunarDayDisplay = $"{lunarDayInPhase} {lunarPhase}",
            TithiName = tithiName,
            LunarMonthNumber = lunarMonthNumber,
            LunarMonthNameEn = lunarMonthName,
            AnimalYear = AnimalYears[animalIndex],
            SesaKalaYoga = sesaKalaYoga,
            Yuga = yuga,
            TraditionalDate = $"{lunarDayInPhase} {lunarPhase}, {lunarMonthName}, Khmer year {result.KhmerYear}",
        };
    }

    private static string GetTithiName(int lunarDay)
    {
        if (lunarDay == 15)
        {
            return "Full Moon";
        }

        if (lunarDay == 30)
        {
            return "New Moon";
        }

        return ((lunarDay - 1) % 5) switch
        {
            0 => "Nanda Tithi",
            1 => "Bhadra Tithi",
            2 => "Jaya Tithi",
            3 => "Rikta Tithi",
            _ => "Purna Tithi",
        };
    }

    private static double CalculateAhargana(int year, int day, int month, double timeFraction)
    {
        var integerPart =
            365D * year
            + day
            + Math.Floor(275D * month / 9D)
            + 2D * Math.Floor(0.5D + 1D / month)
            + Math.Floor(year / 4D)
            - Math.Floor(year / 100D)
            + Math.Floor(year / 400D)
            + 2D
            - 233_142D
            + 1D;

        return integerPart + timeFraction;
    }

    private static SankrantaValue CalculateSankranta(int year, double correction)
    {
        var khmerYear = year - 638D;
        var julianDay = 1_954_167.5D + (khmerYear * 292_207D + 373D) / 800D;
        var correctedJulianDay = julianDay - correction;
        var referenceJulianDay =
            Math.Floor(365.25D * (year + 4_716D))
            + Math.Floor(30.6001D * 5D)
            + 1D
            + (2D - Math.Floor(year / 100D) + Math.Floor(Math.Floor(year / 100D) / 4D))
            - 1_524.5D;

        var rawDay = (int)(Math.Floor(correctedJulianDay - referenceJulianDay) + 1D);
        var seconds = (int)Mod(
            Math.Round(Mod(correctedJulianDay - referenceJulianDay, 1D) * 86_400D, MidpointRounding.AwayFromZero),
            86_400D);
        var weekdayNumber = (int)Mod(Math.Floor(correctedJulianDay + 0.5D) + 1D, 7D) + 1;

        int month;
        int day;
        if (rawDay <= 0)
        {
            month = 3;
            day = rawDay + 31;
        }
        else if (rawDay <= 30)
        {
            month = 4;
            day = rawDay;
        }
        else
        {
            month = 5;
            day = rawDay - 30;
        }

        return new SankrantaValue(rawDay, day, month, TimeSpan.FromSeconds(seconds), correctedJulianDay, weekdayNumber);
    }

    private static bool IsWholeNumber(double value) => Math.Abs(value - Math.Round(value)) < 1E-12;

    private static int Mod(int value, int divisor) => ((value % divisor) + divisor) % divisor;

    private static double Mod(double value, double divisor) => ((value % divisor) + divisor) % divisor;

    private readonly record struct SankrantaValue(int RawDay, int Day, int Month, TimeSpan Time, double JulianDay, int WeekdayNumber);
}
