using KhmerAstrology.Domain.Enums;

namespace KhmerAstrology.WinForms;

/// <summary>
/// Display names for planets and points, shared by the grids, the interpretation
/// text and the chart tooltips/legend.
/// </summary>
internal static class CelestialBodyNames
{
    public static string Get(CelestialBody body, bool isKhmer) => isKhmer
        ? body switch
        {
            CelestialBody.Ascendant => "លគ្នា",
            CelestialBody.Sun => "ព្រះអាទិត្យ",
            CelestialBody.Moon => "ព្រះចន្ទ",
            CelestialBody.Mars => "ព្រះអង្គារ",
            CelestialBody.Mercury => "ព្រះពុធ",
            CelestialBody.Jupiter => "ព្រះព្រហស្បតិ៍",
            CelestialBody.Venus => "ព្រះសុក្រ",
            CelestialBody.Saturn => "ព្រះសៅរ៍",
            CelestialBody.Rahu => "រាហូ",
            CelestialBody.Ketu => "ព្រះកេតុ",
            CelestialBody.KetuVeda => "ព្រះកេតុវេទ",
            CelestialBody.KetuDivya => "ព្រះកេតុទិព្វ",
            CelestialBody.Uranus => "ម្រឹត្យូវ (Uranus)",
            CelestialBody.Neptune => "ព្រះវរុណ (Neptune)",
            CelestialBody.Pluto => "ព្រះយម (Pluto)",
            CelestialBody.Mrityu => "ម្រឹត្យូវ (បុរាណ)",
            CelestialBody.Varuna => "ព្រះវរុណ (បុរាណ)",
            CelestialBody.Yama => "ព្រះយម (បុរាណ)",
            _ => body.ToString(),
        }
        : body switch
        {
            CelestialBody.Ascendant => "Lagna (Ascendant)",
            CelestialBody.KetuVeda => "Ketu Veda",
            CelestialBody.KetuDivya => "Ketu Divya",
            CelestialBody.Uranus => "Uranus (Mrityu)",
            CelestialBody.Neptune => "Neptune (Varuna)",
            CelestialBody.Pluto => "Pluto (Yama)",
            CelestialBody.Mrityu => "Mrityu (traditional)",
            CelestialBody.Varuna => "Varuna (traditional)",
            CelestialBody.Yama => "Yama (traditional)",
            _ => body.ToString(),
        };
}
