using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection.Extensions; // For Verify
using Microsoft.Extensions.Options; // For IOptionsMonitor
using Microsoft.Extensions.DependencyInjection; // For ServiceCollection
using Microsoft.Extensions.DependencyInjection; // For ISiloBuilder
using Microsoft.Extensions.DependencyInjection; // For AzureTableClusteringExtensions
using Microsoft.Extensions.Options; // For IConfigurationValidator

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class AzureTableClusteringExtensionsTests
    {
        [Fact]
        public void UseAzureStorageClustering_CallsGetRequiredServiceCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureStorageClusteringOptions>>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptionsMonitor<AzureStorageClusteringOptions>>())
                .Returns(optionsMonitorMock.Object);

            var builderMock = new Mock<ISiloBuilder>();
            builderMock
                .Setup(b => b.ConfigureServices(It.IsAny<Action<ServiceCollection>>()))
                .Callback<Action<ServiceCollection>>(services =>
                {
                    // Act
                    AzureTableClusteringExtensions.UseAzureStorageClustering(builderMock.Object, null);
                });

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<AzureStorageClusteringOptions>>(), Times.Once);

            // Verify that the AzureStorageClusteringOptionsValidator is added with the correct parameters
            var validatorMock = new Mock<IConfigurationValidator>();
            services.Verify(s => s.AddTransient(It.Is<Mock<IConfigurationValidator>.Object>(m => 
                m.Object is AzureStorageClusteringOptionsValidator &&
                m.Object.GetRequiredService<IOptionsMonitor<AzureStorageClusteringOptions>>().Object == optionsMonitorMock.Object)), Times.Once);
        }
    }
}
