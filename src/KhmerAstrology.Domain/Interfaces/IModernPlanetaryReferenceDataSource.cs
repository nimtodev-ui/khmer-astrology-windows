using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Domain.Interfaces;

public interface IModernPlanetaryReferenceDataSource
{
    ModernPlanetaryReferenceData Load();
}
