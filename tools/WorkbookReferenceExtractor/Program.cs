using System.IO.Compression;
using System.Text.Json;
using System.Xml.Linq;

if (args.Length is < 2 or > 3)
{
    Console.Error.WriteLine("Usage: WorkbookReferenceExtractor <workbook.xlsx> <output.json.gz> [calendar-output.json.gz]");
    return 2;
}

var workbookPath = Path.GetFullPath(args[0]);
var outputPath = Path.GetFullPath(args[1]);
var calendarOutputPath = args.Length == 3 ? Path.GetFullPath(args[2]) : null;
using var workbookStream = File.Open(workbookPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
using var archive = new ZipArchive(workbookStream, ZipArchiveMode.Read);

var sharedStrings = ReadSharedStrings(archive);
var workbook = ReadXml(archive, "xl/workbook.xml");
var workbookRelationships = ReadXml(archive, "xl/_rels/workbook.xml.rels");
var sheetPart = ResolveSheetPart(workbook, workbookRelationships, "សូរ្យយាត្រចាស់");
var calculationPart = ResolveSheetPart(workbook, workbookRelationships, "ក្បួនគណនា");
var calendarPart = calendarOutputPath is null
    ? null
    : ResolveSheetPart(workbook, workbookRelationships, "ប្រតិទិនស្វ័យប្រវត្តិ");
var legacyCells = ReadCells(ReadXml(archive, sheetPart), sharedStrings);
var calculationCells = ReadCells(ReadXml(archive, calculationPart), sharedStrings);

var bodyRows = new[]
{
    new BodyReference("Sun", 151),
    new BodyReference("Moon", 152),
    new BodyReference("Mars", 153),
    new BodyReference("Mercury", 154),
    new BodyReference("Jupiter", 155),
    new BodyReference("Venus", 156),
    new BodyReference("Saturn", 157),
    new BodyReference("Rahu", 158),
    new BodyReference("Uranus", 159),
    new BodyReference("Neptune", 160),
    new BodyReference("Pluto", 161),
};

var bodies = bodyRows.Select(body => new PlanetReference(
    body.Name,
    body.Row,
    ParseDouble(calculationCells[$"AB{body.Row}"]),
    ParseDouble(calculationCells[$"AD{body.Row}"]))).ToArray();

var eras = new[]
{
    new EraReference(-199, "EB", 1_486_208.5D),
    new EraReference(200, "EC", 1_648_375.5D),
    new EraReference(600, "ED", 1_794_107.5D),
    new EraReference(1000, "EE", 1_940_198.5D),
    new EraReference(1400, "EF", 2_086_295.5D),
    new EraReference(1800, "EG", 2_232_392.5D),
    new EraReference(2201, "EA", 2_378_489.5D),
    new EraReference(2601, "EH", 2_524_958.5D),
    new EraReference(3001, "EI", 2_671_055.5D),
    new EraReference(3401, "EJ", 2_817_152.5D),
    new EraReference(3801, "EK", 2_963_249.5D),
    new EraReference(4201, "EL", 3_109_346.5D),
    new EraReference(4459, "EM", 3_255_443.5D),
};

var tables = new Dictionary<string, string[]>(StringComparer.Ordinal);
foreach (var column in eras.Select(era => era.TableColumn).Distinct(StringComparer.Ordinal))
{
    tables[column] = Enumerable.Range(1, 500)
        .Select(row => legacyCells.GetValueOrDefault($"{column}{row}") ?? string.Empty)
        .ToArray();
}

var data = new ModernPlanetaryReferenceData(bodies, eras, tables);
Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
await using (var output = File.Create(outputPath))
await using (var gzip = new GZipStream(output, CompressionLevel.SmallestSize))
{
    await JsonSerializer.SerializeAsync(gzip, data, new JsonSerializerOptions { WriteIndented = false });
}

Console.WriteLine($"Wrote {outputPath} ({new FileInfo(outputPath).Length:N0} bytes)");

if (calendarOutputPath is not null && calendarPart is not null)
{
    var calendarCells = ReadCells(ReadXml(archive, calendarPart), sharedStrings);
    var calendarReferences = ExtractCalendarMonthReferences(calendarCells);
    Directory.CreateDirectory(Path.GetDirectoryName(calendarOutputPath)!);
    await using var calendarOutput = File.Create(calendarOutputPath);
    await using var calendarGzip = new GZipStream(calendarOutput, CompressionLevel.SmallestSize);
    await JsonSerializer.SerializeAsync(
        calendarGzip,
        calendarReferences,
        new JsonSerializerOptions { WriteIndented = false });
    Console.WriteLine($"Wrote {calendarOutputPath} ({new FileInfo(calendarOutputPath).Length:N0} bytes, {calendarReferences.Count:N0} month references)");
}

return 0;

static List<CalendarMonthReference> ExtractCalendarMonthReferences(
    IReadOnlyDictionary<string, string> cells)
{
    var references = new List<CalendarMonthReference>(5_103 * 12);
    for (var year = -643; year <= 4_459; year++)
    {
        var (column, index) = SelectCalendarSource(year);
        var encoded = cells.GetValueOrDefault($"{column}2") ?? string.Empty;
        var offset = index * 120;
        if (encoded.Length < offset + 120)
        {
            throw new InvalidDataException($"Automatic calendar source {column}2 is too short for year {year}.");
        }

        for (var month = 1; month <= 12; month++)
        {
            var code = encoded.Substring(offset + (month - 1) * 10, 10);
            if (!int.TryParse(code.AsSpan(0, 4), out var encodedKhmerYear)
                || !int.TryParse(code.AsSpan(4, 1), out var lunarYearType)
                || !int.TryParse(code.AsSpan(5, 1), out var intercalaryFlag)
                || !int.TryParse(code.AsSpan(6, 2), out var lunarMonthIndex)
                || !int.TryParse(code.AsSpan(8, 2), out var tithiOffset))
            {
                throw new InvalidDataException($"Automatic calendar code '{code}' is invalid for {year}-{month:00}.");
            }

            references.Add(new CalendarMonthReference(
                year,
                month,
                encodedKhmerYear - 2_000,
                lunarYearType,
                intercalaryFlag,
                lunarMonthIndex,
                tithiOffset));
        }
    }

    return references;
}

static (string Column, int Index) SelectCalendarSource(int astronomicalYear)
{
    if (astronomicalYear <= 2_200)
    {
        var offset = astronomicalYear + 643;
        return ($"{ColumnName(55 + offset / 260)}", offset % 260); // BC:BM
    }

    if (astronomicalYear <= 3_000)
    {
        var offset = astronomicalYear - 2_201;
        return ($"{ColumnName(78 + offset / 260)}", offset % 260); // BZ:CC
    }

    var finalOffset = astronomicalYear - 3_001;
    return ($"{ColumnName(83 + finalOffset / 260)}", finalOffset % 260); // CE:CJ
}

static string ColumnName(int number)
{
    var result = string.Empty;
    while (number > 0)
    {
        number--;
        result = (char)('A' + number % 26) + result;
        number /= 26;
    }

    return result;
}

static XDocument ReadXml(ZipArchive archive, string path)
{
    var entry = archive.GetEntry(path) ?? throw new InvalidOperationException($"Missing workbook part: {path}");
    using var stream = entry.Open();
    return XDocument.Load(stream, LoadOptions.PreserveWhitespace);
}

static string[] ReadSharedStrings(ZipArchive archive)
{
    var document = ReadXml(archive, "xl/sharedStrings.xml");
    XNamespace main = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    return document.Descendants(main + "si")
        .Select(item => string.Concat(item.Descendants(main + "t").Select(text => text.Value)))
        .ToArray();
}

static string ResolveSheetPart(XDocument workbook, XDocument relationships, string sheetName)
{
    XNamespace main = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    XNamespace relationship = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    XNamespace packageRelationship = "http://schemas.openxmlformats.org/package/2006/relationships";
    var sheet = workbook.Descendants(main + "sheet").Single(item => (string?)item.Attribute("name") == sheetName);
    var relationshipId = (string?)sheet.Attribute(relationship + "id")
        ?? throw new InvalidOperationException($"Sheet relationship missing: {sheetName}");
    var target = relationships.Descendants(packageRelationship + "Relationship")
        .Single(item => (string?)item.Attribute("Id") == relationshipId)
        .Attribute("Target")?.Value
        ?? throw new InvalidOperationException($"Sheet target missing: {sheetName}");
    return target.StartsWith("xl/", StringComparison.Ordinal) ? target : $"xl/{target.TrimStart('/') }";
}

static Dictionary<string, string> ReadCells(XDocument worksheet, IReadOnlyList<string> sharedStrings)
{
    XNamespace main = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    var result = new Dictionary<string, string>(StringComparer.Ordinal);
    foreach (var cell in worksheet.Descendants(main + "c"))
    {
        var address = cell.Attribute("r")?.Value;
        if (address is null)
        {
            continue;
        }

        var value = cell.Element(main + "v")?.Value ?? string.Empty;
        if (string.Equals((string?)cell.Attribute("t"), "s", StringComparison.Ordinal)
            && int.TryParse(value, out var sharedIndex)
            && sharedIndex >= 0
            && sharedIndex < sharedStrings.Count)
        {
            value = sharedStrings[sharedIndex];
        }

        result[address] = value;
    }

    return result;
}

static double ParseDouble(string value) =>
    double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);

file sealed record BodyReference(string Name, int Row);
file sealed record PlanetReference(string Body, int Row, double PeriodDays, double PhaseOffset);
file sealed record EraReference(int StartYear, string TableColumn, double StartJulianDay);
file sealed record ModernPlanetaryReferenceData(
    IReadOnlyList<PlanetReference> Bodies,
    IReadOnlyList<EraReference> Eras,
    IReadOnlyDictionary<string, string[]> Tables);
file sealed record CalendarMonthReference(
    int AstronomicalYear,
    int GregorianMonth,
    int KhmerYear,
    int LunarYearType,
    int IntercalaryFlag,
    int LunarMonthIndex,
    int TithiOffset);
