using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using Orleans.Configuration;
using Orleans.Runtime.MembershipService;
using System;

namespace Orleans.Tests
{
    public class AzureTableClusteringExtensionsTests
    {
        [Fact]
        public void UseAzureStorageClustering_RegistersValidatorAndResolvesIt()
        {
            // Arrange
            var services = new ServiceCollection();

            // Mock IOptionsMonitor<AzureStorageClusteringOptions>
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureStorageClusteringOptions>>();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>()))
                .Returns(new AzureStorageClusteringOptions());

            // Register the mock as singleton
            services.AddSingleton(optionsMonitorMock.Object);

            var builder = new Mock<ISiloBuilder>();
            builder.Setup(b => b.ConfigureServices(It.IsAny<Action<IServiceCollection>>()))
                .Callback<Action<IServiceCollection>>(configure => configure(services))
                .Returns(builder.Object);

            // Act
            builder.Object.UseAzureStorageClustering(opts => { /* no-op */ });

            var provider = services.BuildServiceProvider();

            // Assert
            var validator = provider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
            Assert.IsType<AzureStorageClusteringOptionsValidator>(validator);
        }
    }
}
