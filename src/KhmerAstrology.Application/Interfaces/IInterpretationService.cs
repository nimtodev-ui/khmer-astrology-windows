using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Application.Interfaces;

public interface IInterpretationService
{
    IReadOnlyList<InterpretationResult> Interpret(IReadOnlyList<PlanetPosition> positions);
}
