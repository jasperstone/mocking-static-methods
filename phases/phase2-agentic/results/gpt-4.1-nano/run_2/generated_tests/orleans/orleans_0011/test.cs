using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Orleans.Clustering.AzureStorage;
using Orleans.Configuration;
using Moq;

namespace Orleans.Tests
{
    public class AzureTableClusteringExtensionsTests
    {
        [Fact]
        public void UseAzureStorageClustering_WithConfigureOptions_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureStorageClusteringOptions>>();
            var options = new AzureStorageClusteringOptions();

            // Setup the IServiceProvider to return the options monitor
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<AzureStorageClusteringOptions>>())
                .Returns(optionsMonitorMock.Object);

            // Setup the options monitor to return options for default name
            optionsMonitorMock.Setup(om => om.Get(Options.DefaultName))
                .Returns(options);

            // Add the service provider to the services
            services.AddSingleton(serviceProviderMock.Object);

            // Create a builder mock
            var builderMock = new Mock<ISiloBuilder>();
            builderMock.Setup(b => b.ConfigureServices(It.IsAny<Action<IServiceCollection>>()))
                .Returns<Action<IServiceCollection>>(action =>
                {
                    // Invoke the action to simulate configuration
                    action(services);
                    return builderMock.Object;
                });

            // Act
            builderMock.Object.UseAzureStorageClustering(cfg => { /* no-op */ });

            // Assert
            // Verify that GetRequiredService was called
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<AzureStorageClusteringOptions>>(), Times.Once);
        }
    }
}
