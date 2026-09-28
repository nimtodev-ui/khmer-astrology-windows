namespace KhmerAstrology.Domain.Models;

/// <summary>
/// Standard Khmer lunar-calendar conventions that the workbook does not tabulate.
/// They are applied on top of workbook values: the Sak uses the workbook's
/// ច.ស. (sheet 30 H6) and holy days use the sheet 30 day walk (lunar day U and
/// month length V).
/// </summary>
public static class KhmerLunarCalendarRules
{
    // Indexed by the last digit of the Chula Sakaraj (ច.ស.): 1 = ឯកស័ក … 0 = សំរឹទ្ធិស័ក.
    private static readonly string[] SakNamesKm =
    [
        "សំរឹទ្ធិស័ក", "ឯកស័ក", "ទោស័ក", "ត្រីស័ក", "ចត្វាស័ក",
        "បញ្ចស័ក", "ឆស័ក", "សប្តស័ក", "អដ្ឋស័ក", "នព្វស័ក",
    ];

    private static readonly string[] SakNamesEn =
    [
        "Samrithi Sak", "Ek Sak", "To Sak", "Trei Sak", "Chattva Sak",
        "Pancha Sak", "Chha Sak", "Sapta Sak", "Attha Sak", "Nopa Sak",
    ];

    /// <summary>Sak (ស័ក) of the year from the last digit of the Chula Sakaraj.</summary>
    public static string GetSakName(int chulaSakaraj, bool khmer)
    {
        var digit = ((chulaSakaraj % 10) + 10) % 10;
        return khmer ? SakNamesKm[digit] : SakNamesEn[digit];
    }

    /// <summary>
    /// Buddhist holy day (ថ្ងៃសីល): 8 and 15 កើត, 8 រោច, and the last day of the
    /// lunar month (15 រោច in a 30-day month, 14 រោច in a 29-day month).
    /// </summary>
    /// <param name="lunarDay">Day of the lunar month, 1..30 (sheet 30 column U).</param>
    /// <param name="monthLength">Length of that lunar month, 29 or 30 (sheet 30 column V).</param>
    public static bool IsHolyDay(int lunarDay, int monthLength) =>
        lunarDay is 8 or 15 or 23 || (monthLength is 29 or 30 && lunarDay == monthLength);

    /// <summary>Moon phase marked on a holy day of the standard calendar; <see cref="KhmerMoonPhase.None"/> otherwise.</summary>
    public static KhmerMoonPhase GetMoonPhase(int lunarDay, int monthLength) => lunarDay switch
    {
        8 => KhmerMoonPhase.FirstQuarter,
        15 => KhmerMoonPhase.Full,
        23 => KhmerMoonPhase.LastQuarter,
        _ when monthLength is 29 or 30 && lunarDay == monthLength => KhmerMoonPhase.New,
        _ => KhmerMoonPhase.None,
    };

    /// <summary>Lunar day as printed on the calendar: "៨កើត" / "Wax 8", "៣រោច" / "Wane 3".</summary>
    public static string FormatLunarDay(int lunarDay, bool khmer)
    {
        var waxing = lunarDay <= 15;
        var day = waxing ? lunarDay : lunarDay - 15;
        if (!khmer)
        {
            return $"{(waxing ? "Wax" : "Wane")} {day}";
        }

        var digits = string.Concat(day.ToString(System.Globalization.CultureInfo.InvariantCulture)
            .Select(ch => (char)('០' + (ch - '0'))));
        return digits + (waxing ? "កើត" : "រោច");
    }

    private static readonly Dictionary<string, string> LunarMonthsEn = new(StringComparer.Ordinal)
    {
        ["ខែមិគសិរ"] = "Migasir",
        ["ខែបុស្ស"] = "Bos",
        ["ខែមាឃ"] = "Meak",
        ["ខែផល្គុន"] = "Phalkun",
        ["ខែចេត្រ"] = "Chet",
        ["ខែពិសាខ"] = "Pisak",
        ["ខែជេស្ឋ"] = "Chesth",
        ["ខែអាសាឍ"] = "Asath",
        ["ខែបឋមាសាឍ"] = "Pathama Asath",
        ["ខែទុតិយាសាឍ"] = "Tutiya Asath",
        ["ខែស្រាពណ៍"] = "Srap",
        ["ខែភទ្របទ"] = "Photrobot",
        ["ខែអស្សុជ"] = "Assoch",
        ["ខែកត្តិក"] = "Kadeuk",
    };

    /// <summary>
    /// Buddhist observances fixed by the lunar date. In a 13-month year the Asalha
    /// observances fall in ខែទុតិយាសាឍ (the second Asath).
    /// </summary>
    public static KhmerLunarObservance? GetObservance(string khmerLunarMonth, int lunarDay, int monthLength) =>
        (khmerLunarMonth, lunarDay) switch
        {
            ("ខែមាឃ", 15) => new("មាឃបូជា", "Meak Bochea"),
            ("ខែពិសាខ", 15) => new("វិសាខបូជា", "Visak Bochea"),
            ("ខែពិសាខ", 19) => new("ព្រះរាជពិធីច្រត់ព្រះនង្គ័ល", "Royal Ploughing Ceremony"),
            ("ខែអាសាឍ" or "ខែទុតិយាសាឍ", 15) => new("អាសាឡ្ហបូជា", "Asalha Bochea"),
            ("ខែអាសាឍ" or "ខែទុតិយាសាឍ", 16) => new("ចូលវស្សា", "Vassa begins"),
            ("ខែភទ្របទ", 16) => new("កាន់បិណ្ឌទី១", "Kan Ben begins"),
            ("ខែភទ្របទ", _) when monthLength is 29 or 30 && lunarDay == monthLength => new("ភ្ជុំបិណ្ឌ", "Pchum Ben"),
            ("ខែអស្សុជ", 15) => new("ចេញវស្សា", "End of Vassa"),
            ("ខែកត្តិក", 14 or 15 or 16) => new("បុណ្យអុំទូក", "Water Festival"),
            _ => null,
        };

    private static readonly Dictionary<string, string> AnimalYearsEn = new(StringComparer.Ordinal)
    {
        ["ជូត"] = "Rat", ["ឆ្លូវ"] = "Ox", ["ខាល"] = "Tiger", ["ថោះ"] = "Rabbit",
        ["រោង"] = "Dragon", ["ម្សាញ់"] = "Snake", ["មមី"] = "Horse", ["មមែ"] = "Goat",
        ["វក"] = "Monkey", ["រកា"] = "Rooster", ["ច"] = "Dog", ["កុរ"] = "Pig",
    };

    /// <summary>Animal year from the sheet 30 string (column M), in Khmer or English.</summary>
    public static string GetAnimalYearName(string khmerAnimalYear, bool khmer) =>
        khmer || !AnimalYearsEn.TryGetValue(khmerAnimalYear, out var english) ? khmerAnimalYear : english;

    /// <summary>Lunar month name from the sheet 30 month string (column L), in Khmer or romanised.</summary>
    public static string GetLunarMonthName(string khmerLunarMonth, bool khmer) =>
        khmer || !LunarMonthsEn.TryGetValue(khmerLunarMonth, out var english) ? khmerLunarMonth : english;
}

public sealed record KhmerLunarObservance(string Khmer, string English)
{
    public string Name(bool khmer) => khmer ? Khmer : English;
}

public enum KhmerMoonPhase
{
    None,
    FirstQuarter,
    Full,
    LastQuarter,
    New,
}
