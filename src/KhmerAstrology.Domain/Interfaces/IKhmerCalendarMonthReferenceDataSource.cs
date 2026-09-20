using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Domain.Interfaces;

public interface IKhmerCalendarMonthReferenceDataSource
{
    KhmerCalendarMonthReference Get(int astronomicalYear, int gregorianMonth);
}
