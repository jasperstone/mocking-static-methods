using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Orleans.Configuration;
using Orleans.Messaging;
using Orleans.Runtime.MembershipService;
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

            // Setup the IServiceProvider to return the optionsMonitorMock when GetRequiredService is called
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<AzureStorageClusteringOptions>>())
                .Returns(optionsMonitorMock.Object);

            // Register the mock IServiceProvider
            services.AddSingleton(serviceProviderMock.Object);

            // Add a dummy builder
            var builder = new SiloBuilder();

            // Act
            var result = builder.UseAzureStorageClustering(opts =>
            {
                // configure options if needed
            });

            // Assert
            // Verify that GetRequiredService was called during the registration
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<AzureStorageClusteringOptions>>(), Times.Once);
            Assert.NotNull(result);
        }
    }

    // Dummy implementations to allow compilation
    public class SiloBuilder : ISiloBuilder
    {
        public IServiceCollection Services { get; } = new ServiceCollection();

        public ISiloBuilder ConfigureServices(Action<IServiceCollection> configure)
        {
            configure(Services);
            return this;
        }
    }

    // Interfaces from Orleans
    public interface ISiloBuilder
    {
        IServiceCollection Services { get; }
        ISiloBuilder ConfigureServices(Action<IServiceCollection> configure);
        // other members omitted
    }
}
