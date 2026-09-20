using KhmerAstrology.Calculation.Interfaces;

namespace KhmerAstrology.Calculation.Houses;

public sealed class HouseCalculator : IHouseCalculator
{
    /// <summary>
    /// Calculates a whole-sign house from an ascendant sign and a planet sign.
    /// The inspected workbook dashboard is sign-based and does not yet expose a separate
    /// house-system formula, so this method is intentionally tracked as a provisional rule.
    /// </summary>
    public int CalculateWholeSignHouse(int ascendantSign, int planetSign)
    {
        if (ascendantSign is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(ascendantSign), "Sign numbers must be 1 through 12.");
        }

        if (planetSign is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(planetSign), "Sign numbers must be 1 through 12.");
        }

        return ((planetSign - ascendantSign + 12) % 12) + 1;
    }
}
