using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Domain.ReferenceData;

/// <summary>
/// Static reference data transcribed from the workbook's lookup sheets.
/// This data is deliberately kept in the domain so calculation code has no Excel dependency.
/// </summary>
public static class WorkbookReferenceData
{
    public static IReadOnlyList<ZodiacSignReference> ZodiacSigns { get; } =
    [
        new(1, "Aries", "មេសៈ", "♈"),
        new(2, "Taurus", "ឧសភៈ", "♉"),
        new(3, "Gemini", "មិថុនៈ", "♊"),
        new(4, "Cancer", "កក្កដៈ", "♋"),
        new(5, "Leo", "សីហៈ", "♌"),
        new(6, "Virgo", "កញ្ញា", "♍"),
        new(7, "Libra", "តុលា", "♎"),
        new(8, "Scorpio", "វិច្ឆិកៈ", "♏"),
        new(9, "Sagittarius", "ធនុ", "♐"),
        new(10, "Capricorn", "មករៈ", "♑"),
        new(11, "Aquarius", "កុម្ភៈ", "♒"),
        new(12, "Pisces", "មីនៈ", "♓")
    ];

    public static IReadOnlyList<NakshatraReference> Nakshatras { get; } =
    [
        new(1, "Ashvini", "អស្សុជ", 0),
        new(2, "Bharani", "ភរណី", 800),
        new(3, "Krittika", "កត្តិក", 1600),
        new(4, "Rohini", "រោហិណី", 2400),
        new(5, "Mrigashira", "មិគសិរ", 3200),
        new(6, "Ardra", "អទ្ទា", 4000),
        new(7, "Punarvasu", "បុនព្វសុ", 4800),
        new(8, "Pushya", "បុស្ស", 5600),
        new(9, "Ashlesha", "អាសឡេស", 6400),
        new(10, "Magha", "មាឃ", 7200),
        new(11, "Purva Phalguni", "បុព្វផល្គុនី", 8000),
        new(12, "Uttara Phalguni", "ឧត្តរផល្គុនី", 8800),
        new(13, "Hasta", "ហត្ថ", 9600),
        new(14, "Chitra", "ចិត្ត", 10400),
        new(15, "Swati", "សាតិ", 11200),
        new(16, "Vishakha", "វិសាខ", 12000),
        new(17, "Anuradha", "អនុរាធ", 12800),
        new(18, "Jyeshtha", "ជេដ្ឋ", 13600),
        new(19, "Mula", "មូល", 14400),
        new(20, "Purva Ashadha", "បុព្វាសាឡ្ហ", 15200),
        new(21, "Uttara Ashadha", "ឧត្តរាសាឡ្ហ", 16000),
        new(22, "Shravana", "សវន", 16800),
        new(23, "Dhanishtha", "ធនិដ្ឋ", 17600),
        new(24, "Shatabhisha", "សតភិសជ", 18400),
        new(25, "Purva Bhadrapada", "បុព្វភទ្ទបទ", 19200),
        new(26, "Uttara Bhadrapada", "ឧត្តរភទ្ទបទ", 20000),
        new(27, "Revati", "រេវតី", 20800)
    ];
}
