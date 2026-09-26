using KhmerAstrology.Application.Interfaces;
using KhmerAstrology.Domain.Models;
using KhmerAstrology.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace KhmerAstrology.Tests;

/// <summary>
/// Guards the composition root used by the WinForms host: every registered
/// service must resolve (which also loads its deployed reference data), so a
/// missing registration or data file fails here instead of at app start-up.
/// </summary>
public sealed class ServiceRegistrationTests
{
    [Fact]
    public void AddKhmerAstrologyServices_ResolvesEveryRegisteredService()
    {
        var services = new ServiceCollection().AddKhmerAstrologyServices();
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true,
        });

        Assert.All(services, descriptor => Assert.NotNull(provider.GetRequiredService(descriptor.ServiceType)));
    }

    [Fact]
    public void AddKhmerAstrologyServices_CalculatesAWorkbookSample()
    {
        using var provider = new ServiceCollection().AddKhmerAstrologyServices().BuildServiceProvider();

        var result = provider.GetRequiredService<IAstrologyCalculationService>().Calculate(new BirthInput
        {
            Name = "Composition root sample",
            BirthDate = new DateOnly(2024, 10, 28),
            BirthTime = new TimeOnly(20, 0, 12),
            Latitude = 11.55,
            Longitude = 104.92,
            TimeZoneId = "Asia/Phnom_Penh",
        });

        Assert.Equal(12, result.D1.Houses.Count);
        Assert.NotEmpty(result.Planets);
        Assert.NotEmpty(result.Interpretations);
    }
}
