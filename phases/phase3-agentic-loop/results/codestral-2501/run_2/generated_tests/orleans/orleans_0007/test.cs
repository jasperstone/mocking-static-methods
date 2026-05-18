using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Persistence.AdoNet.Storage;
using Orleans.Storage;
using Xunit;

public class AdoNetGrainStorageServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAdoNetGrainStorage_ShouldAddTransientService()
    {
        // Arrange
        var services = new ServiceCollection();
        Action<AdoNetGrainStorageOptions> configureOptions = opt => { };

        // Act
        services.AddAdoNetGrainStorage("Test", configureOptions);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var service = serviceProvider.GetService<IConfigurationValidator>();
        Assert.NotNull(service);
    }

    [Fact]
    public void AddAdoNetGrainStorage_ShouldConfigureOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        Action<AdoNetGrainStorageOptions> configureOptions = opt => { opt.UseJsonFormat = true; };

        // Act
        services.AddAdoNetGrainStorage("Test", configureOptions);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var optionsMonitor = serviceProvider.GetService<IOptionsMonitor<AdoNetGrainStorageOptions>>();
        var configuredOptions = optionsMonitor.Get("Test");
        Assert.True(configuredOptions.UseJsonFormat);
    }

    [Fact]
    public void AddAdoNetGrainStorage_ShouldAddGrainStorage()
    {
        // Arrange
        var services = new ServiceCollection();
        Action<AdoNetGrainStorageOptions> configureOptions = opt => { };

        // Act
        services.AddAdoNetGrainStorage("Test", configureOptions);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var grainStorage = serviceProvider.GetService<IGrainStorage>();
        Assert.NotNull(grainStorage);
    }
}
