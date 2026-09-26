using KhmerAstrology.Application.Interfaces;
using KhmerAstrology.Application.Services;
using KhmerAstrology.Calculation.Calendar;
using KhmerAstrology.Calculation.Charts;
using KhmerAstrology.Calculation.Houses;
using KhmerAstrology.Calculation.Interfaces;
using KhmerAstrology.Calculation.Suriyayatra;
using KhmerAstrology.Calculation.Zodiac;
using KhmerAstrology.Domain.Interfaces;
using KhmerAstrology.Infrastructure.ReferenceData;
using Microsoft.Extensions.DependencyInjection;

namespace KhmerAstrology.Infrastructure.DependencyInjection;

/// <summary>
/// Registers every non-UI service: workbook reference data, calculators and
/// application services. Shared by the WinForms host and the start-up test so
/// a missing registration fails a test instead of the application start-up.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKhmerAstrologyServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Workbook reference data
        services.AddSingleton<IKhmerCalendarYearReferenceDataSource, JsonKhmerCalendarYearReferenceDataSource>();
        services.AddSingleton<IModernPlanetaryReferenceDataSource, JsonModernPlanetaryReferenceDataSource>();
        services.AddSingleton<ILocationReferenceDataSource, JsonLocationReferenceDataSource>();
        services.AddSingleton<IZodiacReferenceDataSource, JsonZodiacReferenceDataSource>();
        services.AddSingleton<INakshatraReferenceDataSource, JsonNakshatraReferenceDataSource>();
        services.AddSingleton<ITraditionalOuterPointReferenceDataSource, JsonTraditionalOuterPointReferenceDataSource>();
        services.AddSingleton<IInterpretationReferenceDataSource, JsonInterpretationReferenceDataSource>();
        services.AddSingleton<ICelestialBodyReferenceDataSource, JsonCelestialBodyReferenceDataSource>();

        // Calculators
        services.AddSingleton<ILongitudeNormalizer, LongitudeNormalizer>();
        services.AddSingleton<IZodiacCalculator>(serviceProvider => new ZodiacCalculator(
            serviceProvider.GetRequiredService<ILongitudeNormalizer>(),
            serviceProvider.GetRequiredService<IZodiacReferenceDataSource>()));
        services.AddSingleton<INakshatraCalculator>(serviceProvider => new NakshatraCalculator(
            serviceProvider.GetRequiredService<ILongitudeNormalizer>(),
            serviceProvider.GetRequiredService<INakshatraReferenceDataSource>()));
        services.AddSingleton<D1Calculator>();
        services.AddSingleton<D3Calculator>();
        services.AddSingleton<D9Calculator>();
        services.AddSingleton<IHouseCalculator, HouseCalculator>();
        services.AddSingleton<IKhmerCalendarCalculator>(serviceProvider => new KhmerCalendarCalculator(
            serviceProvider.GetRequiredService<IKhmerCalendarYearReferenceDataSource>()));
        services.AddSingleton<IAutomaticCalendarCalculator, AutomaticCalendarCalculator>();
        services.AddSingleton<ISolarCalculator, SolarCalculator>();
        services.AddSingleton<ILunarCalculator, LunarCalculator>();
        services.AddSingleton<IAscendantCalculator, AscendantCalculator>();
        services.AddSingleton<IModernPlanetaryCalculator, ModernPlanetaryCalculator>();
        services.AddSingleton<ITraditionalOuterPointCalculator, TraditionalOuterPointCalculator>();

        // Application services
        services.AddSingleton<IFoundationCalculationService, FoundationCalculationService>();
        services.AddSingleton<IAstrologyCalculationService, AstrologyCalculationService>();
        services.AddSingleton<IInterpretationService, InterpretationService>();
        return services;
    }
}
