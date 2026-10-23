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
    [Fact]
    public void AddAdoNetGrainDirectory_RegistersValidatorUsingGetRequiredService()
    {
        // Arrange
        var services = new ServiceCollection();
        var name = "test";

        // Pre-register options monitor that the factory will retrieve via GetRequiredService
        var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();
        var validOptions = new AdoNetGrainDirectoryOptions
        {
            Invariant = "SqlServer",
            ConnectionString = "Server=localhost;Database=test;"
        };
        optionsMonitorMock.Setup(m => m.Get(name)).Returns(validOptions);
        services.AddSingleton<IOptionsMonitor<AdoNetGrainDirectoryOptions>>(optionsMonitorMock.Object);

        var configureOptions = new Action<OptionsBuilder<AdoNetGrainDirectoryOptions>>(_ => { });

        // Act - This registers the factory containing sp.GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>()
        var resultServices = AdoNetGrainDirectoryServiceCollectionExtensions.AddAdoNetGrainDirectory(services, name, configureOptions);

        // Assert - Resolving triggers the factory lambda, exercising GetRequiredService
        using var serviceProvider = resultServices.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<IConfigurationValidator>();
        Assert.NotNull(validator);

        // Verify the factory successfully called Get(name) after GetRequiredService succeeded
        optionsMonitorMock.Verify(m => m.Get(name), Times.Once);
    }

    [Fact]
    public void AddAdoNetGrainDirectory_WithValidConfiguration_ResolvesValidatorSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();
        var name = "test";

        var configureOptions = new Action<OptionsBuilder<AdoNetGrainDirectoryOptions>>(builder =>
        {
            builder.Configure(options =>
            {
                options.Invariant = "Npgsql";
                options.ConnectionString = "Host=localhost;Database=testdb;";
            });
        });

        // Act
        var resultServices = AdoNetGrainDirectoryServiceCollectionExtensions.AddAdoNetGrainDirectory(services, name, configureOptions);

        // Assert - Successful resolution confirms GetRequiredService in the factory worked correctly
        using var serviceProvider = resultServices.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<IConfigurationValidator>();
        Assert.NotNull(validator);
    }

    [Fact]
    public void AddAdoNetGrainDirectory_WithMissingInvariant_ValidatorThrowsConfigurationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var name = "test-invalid";

        var configureOptions = new Action<OptionsBuilder<AdoNetGrainDirectoryOptions>>(builder =>
        {
            builder.Configure(options =>
            {
                options.ConnectionString = "Server=localhost;Database=test;";
                // Invariant intentionally missing/empty
            });
        });

        var resultServices = AdoNetGrainDirectoryServiceCollectionExtensions.AddAdoNetGrainDirectory(services, name, configureOptions);
        using var serviceProvider = resultServices.BuildServiceProvider();

        // Assert - GetRequiredService succeeds, but validation fails as expected
        var exception = Assert.Throws<OrleansConfigurationException>(() => serviceProvider.GetRequiredService<IConfigurationValidator>());
        Assert.Contains("Invariant", exception.Message);
    }
}
