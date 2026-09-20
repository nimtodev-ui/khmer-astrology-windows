namespace KhmerAstrology.Domain.Models;

public sealed class BirthInput
{
    public string Name { get; init; } = string.Empty;

    public string Gender { get; init; } = string.Empty;

    public DateOnly BirthDate { get; init; }

    public TimeOnly BirthTime { get; init; }

    public string Country { get; init; } = "Cambodia";

    public string? Province { get; init; }

    public double Latitude { get; init; }

    public double Longitude { get; init; }

    public string TimeZoneId { get; init; } = "Asia/Phnom_Penh";

    /// <summary>
    /// Optional workbook B54 UTC override. When supplied, it takes precedence
    /// over the timezone database offset for astronomical calculations.
    /// </summary>
    public double? UtcOffsetOverrideHours { get; init; }
}
