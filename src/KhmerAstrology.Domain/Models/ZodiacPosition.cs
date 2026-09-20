namespace KhmerAstrology.Domain.Models;

public sealed record ZodiacPosition(
    double LongitudeArcMinutes,
    int SignNumber,
    ZodiacSignReference Sign,
    int Degree,
    int Minute,
    double Second);
