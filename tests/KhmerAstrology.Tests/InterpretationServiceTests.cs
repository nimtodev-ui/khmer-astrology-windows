using KhmerAstrology.Application.Services;
using KhmerAstrology.Domain.Enums;
using KhmerAstrology.Domain.Models;
using KhmerAstrology.Infrastructure.ReferenceData;

namespace KhmerAstrology.Tests;

/// <summary>
/// Covers the workbook planet catalog <c>សរុបតារាគ្រោះ!A5:H17</c> and its use in
/// house interpretations from <c>ទិន្នន័យព្យាករ</c>.
/// </summary>
public sealed class InterpretationServiceTests
{
    [Fact]
    public void CelestialBodyCatalog_CoversEveryCalculatedBody()
    {
        var catalog = new JsonCelestialBodyReferenceDataSource().GetAll();
        var covered = catalog.SelectMany(row => row.Bodies).ToHashSet();

        Assert.Equal(13, catalog.Count);
        Assert.All(Enum.GetValues<CelestialBody>().Where(body => body != CelestialBody.Ascendant),
            body => Assert.Contains(body, covered));
        Assert.All(catalog, row => Assert.False(string.IsNullOrWhiteSpace(row.MeaningKm)));
    }

    [Theory]
    [InlineData(CelestialBody.Sun, "ព្រះអាទិត្យ", "១")]
    [InlineData(CelestialBody.KetuVeda, "ព្រះកេតុវេទ", "៩")]
    [InlineData(CelestialBody.Uranus, "ម្រឹត្យូវ", "០")]
    [InlineData(CelestialBody.Pluto, "ព្រះយម", "យ")]
    public void CelestialBodyCatalog_MatchesWorkbookRows(CelestialBody body, string expectedNameKm, string expectedCode)
    {
        var row = new JsonCelestialBodyReferenceDataSource().GetAll().Single(row => row.Bodies.Contains(body));

        Assert.Equal(expectedNameKm, row.NameKm);
        Assert.Equal(expectedCode, row.Code);
    }

    [Fact]
    public void Interpret_IncludesHouseNameAndPlanetMeaning()
    {
        var service = new InterpretationService(
            new JsonInterpretationReferenceDataSource(),
            new JsonCelestialBodyReferenceDataSource());

        var result = Assert.Single(service.Interpret(
        [
            new PlanetPosition { Body = CelestialBody.Sun, House = 1 },
        ]));

        Assert.Equal(1, result.House);
        Assert.Equal("តនុ", result.HouseNameKm);
        Assert.StartsWith("ពន្លឺ អំណាច", result.BodyMeaningKm);
    }
}
