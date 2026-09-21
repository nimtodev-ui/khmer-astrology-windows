using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Calculation.Interfaces;

public interface IAutomaticCalendarCalculator
{
    /// <summary>
    /// Calculates the complete daily table for the selected CE or BCE year and Gregorian month (1..12).
    /// Reproduces sheet 30 (ប្រតិទិនស្វ័យប្រវត្តិ) from the authoritative workbook.
    /// </summary>
    AutomaticCalendarMonthResult CalculateMonth(int ceOrBceYear, int gregorianMonth);

    /// <summary>
    /// Searches and calculates the full calendar info for a specific day in the selected month and year.
    /// Reproduces sheet 31 (ស្វែងរក ថ្ងៃខែឆ្នាំ) from the authoritative workbook.
    /// </summary>
    KhmerDateSearchResult SearchDate(int ceOrBceYear, int gregorianMonth, int day);
}
