using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Domain.Interfaces;

public interface IInterpretationReferenceDataSource
{
    IReadOnlyList<InterpretationRule> GetAll();
}
