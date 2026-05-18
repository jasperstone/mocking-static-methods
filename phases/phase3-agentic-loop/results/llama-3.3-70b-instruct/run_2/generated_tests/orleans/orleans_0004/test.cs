using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Xunit;

public class AdoNetGrainDirectoryServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAdoNetGrainDirectory_ValidOptions_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddAdoNetGrainDirectory("test", options => options.Configure(o => o.ConnectionString = "test"));

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void AddAdoNetGrainDirectory_InvalidOptions_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => services.AddAdoNetGrainDirectory(null, options => options.Configure(o => o.ConnectionString = "test")));
    }

    [Fact]
    public void AddAdoNetGrainDirectory_GetRequiredService_ReturnsService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<AdoNetGrainDirectoryOptions>("test");
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var result = serviceProvider.GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>().Get("test");

        // Assert
        Assert.NotNull(result);
    }
}
