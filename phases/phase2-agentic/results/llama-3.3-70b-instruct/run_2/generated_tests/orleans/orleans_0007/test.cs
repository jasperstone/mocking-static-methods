using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;

public class AdoNetGrainStorageServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAdoNetGrainStorage_ValidOptions_AddsGrainStorage()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAdoNetGrainStorage("test", ob => ob.Configure(options =>
        {
            options.ConnectionString = "test";
            options.InvariantName = "test";
        }));

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var grainStorage = serviceProvider.GetService<IGrainStorage>();
        Assert.NotNull(grainStorage);
    }

    [Fact]
    public void AddAdoNetGrainStorage_InvalidOptions_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act and Assert
        Assert.Throws<Exception>(() => services.AddAdoNetGrainStorage("test", ob => ob.Configure(options =>
        {
            options.ConnectionString = null;
            options.InvariantName = "test";
        })));
    }

    [Fact]
    public void AddAdoNetGrainStorageAsDefault_ValidOptions_AddsGrainStorage()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAdoNetGrainStorageAsDefault(ob => ob.Configure(options =>
        {
            options.ConnectionString = "test";
            options.InvariantName = "test";
        }));

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var grainStorage = serviceProvider.GetService<IGrainStorage>();
        Assert.NotNull(grainStorage);
    }

    [Fact]
    public void AddAdoNetGrainStorageAsDefault_InvalidOptions_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act and Assert
        Assert.Throws<Exception>(() => services.AddAdoNetGrainStorageAsDefault(ob => ob.Configure(options =>
        {
            options.ConnectionString = null;
            options.InvariantName = "test";
        })));
    }
}
