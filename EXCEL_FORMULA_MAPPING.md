# Excel Formula Mapping

This document is the first migration ledger for:

`C:\Users\to.nim\Desktop\Suriyayatra\គម្ពីរសូរ្យយាត្រ៥១០៣ ឆ្នាំ_MASTER_FINAL.xlsx`

The workbook was inspected as an `.xlsx` package without opening Microsoft Excel. Cached values are recorded where useful, but cached values are not treated as a substitute for a formula implementation. Every migrated formula must keep an entry here and gain a matching automated test.

## Workbook inventory

The workbook contains 32 worksheets, 1,492 formulas on the main `សូរ្យយាត្រ` sheet, and 1,634 formulas on `ក្បួនគណនា`.

## Attached Khmer workbook audit

The current authoritative reference is the attached Khmer workbook named above. A fresh extraction from that file was compared with the application assets and formula inventory:

- The generated modern planetary coefficient archive matches the attached workbook byte-for-byte.
- The generated automatic-calendar month-code archive matches the attached workbook byte-for-byte.
- Zodiac names/start positions, all 27 Nakshatra names/start positions, all 25 Cambodian location coordinates, and all 12 interpretation rows match the attached workbook.
- The calculation-bearing sheets for Sun, Moon, Mars, Mercury, Venus, Jupiter, Saturn, Rahu, Ketu Divya, Ketu Veda, Uranus, Neptune, Pluto, Ascendant, and the coefficient core retain the same numeric formulas as the previously inspected workbook.
- Sheets `អដ្ឋភុជ្ជ`, `សូរ្យយាត្រ`, `រាសិចក្ក D1-D9-D3`, `ប្រតិទិនស្វ័យប្រវត្តិ`, and `ស្វែងរក ថ្ងៃខែឆ្នាំ` contain Khmer display-formula changes and additional derived display cells. These do not replace the numeric planetary coefficient or Ascendant chains; the C# services use the numeric rules and extracted reference tables directly.
- Cached workbook values are not treated as authoritative when workbook inputs are inconsistent across sheets. For example, the attached file contains different cached years in the calendar and planetary sheets. Formula source and user-input-driven recalculation are therefore the validation basis.

| # | Excel sheet | Used range | Formula cells | Purpose | Initial C# boundary |
|---:|---|---|---:|---|---|
| 1 | `អដ្ឋភុជ្ជ` | `A1:J47` | 38 | Suriyayātra/Atthabhujja calendar bridge, Ahargana, Kammaja, Maha Sankranta display | `KhmerAstrology.Calculation.Calendar` in a later phase |
| 2 | `សូរ្យយាត្រ` | `A1:BQ832` | 1,492 | Main input bridge, location selection, planet result table, D1/D9/D3 display fields | Inputs and derived result mapping |
| 3 | `សរុបតារាគ្រោះ` | `A1:J30` | 0 | Catalog of 13 planets and shadow points, labels and source notes | `celestial-bodies.json` via `JsonCelestialBodyReferenceDataSource`; meanings used by `InterpretationService` (`InterpretationServiceTests`) |
| 4 | `ព្រះអាទិត្យ` | `A1:H61` | 28 | Sun calculation chain: mean, mandakendra, mandaphala, sphuta | Future `SolarCalculator` |
| 5 | `ព្រះចន្ទ` | `A1:D59` | 38 | Moon calculation chain: mean Moon, apogee, correction, sphuta | Future `LunarCalculator` |
| 6 | `ព្រះអង្គារ` | `A1:I103` | 70 | Mars Suriyayātra calculation chain | Future `PlanetaryCalculator` |
| 7 | `ព្រះពុធ` | `A1:I109` | 67 | Mercury calculation chain and retrograde notes | Future `PlanetaryCalculator` |
| 8 | `ព្រះសុក្រ` | `A1:G139` | 81 | Venus calculation chain and external audit notes | Future `PlanetaryCalculator` |
| 9 | `ព្រះព្រហស្បតិ៍` | `A1:G138` | 88 | Jupiter calculation chain and audit notes | Future `PlanetaryCalculator` |
| 10 | `សូរ្យយាត្រចាស់` | `A1:EM467` | 472 | Legacy Suriyayātra tables and historical comparison blocks | Validation source only |
| 11 | `ក្បួនគណនា` | `A1:KF180` | 1,634 | Calculation core, longitude tables, time-zone calculations and final source longitudes | Future calculation engine; do not copy into Forms |
| 12 | `ទិន្នន័យរាសី` | `A1:D13` | 0 | Zodiac reference table, Khmer names, intercalary/time-unit fields | `ZodiacCalculator` reference |
| 13 | `ទិន្នន័យនក្ខត្តឫក្ស` | `A1:C28` | 0 | 27 Nakshatra names and start arcminutes | `NakshatraCalculator` reference |
| 14 | `ទិន្នន័យឋានៈ` | `A1:E15` | 0 | Planet dignity/status lookup | Not migrated: B2:D15 are empty and E2:E15 read "ត្រូវផ្ទៀងផ្ទាត់តាមគម្ពីរ" in the attached workbook |
| 15 | `ទិន្នន័យព្យាករ` | `A1:D13` | 0 | House names and interpretation text | Future `InterpretationService` |
| 16 | `ទីតាំងកម្ពុជា` | `A1:H26` | 0 | 25 Cambodian location rows with latitude, longitude, UTC and Khmer display names | Infrastructure reference data |
| 17 | `ពេលវេលាព្រះអាទិត្យ` | `A1:G865` | 0 | Sunrise, solar noon and sunset lookup table | Future location/solar service |
| 18 | `តំបន់ម៉ោងអន្តរជាតិ` | `A1:AX3788` | 468 | IANA zones, offsets, DST and transition data | Future timezone service |
| 19 | `ប្រភពអន្តរជាតិ` | `A1:D10` | 0 | External source registry and usage notes | Documentation/source audit |
| 20 | `Lists` | `A1:E500` | 0 | English/Khmer display mappings and selection lists | Infrastructure reference data |
| 21 | `សវនកម្មវក្រៈ` | `A1:F99` | 23 | Retrograde/external audit checks | Future validation only |
| 22 | `ព្រះសៅរ៍` | `A1:K139` | 81 | Saturn calculation chain and audit fields | Future `PlanetaryCalculator` |
| 23 | `រាហូ` | `A1:E53` | 26 | Rahu calculation chain | Future `PlanetaryCalculator` |
| 24 | `ព្រះកេតុទិព្វ` | `A1:J58` | 33 | Traditional Ketu Divya calculation chain | Future shadow-point calculator |
| 25 | `ព្រះកេតុវេទ` | `A1:E53` | 22 | Traditional Ketu Veda calculation chain | Future shadow-point calculator |
| 26 | `ម្រឹត្យូវ` | `A1:K139` | 81 | Mrityu calculation/reference sheet | Future workbook point; not silently omitted |
| 27 | `ព្រះវរុណ` | `A1:J122` | 85 | Varuna calculation/reference sheet | Future workbook point; not silently omitted |
| 28 | `ព្រះយម` | `A1:J122` | 75 | Yama calculation/reference sheet | Future workbook point; not silently omitted |
| 29 | `រាសិចក្ក D1-D9-D3` | `A1:BR70` | 352 | D1, D9/Navamsa, D3/Triamsa tables and chart layout | `D1Calculator`, `D9Calculator`, `D3Calculator` |
| 30 | `ប្រតិទិនស្វ័យប្រវត្តិ` | `A1:CK371` | 3,112 | Automatic Khmer calendar table | Future `KhmerCalendarCalculator` |
| 31 | `ស្វែងរក ថ្ងៃខែឆ្នាំ` | `A1:AR371` | 2,598 | Date search and calendar lookup UI support | Future calendar UI/reference |
| 32 | `តារាងប្រតិទិន` | `A1:G12` | 0 | Calendar coverage and status notes | Validation/documentation |

## Inputs and final outputs

### Main input bridge: `សូរ្យយាត្រ`

| Cell | Label | Role |
|---|---|---|
| `B3` | CE Year (+) / BCE Year (-) | Calendar year input |
| `B4` | Day | Calendar day input |
| `B5` | Month | Calendar month input |
| `B7:B9` | Hour, Minute, Second | Local clock input |
| `B10` | Cambodia / international link mode | Location mode input |
| `B11` | Cambodian capital/province | Cambodian location selection |
| `B12:B13` | International country and state/region | International location selection |
| `B49:B53` | Active country, city/region, latitude, longitude, IANA zone | Resolved location bridge |
| `B54` | UTC override (optional) | Optional timezone override |
| `B55` | UTC Effective | Resolved UTC offset |
| `B56` | UTC Instant | `=(DATE(B6,B5,B4)+TIME(B7,B8,B9))-B55/24` |

### Main result table: `សូរ្យយាត្រ!A17:R30`

| Range | Purpose | Source/dependency |
|---|---|---|
| `A17:A30` | Workbook body labels | Includes Ascendant, Sun, Moon, Mars, Mercury, Jupiter, Venus, Saturn, Rahu, Ketu Veda, Ketu Divya, Uranus, Neptune and Pluto |
| `B17:B30` | Longitude in arcminutes | Ascendant from `ក្បួនគណនា!Q52`; modern bodies from `ក្បួនគណនា!T45:T56`; Ketu Divya from `ព្រះកេតុទិព្វ!B36` |
| `C17:C30` | Zodiac display name | `INT(MOD(Bx,21600)/1800)+1` mapped through `ទិន្នន័យរាសី`/`Lists` |
| `D17:D30` | Whole-degree component | `INT(MOD(Bx,1800)/60)` |
| `E17:E30` | Arcminute component | `INT(MOD(Bx,60))` |
| `F17:F30` | Arcsecond component | `ROUND(MOD(Bx,1)*60,2)` for modern rows; Ascendant uses workbook display rounding |
| `G17:G30` | Nakshatra and Pada display | `INT(MOD(Bx,21600)/800)+1` and `INT(MOD(Bx,800)/200)+1` |
| `H17:H30` | English Nakshatra | Workbook’s 27-name `CHOOSE` list; reference table is `ទិន្នន័យនក្ខត្តឫក្ស` |
| `J17:J30` | Navamsa/D9 display | `MOD(INT(MOD(Bx,21600)/200),12)+1` |
| `K17:K30` | Triamsa/D3 display | `MOD(INT(MOD(Bx,21600)/1800)+4*INT(MOD(Bx,1800)/600),12)+1` |

The workbook's D1/D9/D3 dashboard repeats these formulas in `រាសិចក្ក D1-D9-D3!D5:F18` and maps them to chart cells. The workbook does not expose a separate house-system calculation in the inspected chart sheet; the initial C# `HouseCalculator` therefore implements the explicitly requested whole-sign method and is marked as pending workbook house-system validation.

## Foundational formula migration ledger

The following entries are the only formulas migrated in the first implementation phase.

| Excel sheet | Excel cell/range | Formula/specification | Purpose | C# service | C# method |
|---|---|---|---|---|---|
| `សូរ្យយាត្រ` | `B17:B30` (input to derived columns) | Longitude measured in arcminutes; normalized with `MOD(value,21600)` in downstream formulas | Canonical angular coordinate | `KhmerAstrology.Calculation` | `LongitudeNormalizer.Normalize` |
| `សូរ្យយាត្រ` | `C17:C30`; `រាសិចក្ក D1-D9-D3!D5:D18` | `INT(MOD(Longitude,21600)/1800)+1` | D1 zodiac sign number 1–12 | `ZodiacCalculator` / `D1Calculator` | `Calculate` |
| `សូរ្យយាត្រ` | `D17:D30` | `INT(MOD(Longitude,1800)/60)` | Degree inside sign | `ZodiacCalculator` | `Calculate` |
| `សូរ្យយាត្រ` | `E17:E30` | `INT(MOD(Longitude,60))` | Arcminutes inside sign | `ZodiacCalculator` | `Calculate` |
| `សូរ្យយាត្រ` | `F17:F30` | `ROUND(MOD(Longitude,1)*60,2)` | Arcseconds inside sign | `ZodiacCalculator` | `Calculate` |
| `សូរ្យយាត្រ` | `G17:G30`; `H17:H30` | `INT(MOD(Longitude,21600)/800)+1` | 27 Nakshatra index | `NakshatraCalculator` | `Calculate` |
| `សូរ្យយាត្រ` | `G17:G30` | `INT(MOD(Longitude,800)/200)+1` | Nakshatra Pada 1–4 | `NakshatraCalculator` | `Calculate` |
| `សូរ្យយាត្រ` | `J17:J30`; `រាសិចក្ក D1-D9-D3!E5:E18` | `MOD(INT(MOD(Longitude,21600)/200),12)+1` | Navamsa/D9 sign | `D9Calculator` | `CalculateSign` |
| `សូរ្យយាត្រ` | `K17:K30`; `រាសិចក្ក D1-D9-D3!F5:F18` | `MOD(INT(MOD(Longitude,21600)/1800)+4*INT(MOD(Longitude,1800)/600),12)+1` | Triamsa/D3 sign | `D3Calculator` | `CalculateSign` |
| `រាសិចក្ក D1-D9-D3` | `D5:D18`, `E5:E18`, `F5:F18` | Each row references `សូរ្យយាត្រ!B17:B30` and applies the formulas above | Dashboard validation of divisional signs | `DivisionalChartCalculator` | `CalculateSign` |
| Not found as an Excel formula in inspected chart sheets | N/A | `((planetSign - ascendantSign + 12) % 12) + 1` | Initial whole-sign house fallback required by the product specification | `HouseCalculator` | `CalculateWholeSignHouse` |
| `អដ្ឋភុជ្ជ` | `B33:B35` | `INT((365*B47+សូរ្យយាត្រ!B4+INT(275*សូរ្យយាត្រ!B5/9)+2*INT(0.5+1/សូរ្យយាត្រ!B5)+INT(B47/4)-INT(B47/100)+INT(B47/400)+2-233142)+1)` and the corresponding time fraction | Ahargana day and fractional day | `KhmerCalendarCalculator` | `CalculateAhargana` |
| `អដ្ឋភុជ្ជ` | `B34` | `IF(MOD(B35,1)=0,800,800-MOD(B35,1)*800)` | Kammaja / solar-day fraction | `KhmerCalendarCalculator` | `Calculate` |
| `អដ្ឋភុជ្ជ` | `B37:B40` | Khmer-year epoch and Julian-day expressions: B38 = Rise of Sak (ឡើងស័ក, correction 0), B39 = Maha Sankranta (មហាសង្ក្រាន្ត, correction 2.165 days), B40 = March 31 reference Julian day | Sankranta reference values | `KhmerCalendarCalculator` | `CalculateSankranta` |
| `អដ្ឋភុជ្ជ` | `B41:B46`, `B30:B32`, `F7` | Day/time/weekday: B41/B42/B45 = Rise of Sak (B30/F7), B43/B44/B46 = Maha Sankranta (B31/B32) | Maha Sankranta and Rise of Sak date/time and weekdays | `KhmerCalendarCalculator` | `Calculate` |
| `អដ្ឋភុជ្ជ` | `B14:B28` | `MOD`, `QUOTIENT`, `IF` rules from the workbook | Avamana, Masakendra, Bori Tithi and lunar-year classification | `KhmerCalendarCalculator` | `Calculate` |
| `ប្រតិទិនស្វ័យប្រវត្តិ` | `BC2:BM2`, `BZ2:CC2`, `CE2:CJ2` | Encoded 10-character month records selected by astronomical year and decoded into Khmer year, lunar-year type, intercalary flag, lunar month index, and tithi offset | Automatic calendar month reference table | `JsonKhmerCalendarMonthReferenceDataSource` | `Get` |
| `ប្រតិទិនស្វ័យប្រវត្តិ` | `A5:Q371` | Date rows derive weekday, lunar day/phase, tithi name, lunar month, animal year, Sesa-Kala-Yoga, Yuga, and related era fields from the selected month code | User-specific traditional calendar date | `KhmerCalendarCalculator` | `ApplyDateLevelCalendar` |
| `ព្រះអាទិត្យ` | `B10:B38` | Mean remainder, mean longitude, 600-arcminute shadow segments, signed correction, and `MOD(...,21600)` | Traditional workbook Sun longitude | `SolarCalculator` | `Calculate` |
| `ព្រះចន្ទ` | `B12:B46` | Avamana/Bori Tithi base, mean Moon, mean exaltation, 900-arcminute shadow segments, signed correction, and `MOD(...,21600)` | Traditional workbook Moon longitude | `LunarCalculator` | `Calculate` |
| `ក្បួនគណនា` | `Q41:Q52` | `Q41=Q56-2415018.5`; `Q42=Q56`; `Q43=(Q42-2451545)/36525`; `Q44=MOD(280.46061837+360.98564736629*(Q42-2451545)+0.000387933*Q43^2-Q43^3/38710000,360)`; `Q45=សូរ្យយាត្រ!B52`; `Q46=MOD(Q44+Q45,360)`; `Q47=សូរ្យយាត្រ!B51`; `Q48` mean-obliquity polynomial; `Q49` quadrant-aware tropical ascendant; `Q50` Lahiri polynomial; `Q51=MOD(Q49-Q50,360)`; `Q52=Q51*60` | Modern workbook Ascendant longitude in arcminutes, with auditable intermediate values | `AscendantCalculator` | `Calculate` / `CalculateLongitudeArcMinutes` |
| `ក្បួនគណនា` | `AE151:KF161` | Shared polynomial coefficient/interpolation table; period and phase metadata are read from rows 151:161 and the workbook's semicolon-delimited coefficient blocks are evaluated with the same backward recurrence as the source formulas | Modern planetary longitudes for the main result table | `ModernPlanetaryCalculator` | `Calculate` |
| `សូរ្យយាត្រ` | `T45:T56` | Main result-table bridge that consumes the calculation-core longitude rows | Workbook body-to-longitude mapping | `ModernPlanetaryCalculator` | `Calculate` |
| `សូរ្យយាត្រ` | `T53` | `MOD(T52+10800,21600)` | Ketu Veda longitude derived from Rahu | `ModernPlanetaryCalculator` | `CalculateKetuVeda` |
| `ព្រះកេតុទិព្វ` | `G5:G10`, `B36` | `G8=MOD(G5+G6,G7)`; `G9=MOD(360-G8*360/G7,360)`; `G10=G9*60`; `B36=G10`; workbook constants `G6=329.47146836388902`, `G7=679` | Ketu Divya longitude | `ModernPlanetaryCalculator` | `CalculateKetuDivya` |
| `ទិន្នន័យព្យាករ` | `A1:D13` | Twelve workbook house names/descriptions with source column D | House interpretation reference data | `JsonInterpretationReferenceDataSource` / `InterpretationService` | `GetAll` / `Interpret` |
| `ម្រឹត្យូវ` | `B12:B91`, `D13:J20` | Mean remainder plus four correction motions, 900-arcminute shadow tables, and final `B91` longitude | Mrityu traditional outer-point chain | `TraditionalOuterPointCalculator` | `Calculate` |
| `ព្រះវរុណ` | `B13:B91`, `D13:J20` | Mean remainder plus four correction motions, 900-arcminute shadow tables, and final `B91` longitude | Varuna traditional outer-point chain | `TraditionalOuterPointCalculator` | `Calculate` |
| `ព្រះយម` | `B13:B91`, `D13:J20` | Rounded mean remainder plus sign-based bhujja rules, four correction motions, and final `B91` longitude | Yama traditional outer-point chain | `TraditionalOuterPointCalculator` | `Calculate` |

## Dependency chain observed

```text
Input date/time/location
    -> សូរ្យយាត្រ!B3:B13, B49:B55
    -> អដ្ឋភុជ្ជ!B33:B47 (Ahargana/calendar bridge)
    -> ក្បួនគណនា!Q52 and T45:T56 (calculation core longitudes)
    -> សូរ្យយាត្រ!B17:B30 (longitude result table)
    -> normalize longitude
    -> D1 / degree-minute-second / Nakshatra-Pada / D9 / D3
    -> រាសិចក្ក D1-D9-D3 chart dashboard
```

The future planetary migration must preserve the intermediate chain. In particular, `ក្បួនគណនា!Q52` is the workbook Ascendant source and `ក្បួនគណនា!T45:T56` are the modern longitude source cells; no UI code should reproduce those formulas.

## Workbook observations requiring validation before later phases

1. The workbook has both `Ketu Veda` and `Ketu Divya`; it does not use only a generic Ketu row in the main result table.
2. Additional workbook point sheets exist for `ម្រឹត្យូវ`, `ព្រះវរុណ`, and `ព្រះយម`. They are not silently discarded and must be classified before the final enum is frozen.
3. The cached value shown by `សូរ្យយាត្រ!C25` is `February` although the row formula is the same sign lookup pattern used by other body rows. The C# migration must validate the `Lists` mapping rather than copying this display value.
4. The inspected chart/dashboard is sign-based. A separate house-system formula was not found in this first inventory; whole-sign houses are therefore implemented as an explicit provisional service, not claimed as a workbook match.
5. `អដ្ឋភុជ្ជ` and the automatic calendar sheets contain many calendar rules and must be migrated only after these foundational tests are stable.
