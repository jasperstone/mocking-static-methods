using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans;
using Orleans.Clustering.AzureStorage;
using Xunit;

public class AzureTableClusteringExtensionsTests
{
    [Fact]
    public void UseAzureStorageClustering_ShouldRegisterConfigurationValidator()
    {
        // Arrange
        var services = new ServiceCollection();
        var configureOptions = new Action<OptionsBuilder<AzureStorageClusteringOptions>>(builder =>
        {
            builder.Configure(options => options.TableName = "TestTable");
        });

        var mockSiloBuilder = new Mock<ISiloBuilder>();
        mockSiloBuilder.Setup(b => b.ConfigureServices(It.IsAny<Action<IServiceCollection>>()))
            .Callback<Action<IServiceCollection>>(action => action(services));

        // Act
        mockSiloBuilder.Object.UseAzureStorageClustering(configureOptions);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<IConfigurationValidator>();

        Assert.NotNull(validator);
        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<AzureStorageClusteringOptions>>();
        Assert.Equal("TestTable", optionsMonitor.Get(Options.DefaultName).TableName);
    }
}
