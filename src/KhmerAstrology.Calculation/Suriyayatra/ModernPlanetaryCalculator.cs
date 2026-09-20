using System.Globalization;
using KhmerAstrology.Calculation.Interfaces;
using KhmerAstrology.Calculation.Suriyayatra.Models;
using KhmerAstrology.Domain.Enums;
using KhmerAstrology.Domain.Interfaces;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Calculation.Suriyayatra;

/// <summary>
/// Ports the shared polynomial chain from ក្បួនគណនា!AE151:KF161 and the
/// result bridge from សូរ្យយាត្រ!T45:T56.
/// </summary>
public sealed class ModernPlanetaryCalculator : IModernPlanetaryCalculator
{
    private readonly IModernPlanetaryReferenceDataSource _referenceDataSource;

    public ModernPlanetaryCalculator(IModernPlanetaryReferenceDataSource referenceDataSource)
    {
        _referenceDataSource = referenceDataSource;
    }

    public ModernPlanetaryCalculationResult Calculate(BirthInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var data = _referenceDataSource.Load();
        var utcJulianDay = AstronomicalTimeCalculator.CalculateUtcJulianDay(input);
        var era = SelectEra(data.Eras, input.BirthDate.Year);
        var positions = new List<ModernPlanetaryPosition>(data.Bodies.Count + 1);
        double? rahuLongitude = null;

        foreach (var reference in data.Bodies)
        {
            var longitude = CalculateLongitude(utcJulianDay, era, reference, data.Tables);
            var body = Enum.Parse<CelestialBody>(reference.Body, ignoreCase: false);
            positions.Add(new ModernPlanetaryPosition(body, longitude));
            if (body == CelestialBody.Rahu)
            {
                rahuLongitude = longitude;
            }
        }

        if (rahuLongitude is null)
        {
            throw new InvalidDataException("The modern planetary reference data does not contain Rahu.");
        }

        // Workbook សូរ្យយាត្រ!T53 = MOD(T52+10800,21600).
        positions.Insert(
            positions.FindIndex(position => position.Body == CelestialBody.Rahu) + 1,
            new ModernPlanetaryPosition(CelestialBody.KetuVeda, Mod(rahuLongitude.Value + 10_800D, 21_600D)));

        // Workbook ព្រះកេតុទិព្វ!G5:G10 and B36. G6/G7 are workbook constants;
        // G8/G9 apply the reverse-cycle formula and G10 converts degrees to arcminutes.
        var localJulianDay = AstronomicalTimeCalculator.CalculateLocalJulianDay(input);
        var ketuDivyaRemainder = Mod(localJulianDay + 329.47146836388902D, 679D);
        var ketuDivyaDegrees = Mod(360D - ketuDivyaRemainder * 360D / 679D, 360D);
        positions.Insert(
            positions.FindIndex(position => position.Body == CelestialBody.KetuVeda) + 1,
            new ModernPlanetaryPosition(CelestialBody.KetuDivya, ketuDivyaDegrees * 60D));

        return new ModernPlanetaryCalculationResult(utcJulianDay, positions);
    }

    private static double CalculateLongitude(
        double utcJulianDay,
        ModernPlanetaryEra era,
        ModernPlanetReference reference,
        IReadOnlyDictionary<string, string[]> tables)
    {
        var elapsedPeriods = Math.Floor((utcJulianDay - era.StartJulianDay) / reference.PeriodDays);
        var tableRow = (int)Math.Floor((reference.PhaseOffset + elapsedPeriods - 2D) / 8D) + 2;
        if (!tables.TryGetValue(era.TableColumn, out var table)
            || tableRow < 1
            || tableRow > table.Length)
        {
            throw new InvalidDataException(
                $"Modern planetary table {era.TableColumn}{tableRow} is unavailable.");
        }

        var blockNumber = (int)Mod(reference.PhaseOffset + elapsedPeriods - 2D, 8D);
        var blocks = table[tableRow - 1].Split('|', StringSplitOptions.None);
        if (blockNumber >= blocks.Length)
        {
            throw new InvalidDataException(
                $"Modern planetary coefficient block {blockNumber + 1} is unavailable in {era.TableColumn}{tableRow}.");
        }

        var coefficients = blocks[blockNumber]
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => double.Parse(value, CultureInfo.InvariantCulture))
            .ToArray();
        if (coefficients.Length < 3)
        {
            throw new InvalidDataException(
                $"Modern planetary coefficient block {era.TableColumn}{tableRow} has too few coefficients.");
        }

        var phase = 2D * (utcJulianDay - (era.StartJulianDay + elapsedPeriods * reference.PeriodDays))
            / reference.PeriodDays - 1D;
        var next = 0D;
        var nextNext = 0D;
        for (var index = coefficients.Length - 1; index >= 1; index--)
        {
            var current = coefficients[index] + 2D * phase * next - nextNext;
            nextNext = next;
            next = current;
        }

        return Mod(phase * next - nextNext + coefficients[0], 360D) * 60D;
    }

    private static ModernPlanetaryEra SelectEra(
        IReadOnlyList<ModernPlanetaryEra> eras,
        int astronomicalYear)
    {
        foreach (var era in eras)
        {
            if (astronomicalYear < era.StartYear)
            {
                return era;
            }
        }

        return eras[^1];
    }

    private static double Mod(double value, double divisor) => ((value % divisor) + divisor) % divisor;
}
