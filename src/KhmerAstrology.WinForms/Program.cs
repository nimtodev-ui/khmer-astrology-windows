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
using System.Diagnostics;

namespace KhmerAstrology.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ConfigureLogging();
        try
        {
            ApplicationConfiguration.Initialize();

            var services = new ServiceCollection();
            ConfigureServices(services);
            using var serviceProvider = services.BuildServiceProvider();
            System.Windows.Forms.Application.Run(serviceProvider.GetRequiredService<MainForm>());
        }
        catch (Exception exception)
        {
            Trace.WriteLine($"Startup failure: {exception}");
            MessageBox.Show(
                "Khmer Astrology could not start because a required reference file or service is unavailable. Review logs/application.log and verify the Data folder was deployed.",
                "Khmer Astrology startup error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private static void ConfigureLogging()
    {
        try
        {
            var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(logDirectory);
            Trace.Listeners.Add(new TextWriterTraceListener(Path.Combine(logDirectory, "application.log")));
            Trace.AutoFlush = true;
        }
        catch
        {
            // Logging must never prevent the application from showing its startup error.
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<UiFontProvider>();
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
        services.AddSingleton<IKhmerCalendarMonthReferenceDataSource, JsonKhmerCalendarMonthReferenceDataSource>();
        services.AddSingleton<IKhmerCalendarCalculator>(serviceProvider => new KhmerCalendarCalculator(
            serviceProvider.GetRequiredService<IKhmerCalendarMonthReferenceDataSource>()));
        services.AddSingleton<ISolarCalculator, SolarCalculator>();
        services.AddSingleton<ILunarCalculator, LunarCalculator>();
        services.AddSingleton<IAscendantCalculator, AscendantCalculator>();
        services.AddSingleton<IModernPlanetaryCalculator, ModernPlanetaryCalculator>();
        services.AddSingleton<ITraditionalOuterPointCalculator, TraditionalOuterPointCalculator>();
        services.AddSingleton<IFoundationCalculationService, FoundationCalculationService>();
        services.AddSingleton<IAstrologyCalculationService, AstrologyCalculationService>();
        services.AddSingleton<IModernPlanetaryReferenceDataSource, JsonModernPlanetaryReferenceDataSource>();
        services.AddSingleton<ILocationReferenceDataSource, JsonLocationReferenceDataSource>();
        services.AddSingleton<IZodiacReferenceDataSource, JsonZodiacReferenceDataSource>();
        services.AddSingleton<INakshatraReferenceDataSource, JsonNakshatraReferenceDataSource>();
        services.AddSingleton<ITraditionalOuterPointReferenceDataSource, JsonTraditionalOuterPointReferenceDataSource>();
        services.AddSingleton<IInterpretationReferenceDataSource, JsonInterpretationReferenceDataSource>();
        services.AddSingleton<IInterpretationService, InterpretationService>();
        services.AddTransient<MainForm>();
    }
}
