namespace KhmerAstrology.Domain.Models;

public sealed record NakshatraPosition(
    double LongitudeArcMinutes,
    int NakshatraNumber,
    NakshatraReference Nakshatra,
    int Pada);
