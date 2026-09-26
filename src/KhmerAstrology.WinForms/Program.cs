using KhmerAstrology.Infrastructure.DependencyInjection;
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
        services.AddKhmerAstrologyServices();
        services.AddSingleton<UiFontProvider>();
        services.AddTransient<MainForm>();
    }
}
