using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Domain.Interfaces;

public interface ITraditionalOuterPointReferenceDataSource
{
    IReadOnlyList<TraditionalOuterPointReference> Load();
}
