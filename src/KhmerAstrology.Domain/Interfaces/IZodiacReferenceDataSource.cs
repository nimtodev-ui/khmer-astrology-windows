using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Domain.Interfaces;

public interface IZodiacReferenceDataSource
{
    IReadOnlyList<ZodiacSignReference> GetAll();
}
