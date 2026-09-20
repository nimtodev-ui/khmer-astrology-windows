using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Domain.Interfaces;

public interface INakshatraReferenceDataSource
{
    IReadOnlyList<NakshatraReference> GetAll();
}
