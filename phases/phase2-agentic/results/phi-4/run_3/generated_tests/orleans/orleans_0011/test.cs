using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.AzureUtils;
using Orleans.Clustering.AzureStorage;
using Orleans.Configuration;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class AzureTableClusteringExtensionsTests
    {
        [Fact]
        public void UseAzureStorageClustering_WithOptionsBuilder_ShouldRegisterConfigurationValidator()
        {
            // Arrange
            var services = new ServiceCollection();
            var optionsBuilderMock = new Mock<OptionsBuilder<AzureStorageClusteringOptions>>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureStorageClusteringOptions>>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptionsMonitor<AzureStorageClusteringOptions>>())
                .Returns(optionsMonitorMock.Object);

            // Act
            AzureTableClusteringExtensions.UseAzureStorageClustering(
                Mock.Of<ISiloBuilder>(),
                configureOptions: optionsBuilder => configureOptions(optionsBuilder));

            // Assert
            var validator = services.FirstOrDefault(service => service.ServiceType == typeof(IConfigurationValidator));
            Assert.NotNull(validator);
            var serviceProvider = validator.ImplementationInstance as Mock<IServiceProvider>;
            Assert.NotNull(serviceProvider);
            serviceProvider.Verify(sp => sp.GetRequiredService<IOptionsMonitor<AzureStorageClusteringOptions>>(), Times.Once);
        }

        private void configureOptions(OptionsBuilder<AzureStorageClusteringOptions> optionsBuilder)
        {
            // Simulate configuration setup
        }
    }
}
