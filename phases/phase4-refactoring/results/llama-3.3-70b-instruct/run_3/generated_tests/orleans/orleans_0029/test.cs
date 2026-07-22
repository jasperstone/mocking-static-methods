using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Persistence.Cosmos;
using Microsoft.Extensions.Logging;

public class CosmosGrainStorageTests
{
    [Fact]
    public async Task Create_CallsGetRequiredService()
    {
        // Arrange
        var services = new ServiceCollection();
        var optionsMonitor = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
        services.AddSingleton(optionsMonitor.Object);
        var partitionKeyProvider = new Mock<IPartitionKeyProvider>();
        services.AddSingleton(partitionKeyProvider.Object);
        var loggerFactory = new Mock<ILoggerFactory>();
        services.AddSingleton(loggerFactory.Object);
        var clusterOptions = new Mock<IOptions<ClusterOptions>>();
        services.AddSingleton(clusterOptions.Object);
        var activatorProvider = new Mock<IActivatorProvider>();
        services.AddSingleton(activatorProvider.Object);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var storage = CosmosStorageFactory.Create(serviceProvider, "test");

        // Assert
        optionsMonitor.Verify(m => m.Get("test"), Times.Once);
    }
}
