namespace KhmerAstrology.Domain.Models;

public sealed record NakshatraReference(
    int Number,
    string NameEn,
    string NameKm,
    double StartArcMinutes);
