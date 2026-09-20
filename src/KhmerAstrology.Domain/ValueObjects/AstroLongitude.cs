using KhmerAstrology.Domain.Constants;

namespace KhmerAstrology.Domain.ValueObjects;

public readonly record struct AstroLongitude(double ArcMinutes)
{
    public AstroLongitude Normalize()
    {
        var normalized = ((ArcMinutes % AstrologyConstants.FullCircleArcMinutes)
                          + AstrologyConstants.FullCircleArcMinutes)
                         % AstrologyConstants.FullCircleArcMinutes;

        return new AstroLongitude(normalized);
    }

    public double Degrees => Normalize().ArcMinutes / 60D;

    public override string ToString() => $"{Normalize().Degrees:0.########}°";
}
