using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Domain.Interfaces;

public interface ILocationReferenceDataSource
{
    IReadOnlyList<AstrologyLocation> GetAll();
}
