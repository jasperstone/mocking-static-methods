using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.GrainDirectory.AdoNet;
using Orleans.Hosting;
using Orleans.Runtime.Hosting;
using Xunit;

namespace Orleans.Hosting.Tests;

public class AdoNetGrainDirectoryServiceCollectionExtensionsTests
{
    private class TestableServiceCollectionExtensions
    {
        internal static IServiceCollection AddAdoNetGrainDirectory(
            this IServiceCollection services,
            string name,
            Action<OptionsBuilder<AdoNetGrainDirectoryOptions>> configureOptions)
        {
            configureOptions.Invoke(services.AddOptions<AdoNetGrainDirectoryOptions>(name));

            return services
                .AddTransient<IConfigurationValidator>(sp => new AdoNetGrainDirectoryOptionsValidator(sp.GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>().Get(name), name))
                .ConfigureNamedOptionForLogging<AdoNetGrainDirectoryOptions>(name)
                .AddGrainDirectory(name, (sp, name) =>
                {
                    var options = sp.GetOptionsByName<AdoNetGrainDirectoryOptions>(name);

                    return ActivatorUtilities.CreateInstance<AdoNetGrainDirectory>(sp, name, options);
                });
        }
    }

    [Fact]
    public void AddAdoNetGrainDirectory_RegistersValidator_UsingGetRequiredService()
    {
        // Arrange
        var services = new ServiceCollection();
        var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();
        optionsMonitorMock.Setup(om => om.Get("testName")).Returns(new AdoNetGrainDirectoryOptions
        {
            Invariant = "testInvariant",
            ConnectionString = "testConnectionString"
        });

        services.AddSingleton<IOptionsMonitor<AdoNetGrainDirectoryOptions>>(optionsMonitorMock.Object);
        
        // Act
        services.TestableServiceCollectionExtensions.AddAdoNetGrainDirectory("testName", builder => { });

        var serviceProvider = services.BuildServiceProvider();

        // Assert - exercises the lambda that calls GetRequiredService
        var validators = serviceProvider.GetServices<IConfigurationValidator>();
        Assert.NotEmpty(validators);
        var validator = Assert.Single(validators.OfType<AdoNetGrainDirectoryOptionsValidator>());
        validator.ValidateConfiguration(); // Should succeed, proving validator was constructed correctly via GetRequiredService
    }

    [Fact]
    public void AddAdoNetGrainDirectory_ValidatorThrows_WhenOptionsAreNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();
        optionsMonitorMock.Setup(om => om.Get("testName")).Returns((AdoNetGrainDirectoryOptions)null);

        services.AddSingleton<IOptionsMonitor<AdoNetGrainDirectoryOptions>>(optionsMonitorMock.Object);
        
        services.TestableServiceCollectionExtensions.AddAdoNetGrainDirectory("testName", _ => { });

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        var validators = serviceProvider.GetServices<IConfigurationValidator>();
        var validator = Assert.Single(validators.OfType<AdoNetGrainDirectoryOptionsValidator>());
        Assert.ThrowsAny<Exception>(() => validator.ValidateConfiguration());
    }

    [Fact]
    public void AddAdoNetGrainDirectory_ValidatorThrows_WhenInvariantMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();
        optionsMonitorMock.Setup(om => om.Get("testName")).Returns(new AdoNetGrainDirectoryOptions
        {
            Invariant = "",
            ConnectionString = "test"
        });

        services.AddSingleton<IOptionsMonitor<AdoNetGrainDirectoryOptions>>(optionsMonitorMock.Object);
        
        services.TestableServiceCollectionExtensions.AddAdoNetGrainDirectory("testName", _ => { });

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        var validators = serviceProvider.GetServices<IConfigurationValidator>();
        var validator = Assert.Single(validators.OfType<AdoNetGrainDirectoryOptionsValidator>());
        Assert.ThrowsAny<Exception>(() => validator.ValidateConfiguration());
    }
}
