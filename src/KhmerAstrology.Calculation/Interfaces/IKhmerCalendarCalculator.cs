using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Calculation.Interfaces;

public interface IKhmerCalendarCalculator
{
    KhmerCalendarResult Calculate(BirthInput input);

    /// <summary>
    /// Calculates the workbook Atthabhujj year chain from the CE/BCE year
    /// parameter and the target day, month, and time entered on the sheet.
    /// A negative year represents BCE and is normalized using the workbook's
    /// no-year-zero convention.
    /// </summary>
    KhmerCalendarResult CalculateForYear(
        int ceOrBceYear,
        int day,
        int month,
        TimeOnly time);
}
