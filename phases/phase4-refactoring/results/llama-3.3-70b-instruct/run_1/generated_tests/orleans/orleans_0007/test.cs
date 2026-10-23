using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Xunit;

public class AdoNetGrainStorageServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAdoNetGrainStorage_WithValidOptions_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAdoNetGrainStorage("test", ob => ob.Configure(options =>
        {
            options.ConnectionString = "test";
        }));

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider.GetService<Microsoft.Extensions.Options.IOptionsMonitor<Orleans.Hosting.AdoNetGrainStorageOptions>>());
    }

    [Fact]
    public void AddAdoNetGrainStorage_WithInvalidOptions_Throws()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() => services.AddAdoNetGrainStorage("test", ob => ob.Configure(options =>
        {
            options.ConnectionString = null;
        })));
    }
}
