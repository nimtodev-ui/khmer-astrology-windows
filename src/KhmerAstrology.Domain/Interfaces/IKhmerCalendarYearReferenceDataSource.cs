using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Domain.Interfaces;

public interface IKhmerCalendarYearReferenceDataSource
{
    KhmerCalendarYearReference Get(int astronomicalYear);
}
