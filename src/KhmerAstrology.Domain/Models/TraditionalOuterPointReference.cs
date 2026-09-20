namespace KhmerAstrology.Domain.Models;

public sealed class TraditionalOuterPointReference
{
    public string Body { get; init; } = string.Empty;

    public int MeanOffset { get; init; }

    public int MeanPeriod { get; init; }

    public bool RoundMeanLongitude { get; init; }

    public int ExaltationArcMinutes { get; init; }

    public IReadOnlyList<int> FirstTableBase { get; init; } = [];

    public IReadOnlyList<int> FirstTableDifference { get; init; } = [];

    public IReadOnlyList<int> SecondTableBase { get; init; } = [];

    public IReadOnlyList<int> SecondTableDifference { get; init; } = [];

    public bool UsesSignBasedBhujja { get; init; }
}
