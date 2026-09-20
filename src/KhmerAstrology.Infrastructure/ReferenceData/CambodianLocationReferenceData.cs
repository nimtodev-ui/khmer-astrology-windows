using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Infrastructure.ReferenceData;

/// <summary>
/// Cambodian locations transcribed from the workbook's <c>ទីតាំងកម្ពុជា</c> sheet.
/// </summary>
public static class CambodianLocationReferenceData
{
    public static IReadOnlyList<AstrologyLocation> All { get; } =
    [
        new() { Id = 1, NameEn = "Phnom Penh", NameKm = "ក្រុងភ្នំពេញ", Province = "Phnom Penh", Latitude = 11.55, Longitude = 104.92 },
        new() { Id = 2, NameEn = "Banteay Meanchey", NameKm = "ក្រុងសិរីសោភ័ណ", Province = "Banteay Meanchey", Latitude = 13.59, Longitude = 102.97 },
        new() { Id = 3, NameEn = "Battambang", NameKm = "ក្រុងបាត់ដំបង", Province = "Battambang", Latitude = 13.10, Longitude = 103.20 },
        new() { Id = 4, NameEn = "Kampong Cham", NameKm = "ក្រុងកំពង់ចាម", Province = "Kampong Cham", Latitude = 11.99, Longitude = 105.46 },
        new() { Id = 5, NameEn = "Kampong Chhnang", NameKm = "ក្រុងកំពង់ឆ្នាំង", Province = "Kampong Chhnang", Latitude = 12.25, Longitude = 104.66 },
        new() { Id = 6, NameEn = "Kampong Speu", NameKm = "ក្រុងច្បារមន", Province = "Kampong Speu", Latitude = 11.45, Longitude = 104.52 },
        new() { Id = 7, NameEn = "Kampong Thom", NameKm = "ក្រុងស្ទឹងសែន", Province = "Kampong Thom", Latitude = 12.71, Longitude = 104.88 },
        new() { Id = 8, NameEn = "Kampot", NameKm = "ក្រុងកំពត", Province = "Kampot", Latitude = 10.61, Longitude = 104.18 },
        new() { Id = 9, NameEn = "Kandal", NameKm = "ក្រុងតាខ្មៅ", Province = "Kandal", Latitude = 11.48, Longitude = 104.95 },
        new() { Id = 10, NameEn = "Koh Kong", NameKm = "ក្រុងខេមរភូមិន្ទ", Province = "Koh Kong", Latitude = 11.61, Longitude = 102.98 },
        new() { Id = 11, NameEn = "Kratie", NameKm = "ក្រុងក្រចេះ", Province = "Kratie", Latitude = 12.48, Longitude = 106.01 },
        new() { Id = 12, NameEn = "Mondulkiri", NameKm = "ក្រុងសែនមនោរម្យ", Province = "Mondulkiri", Latitude = 12.45, Longitude = 107.18 },
        new() { Id = 13, NameEn = "Preah Vihear", NameKm = "ក្រុងព្រះវិហារ/ត្បែងមានជ័យ", Province = "Preah Vihear", Latitude = 13.80, Longitude = 104.98 },
        new() { Id = 14, NameEn = "Preah Sihanouk", NameKm = "ក្រុងព្រះសីហនុ", Province = "Preah Sihanouk", Latitude = 10.62, Longitude = 103.52 },
        new() { Id = 15, NameEn = "Pursat", NameKm = "ក្រុងពោធិ៍សាត់", Province = "Pursat", Latitude = 12.53, Longitude = 103.91 },
        new() { Id = 16, NameEn = "Prey Veng", NameKm = "ក្រុងព្រៃវែង", Province = "Prey Veng", Latitude = 11.48, Longitude = 105.33 },
        new() { Id = 17, NameEn = "Ratanakiri", NameKm = "ក្រុងបានលុង", Province = "Ratanakiri", Latitude = 13.74, Longitude = 107.00 },
        new() { Id = 18, NameEn = "Siem Reap", NameKm = "ក្រុងសៀមរាប", Province = "Siem Reap", Latitude = 13.36, Longitude = 103.85 },
        new() { Id = 19, NameEn = "Stung Treng", NameKm = "ក្រុងស្ទឹងត្រែង", Province = "Stung Treng", Latitude = 13.52, Longitude = 105.97 },
        new() { Id = 20, NameEn = "Svay Rieng", NameKm = "ក្រុងស្វាយរៀង", Province = "Svay Rieng", Latitude = 11.08, Longitude = 105.80 },
        new() { Id = 21, NameEn = "Takeo", NameKm = "ក្រុងដូនកែវ", Province = "Takeo", Latitude = 10.99, Longitude = 104.78 },
        new() { Id = 22, NameEn = "Tboung Khmum", NameKm = "ក្រុងសួង/ត្បូងឃ្មុំ", Province = "Tboung Khmum", Latitude = 11.89, Longitude = 105.88 },
        new() { Id = 23, NameEn = "Pailin", NameKm = "ក្រុងប៉ៃលិន", Province = "Pailin", Latitude = 12.85, Longitude = 102.61 },
        new() { Id = 24, NameEn = "Kep", NameKm = "ក្រុងកែប", Province = "Kep", Latitude = 10.48, Longitude = 104.31 },
        new() { Id = 25, NameEn = "Oddar Meanchey", NameKm = "ក្រុងសំរោង", Province = "Oddar Meanchey", Latitude = 14.190411388888901, Longitude = 103.51157 }
    ];
}
