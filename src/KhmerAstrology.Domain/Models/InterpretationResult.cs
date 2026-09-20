using KhmerAstrology.Domain.Enums;

namespace KhmerAstrology.Domain.Models;

public sealed record InterpretationResult(
    CelestialBody Body,
    int House,
    string Title,
    string Description,
    string Source);
