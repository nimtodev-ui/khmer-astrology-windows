using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Domain.Interfaces;

public interface ICelestialBodyReferenceDataSource
{
    IReadOnlyList<CelestialBodyReference> GetAll();
}
