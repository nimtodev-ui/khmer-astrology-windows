namespace KhmerAstrology.Domain.Models;

public sealed record InterpretationRule(
    int HouseNumber,
    string NameKm,
    string DescriptionKm,
    string Source);
