using KhmerAstrology.Application.Interfaces;
using KhmerAstrology.Domain.Enums;
using KhmerAstrology.Domain.Interfaces;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Application.Services;

public sealed class InterpretationService : IInterpretationService
{
    private readonly IReadOnlyDictionary<int, InterpretationRule> _rules;
    private readonly IReadOnlyDictionary<CelestialBody, CelestialBodyReference> _bodies;

    public InterpretationService(
        IInterpretationReferenceDataSource referenceDataSource,
        ICelestialBodyReferenceDataSource celestialBodyReferenceDataSource)
    {
        _rules = referenceDataSource.GetAll().ToDictionary(rule => rule.HouseNumber);
        _bodies = celestialBodyReferenceDataSource.GetAll()
            .SelectMany(reference => reference.Bodies.Select(body => (body, reference)))
            .ToDictionary(item => item.body, item => item.reference);
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
                    rule.Source,
                    rule.NameKm,
                    _bodies.TryGetValue(position.Body, out var body) ? body.MeaningKm : string.Empty);
            })
            .ToArray();
    }
}
