using KhmerAstrology.Application.Interfaces;
using KhmerAstrology.Domain.Interfaces;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Application.Services;

public sealed class InterpretationService : IInterpretationService
{
    private readonly IReadOnlyDictionary<int, InterpretationRule> _rules;

    public InterpretationService(IInterpretationReferenceDataSource referenceDataSource)
    {
        _rules = referenceDataSource.GetAll().ToDictionary(rule => rule.HouseNumber);
    }

    public IReadOnlyList<InterpretationResult> Interpret(IReadOnlyList<PlanetPosition> positions)
    {
        ArgumentNullException.ThrowIfNull(positions);
        return positions
            .Where(position => _rules.ContainsKey(position.House))
            .Select(position =>
            {
                var rule = _rules[position.House];
                return new InterpretationResult(
                    position.Body,
                    position.House,
                    $"{position.Body} — House {position.House} / {rule.NameKm}",
                    rule.DescriptionKm,
                    rule.Source);
            })
            .ToArray();
    }
}
