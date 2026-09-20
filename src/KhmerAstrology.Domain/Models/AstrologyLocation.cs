namespace KhmerAstrology.Domain.Models;

public sealed class AstrologyLocation
{
    public int Id { get; init; }

    public string NameEn { get; init; } = string.Empty;

    public string NameKm { get; init; } = string.Empty;

    public string Province { get; init; } = string.Empty;

    public double Latitude { get; init; }

    public double Longitude { get; init; }

    public string TimeZoneId { get; init; } = "Asia/Phnom_Penh";

    public override string ToString() => $"{NameEn} — {NameKm}";
}
