using System.Text;
using KhmerAstrology.Calculation.Interfaces;
using KhmerAstrology.Domain.Interfaces;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Calculation.Calendar;

/// <summary>
/// Implements the 365/366 day Khmer automatic calendar engine from sheet 30 (ប្រតិទិនស្វ័យប្រវត្តិ).
/// </summary>
public sealed class AutomaticCalendarCalculator : IAutomaticCalendarCalculator
{
    private readonly IKhmerCalendarYearReferenceDataSource _yearReferenceDataSource;

    public AutomaticCalendarCalculator(IKhmerCalendarYearReferenceDataSource yearReferenceDataSource)
    {
        _yearReferenceDataSource = yearReferenceDataSource ?? throw new ArgumentNullException(nameof(yearReferenceDataSource));
    }

    private static readonly string[] Weekdays =
    [
        "អាទិត្យ", // Sunday = 0
        "ចន្ទ",    // Monday = 1
        "អង្គារ",   // Tuesday = 2
        "ពុធ",     // Wednesday = 3
        "ព្រហស្បតិ៍", // Thursday = 4
        "សុក្រ",    // Friday = 5
        "សៅរ៍"     // Saturday = 6
    ];

    public static readonly string[] GregorianMonthKhmerNames =
    [
        "មករា", "កុម្ភៈ", "មីនា", "មេសា", "ឧសភា", "មិថុនា",
        "កក្កដា", "សីហា", "កញ្ញា", "តុលា", "វិច្ឆិកា", "ធ្នូ"
    ];

    // Exact strings from sheet 30 K6 / sheet 31 B16 (CHOOSE list).
    private static readonly string[] TithiNames =
    [
        "នន្ទតិថី", "ភទ្រតិថី", "ជយតិថី", "រិក្តតិថី", "បូណ៌តិថី"
    ];

    private static readonly string[] LunarMonths12 =
    [
        "ខែមិគសិរ", "ខែបុស្ស", "ខែមាឃ", "ខែផល្គុន", "ខែចេត្រ", "ខែពិសាខ",
        "ខែជេស្ឋ", "ខែអាសាឍ", "ខែស្រាពណ៍", "ខែភទ្របទ", "ខែអស្សុជ", "ខែកត្តិក"
    ];

    private static readonly string[] LunarMonths13 =
    [
        "ខែមិគសិរ", "ខែបុស្ស", "ខែមាឃ", "ខែផល្គុន", "ខែចេត្រ", "ខែពិសាខ",
        "ខែជេស្ឋ", "ខែបឋមាសាឍ", "ខែទុតិយាសាឍ", "ខែស្រាពណ៍", "ខែភទ្របទ", "ខែអស្សុជ", "ខែកត្តិក"
    ];

    private static readonly string[] AnimalNames =
    [
        "ជូត", "ឆ្លូវ", "ខាល", "ថោះ", "រោង", "ម្សាញ់",
        "មមី", "មមែ", "វក", "រកា", "ច", "កុរ"
    ];

    public static readonly (string Name, string Meaning)[] Samvatsaras =
    [
        ("ប្រភវៈ", "របស់ថ្មីៗកើតឡើង, ល្អ"), // 1
        ("វិភវៈ", "សម្បូរសប្បាយ, សេដ្ឋកិច្ចល្អ"), // 2
        ("ឝុក្លៈ", "មានសេចក្តីសុខ, បរិសុទ្ធ"), // 3
        ("ប្រមោទៈ", "ប្រជាជនសប្បាយចិត្ត"), // 4
        ("ប្រជាបតិ", "ផលដំណាំល្អ"), // 5
        ("អង្គិរសៈ", "ប្រជាជនមានពន្លឺបញ្ញា, តែអាចមានភ្លើង"), // 6
        ("ឝ្រីមុខៈ", "កិត្តិយស, ភាពស្រស់ស្អាត"), // 7
        ("ភវៈ", "មានភាពរស់រវើក, សុខភាពល្អ"), // 8
        ("យុវៈ", "ភាពខ្លាំងក្លា, សប្បាយរីករាយ"), // 9
        ("ធាត្ឫៈ", "ការកសាង, ផលល្អ"), // 10
        ("ឥឝ្វរៈ", "មានអំណាច, ភាពជាធំ"), // 11
        ("ពហុធាន្យៈ", "ស្រូវអង្ករសម្បូរ, កសិកម្មល្អ"), // 12
        ("ប្រមាថីៈ", "មានទុក្ខព្រួយខ្លះ, ការបៀតបៀន"), // 13
        ("វិក្រមៈ", "ជ័យជម្នះ, ការតស៊ូ"), // 14
        ("វ្ឫឞៈ", "ភាពរឹងមាំ, ផលដំណាំបរិបូណ៌"), // 15
        ("ចិត្ត្រភានុ", "អស្ចារ្យ, តែប្រយ័ត្នរឿងភ្លើង"), // 16
        ("សុភានុ", "ភាពរុងរឿង, កិត្តិសព្ទ"), // 17
        ("តារណៈ", "ឆ្លងផុតគ្រោះ, ការសង្គ្រោះ"), // 18
        ("បាថ៌ិវៈ", "ថ្នាក់ដឹកនាំរុងរឿង, ដីមានតម្លៃល្អ"), // 19
        ("វ្យយៈ", "សេដ្ឋកិច្ចធ្លាក់, ចំណាយច្រើន"), // 20
        ("សវ៌ជិតៈ", "មានជ័យជម្នះក្នុងសង្គ្រាម"), // 21
        ("សវ៌ធារី", "សម្បូរណ៍សប្បាយ, គ្រប់គ្រាន់"), // 22
        ("វិរោធិ", "មានជម្លោះផ្ទៃក្នុង, ការបែកបាក់"), // 23
        ("វិក្ឫតិ", "ធម្មជាតិកាច, មានជំងឺ, មិនល្អ"), // 24
        ("ខរៈ", "កើតភាពសោះកក្រោះ, ក្ដៅហួតហែង"), // 25
        ("នន្ទនៈ", "ការសប្បាយរីករាយ"), // 26
        ("វិជយៈ", "ជោគជ័យក្នុងការងារ, កីឡា, ទ័ព"), // 27
        ("ជយៈ", "ជោគជ័យលើសត្រូវ, ល្អ"), // 28
        ("មន្មថៈ", "កាមទេព មនុស្សលង់ស្នេហា, តណ្ហាខ្លាំង"), // 29
        ("ទុម៌ុខៈ", "កើតរឿងអាស្រូវ, ការនិយាយអាក្រក់ដាក់គ្នា"), // 30
        ("ហេមលម្ពៈ", "សេដ្ឋកិច្ចឡើង, មាសឡើងថ្លៃ"), // 31
        ("វិលម្ពិ", "ការងារស្ទះ, អ្វីៗមិនទាន់ចិត្ត"), // 32
        ("វិការី", "រោគរាតត្បាត, ជំងឺឆ្លងសាហាវ"), // 33
        ("ឝវ៌រី", "ភាពងងឹត, លាក់កំបាំង, កើតទុរ្ភិក្ស"), // 34
        ("ប្លវៈ", "ទឹកជំនន់, ភ្លៀងច្រើន"), // 35
        ("ឝុភក្ឫតៈ", "ឆ្នាំល្អកើតសប្បុរសធម៌, បុណ្យកុសល"), // 36
        ("ឝោភក្ឫតៈ", "សិល្បៈរីកចម្រើន បច្ចេកទេសកើតថ្មី"), // 37
        ("ក្រោធិ", "សង្គ្រាម, ភ្លើង, ជម្លោះខ្លាំង"), // 38
        ("វិឝ្វាវសុ", "លាភមធ្យម, សេដ្ឋកិច្ចងើប"), // 39
        ("បរាភវៈ", "ការបរាជ័យ, បាក់មុខមាត់, ខកបំណង"), // 40
        ("ប្លវង្គៈ", "មិនទៀងទាត់, អ្វីៗផ្លាស់ប្តូរលឿន"), // 41
        ("កីលកៈ", "ការរាំងស្ទះ, បញ្ហាជាប់គាំង"), // 42
        ("សៅម្យៈ", "ភាពត្រជាក់ សេចក្តីសុខ, ធូរស្រាល"), // 43
        ("សាធារណៈ", "ធម្មតា មិនល្អមិនអាក្រក់"), // 44
        ("វិរោធក្ឫតៈ", "ការប្រឆាំងគ្នា, មិនចុះសម្រុងគ្នា"), // 45
        ("បរិធាវី", "ការភ័យខ្លាច, រត់គេចខ្លួន"), // 46
        ("ប្រមាទី", "ការធ្វេសប្រហែស (គ្រោះថ្នាក់ដោយអចេតនា)"), // 47
        ("អានន្ទៈ", "សេចក្តីរីករាយ សុខសាន្ត"), // 48
        ("រក្ឞសៈ", "សាហាវឃោរឃៅ, ចិត្តអាក្រក់"), // 49
        ("អនលៈ", "ក្ដៅខ្លាំង, ភ្លើងឆេះ, រាំងស្ងួត"), // 50
        ("បិង្គលៈ", "កើតជំងឺ, មានការភ័យព្រួយ"), // 51
        ("កាលយុក្តៈ", "ពេលវេលាកំណត់, យុត្តិធម៌"), // 52
        ("សិទ្ធាថ៌ី", "សម្រេចជោគជ័យ, លាភសក្ការៈ"), // 53
        ("រៅទ្រៈ", "ដ៏សាហាវព្យុះសង្ឃរា, ការបំផ្លាញ"), // 54
        ("ទុម៌តិ", "ការបោកប្រាស់, នយោបាយកខ្វក់"), // 55
        ("ទុន្ទុភិ", "សំឡេងលាន់ឮ, កិត្តិនាម កិត្តិយស"), // 56
        ("រុធិរោទ្គារី", "ជំងឺរាតត្បាតធ្ងន់, សង្គ្រាមបង្ហូរឈាម"), // 57
        ("រក្តាក្ឞី", "កំហឹង, ជំងឺភ្នែក, ការសម្លឹងព្យាបាទគ្នា"), // 58
        ("ក្រោធនៈ", "ជម្លោះ, ការមិនពេញចិត្ត"), // 59
        ("ក្ឞយៈ", "ការវិនាសធំ គ្រោះទុរ្ភិក្ស"), // 60
    ];

    public AutomaticCalendarMonthResult CalculateMonth(int ceOrBceYear, int gregorianMonth)
    {
        if (ceOrBceYear == 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ceOrBceYear),
                "Year 0 is not valid. Enter a positive CE year or a negative BCE year.");
        }

        if (gregorianMonth is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(
                nameof(gregorianMonth),
                "Month must be between 1 and 12.");
        }

        var astronomicalYear = ceOrBceYear < 0 ? ceOrBceYear + 1 : ceOrBceYear;
        var ksAuto = ceOrBceYear < 0 ? 3101 + ceOrBceYear : 3100 + ceOrBceYear;
        var isLeap = IsLeapYear(astronomicalYear);
        var ab2 = isLeap ? 1 : 0;
        int[] daysBeforeMonth = [0, 31, 59 + ab2, 90 + ab2, 120 + ab2, 151 + ab2, 181 + ab2, 212 + ab2, 243 + ab2, 273 + ab2, 304 + ab2, 334 + ab2];
        var ad2 = daysBeforeMonth[gregorianMonth - 1];
        var daysInMonth = DaysInMonth(astronomicalYear, gregorianMonth);

        var (R, S, T, U, V) = WalkYear(astronomicalYear);

        var rows = new List<AutomaticCalendarDayRow>(daysInMonth);
        var monthName = GregorianMonthKhmerNames[gregorianMonth - 1];

        for (var d = 1; d <= daysInMonth; d++)
        {
            var idx = ad2 + d - 1;
            var weekdayIdx = CalculateGregorianWeekdayIndex(astronomicalYear, ad2 + d);
            var weekday = Weekdays[weekdayIdx];

            var be = astronomicalYear + 543 + ((ad2 + d) >= 106 + ab2 ? 1 : 0);
            var ms = be - 622;
            var cs = be - 1182;
            var ks = be + 2557;

            var u = U[idx];
            var lunarDay = u <= 15
                ? $"{ToKhmerNumerals(u)} កើត"
                : $"{ToKhmerNumerals(u - 15)} រោច";

            string tithiName;
            if (u == 15)
            {
                tithiName = AutomaticCalendarMonthSummary.FullMoonTithi;
            }
            else if (u is 29 or 30)
            {
                tithiName = AutomaticCalendarMonthSummary.NewMoonTithi;
            }
            else
            {
                var p = u <= 15 ? u : u - 15;
                tithiName = TithiNames[(p - 1) % 5];
            }

            var s = S[idx];
            var t = T[idx];
            var lunarMonth = (s == 2 ? LunarMonths13 : LunarMonths12)[t - 1];

            var animalIdx = Mod(R[idx] - (t <= 4 ? 3 : 2), 12);
            var animal = AnimalNames[animalIdx];

            var sesa = Mod(cs + 1120, 7);
            var yuga = Mod(ms + 12, 60);
            if (yuga == 0)
            {
                yuga = 60;
            }

            var sam = Samvatsaras[yuga - 1];

            rows.Add(new AutomaticCalendarDayRow(
                DayNumber: d,
                Weekday: weekday,
                Day: d,
                MonthName: monthName,
                CeYear: ceOrBceYear,
                BuddhistYear: be,
                MahaSakaraj: ms,
                ChulaSakaraj: cs,
                KromSakaraj: ks,
                LunarDay: lunarDay,
                TithiName: tithiName,
                LunarMonth: lunarMonth,
                AnimalYear: animal,
                SesaKalaYoga: sesa,
                Yuga: yuga,
                SamvatsaraName: sam.Name,
                Meaning: sam.Meaning));
        }

        return new AutomaticCalendarMonthResult(
            AstronomicalYear: astronomicalYear,
            CeOrBceYear: ceOrBceYear,
            MonthIndex: gregorianMonth,
            MonthName: monthName,
            KromSakarajAuto: ksAuto,
            Days: rows);
    }

    public KhmerLunarState GetLunarState(int ceOrBceYear, int gregorianMonth, int day)
    {
        if (ceOrBceYear == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ceOrBceYear), "Year 0 is not valid.");
        }

        if (gregorianMonth is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(gregorianMonth), "Month must be between 1 and 12.");
        }

        var astronomicalYear = ceOrBceYear < 0 ? ceOrBceYear + 1 : ceOrBceYear;
        if (day < 1 || day > DaysInMonth(astronomicalYear, gregorianMonth))
        {
            throw new ArgumentOutOfRangeException(nameof(day));
        }

        var dayIndex = day - 1;
        for (var month = 1; month < gregorianMonth; month++)
        {
            dayIndex += DaysInMonth(astronomicalYear, month);
        }

        var (r, s, t, u, v) = WalkYear(astronomicalYear);
        return new KhmerLunarState(r[dayIndex], s[dayIndex], t[dayIndex], u[dayIndex], v[dayIndex]);
    }

    /// <summary>
    /// Day-by-day lunar walk for one astronomical year (sheet 30 columns R:X, rows 6..371).
    /// </summary>
    private (int[] R, int[] S, int[] T, int[] U, int[] V) WalkYear(int astronomicalYear)
    {
        var ab2 = IsLeapYear(astronomicalYear) ? 1 : 0;
        var yearRef = _yearReferenceDataSource.Get(astronomicalYear);
        var bq2 = yearRef.KhmerYear;
        var br2 = yearRef.LunarYearType;
        var bs2 = yearRef.IntercalaryFlag1;
        var bt2 = yearRef.IntercalaryFlag2;
        var bu2 = yearRef.StartMonthIndex;
        var bv2 = yearRef.StartTithiOffset;

        var totalDaysInYear = 365 + ab2;
        var R = new int[totalDaysInYear];
        var S = new int[totalDaysInYear];
        var T = new int[totalDaysInYear];
        var U = new int[totalDaysInYear];
        var V = new int[totalDaysInYear];
        var X = new int[totalDaysInYear];

        // Day 1 (Jan 1, index 0)
        R[0] = bu2 == 13 ? bq2 + 1 : bq2;
        S[0] = bu2 == 13 ? bs2 : br2;
        T[0] = bu2 == 13 ? 1 : bu2 + 1;
        U[0] = bu2 == 13 ? bv2 : bv2 + 1;
        V[0] = GetLunarMonthLength(S[0], T[0]);
        X[0] = bu2 == 13 ? 1 : 0;

        // Day 2..totalDaysInYear
        for (var i = 1; i < totalDaysInYear; i++)
        {
            var yearEnd = (U[i - 1] == V[i - 1]) && (T[i - 1] == (S[i - 1] == 2 ? 13 : 12));
            R[i] = R[i - 1] + (yearEnd ? 1 : 0);
            X[i] = X[i - 1] + (yearEnd ? 1 : 0);
            S[i] = R[i] == R[i - 1] ? S[i - 1] : (X[i] == 1 ? bs2 : bt2);

            if (U[i - 1] < V[i - 1])
            {
                T[i] = T[i - 1];
                U[i] = U[i - 1] + 1;
            }
            else
            {
                T[i] = T[i - 1] < (S[i - 1] == 2 ? 13 : 12) ? T[i - 1] + 1 : 1;
                U[i] = 1;
            }

            V[i] = GetLunarMonthLength(S[i], T[i]);
        }

        return (R, S, T, U, V);
    }

    public KhmerDateSearchResult SearchDate(int ceOrBceYear, int gregorianMonth, int day)
    {
        var monthResult = CalculateMonth(ceOrBceYear, gregorianMonth);
        if (day < 1 || day > monthResult.Days.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(day), $"Day must be between 1 and {monthResult.Days.Count} for month {gregorianMonth}.");
        }

        var dayRow = monthResult.Days[day - 1];
        var yearDisplay = ceOrBceYear < 0
            ? $"{-ceOrBceYear} មុន គ.ស."
            : $"{ceOrBceYear} គ.ស.";
        var solarFullDate = $"{day} {dayRow.MonthName} {yearDisplay}";

        return new KhmerDateSearchResult(
            Day: day,
            Month: gregorianMonth,
            MonthName: dayRow.MonthName,
            CeOrBceYear: ceOrBceYear,
            AstronomicalYear: monthResult.AstronomicalYear,
            KromSakarajAuto: monthResult.KromSakarajAuto,
            SolarFullDate: solarFullDate,
            SolarWeekday: dayRow.Weekday,
            SolarDay: day,
            SolarMonth: dayRow.MonthName,
            SolarYear: yearDisplay,
            LunarWeekday: dayRow.Weekday,
            LunarDay: dayRow.LunarDay,
            TithiName: dayRow.TithiName,
            LunarMonth: dayRow.LunarMonth,
            AnimalYear: dayRow.AnimalYear,
            BuddhistYear: dayRow.BuddhistYear,
            MahaSakaraj: dayRow.MahaSakaraj,
            ChulaSakaraj: dayRow.ChulaSakaraj,
            KromSakaraj: dayRow.KromSakaraj,
            SesaKalaYoga: dayRow.SesaKalaYoga);
    }

    public static string ToKhmerNumerals(int number)
    {
        var s = number.ToString();
        var sb = new StringBuilder(s.Length);
        foreach (var ch in s)
        {
            if (ch is >= '0' and <= '9')
            {
                sb.Append((char)('០' + (ch - '0')));
            }
            else
            {
                sb.Append(ch);
            }
        }
        return sb.ToString();
    }

    private static int GetLunarMonthLength(int s, int t)
    {
        if (s == 0)
        {
            return (t % 2 == 1) ? 29 : 30;
        }

        if (s == 1)
        {
            return (t == 7) ? 30 : ((t % 2 == 1) ? 29 : 30);
        }

        // s == 2: 13-month leap year
        int[] lens = [29, 30, 29, 30, 29, 30, 29, 30, 30, 29, 30, 29, 30];
        return lens[t - 1];
    }

    private static bool IsLeapYear(int year) =>
        year % 400 == 0 || (year % 4 == 0 && year % 100 != 0);

    private static int DaysInMonth(int year, int month) => month switch
    {
        2 => IsLeapYear(year) ? 29 : 28,
        4 or 6 or 9 or 11 => 30,
        _ => 31,
    };

    private static int CalculateGregorianWeekdayIndex(int astronomicalYear, int dayOfYear)
    {
        // Excel sheet 30 formula:
        // MOD(MOD((365*(AE2-1800)+INT((AE2-1)/4)-INT((AE2-1)/100)+INT((AE2-1)/400)-436+AD2+A6-1)+3,7)+7,7)
        long y = astronomicalYear;
        long term1 = 365L * (y - 1800L);
        long term2 = (long)Math.Floor((y - 1.0) / 4.0);
        long term3 = (long)Math.Floor((y - 1.0) / 100.0);
        long term4 = (long)Math.Floor((y - 1.0) / 400.0);
        long days = term1 + term2 - term3 + term4 - 436L + dayOfYear - 1L;
        return (int)(((days + 3L) % 7L + 7L) % 7L);
    }

    private static int Mod(int value, int divisor) =>
        ((value % divisor) + divisor) % divisor;
}
