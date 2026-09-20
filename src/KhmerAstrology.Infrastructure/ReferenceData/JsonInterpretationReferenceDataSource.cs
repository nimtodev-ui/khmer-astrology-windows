using KhmerAstrology.Domain.Interfaces;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Infrastructure.ReferenceData;

public sealed class JsonInterpretationReferenceDataSource : IInterpretationReferenceDataSource
{
    private readonly Lazy<IReadOnlyList<InterpretationRule>> _rules = new(LoadRules);

    public IReadOnlyList<InterpretationRule> GetAll() => _rules.Value;

    private static IReadOnlyList<InterpretationRule> LoadRules()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "interpretations.json");
        var file = JsonFile.Read<InterpretationFile>(path, "Interpretation");
        if (!string.Equals(file.Status, "workbook-backed", StringComparison.OrdinalIgnoreCase)
            || file.Rules is null
            || file.Rules.Count != 12)
        {
            throw new InvalidDataException("Workbook interpretation reference data must contain 12 house rules.");
        }

        var rules = file.Rules
            .Select(rule => new InterpretationRule(rule.HouseNumber, rule.NameKm, rule.DescriptionKm, rule.Source))
            .ToArray();
        if (rules.Any(rule => rule.HouseNumber is < 1 or > 12)
            || rules.Select(rule => rule.HouseNumber).Distinct().Count() != 12)
        {
            throw new InvalidDataException("Workbook interpretation rules must cover houses 1 through 12.");
        }
        return rules;
    }

    private sealed class InterpretationFile
    {
        public string Status { get; init; } = string.Empty;
        public List<InterpretationJsonRule>? Rules { get; init; }
    }

    private sealed class InterpretationJsonRule
    {
        public int HouseNumber { get; init; }
        public string NameKm { get; init; } = string.Empty;
        public string DescriptionKm { get; init; } = string.Empty;
        public string Source { get; init; } = string.Empty;
    }
}
