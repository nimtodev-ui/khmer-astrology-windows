using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Application.DTOs;

public sealed record FoundationCalculationResult(
    ZodiacPosition Zodiac,
    NakshatraPosition Nakshatra,
    int D1Sign,
    int D3Sign,
    int D9Sign);
