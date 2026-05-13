using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Storage;
using Xunit;

namespace Orleans.Persistence.AzureStorage.Tests
{
    public class AzureTableGrainStorageFactoryTests
    {
        [Fact]
        public void Create_ShouldCallGetRequiredServiceAndCreateInstance()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();
            var clusterOptions = new ClusterOptions { ClusterId = "test-cluster" };

            var options = new AzureTableStorageOptions();
            options.TableName = "TestTable";

            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptionsMonitor<AzureTableStorageOptions>)))
                .Returns(optionsMonitorMock.Object);

            // Setup extension method GetRequiredService to return optionsMonitorMock.Object
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptionsMonitor<AzureTableStorageOptions>>())
                .Returns(optionsMonitorMock.Object);

            // Setup GetProviderClusterOptions extension method
            serviceProviderMock
                .Setup(sp => sp.GetProviderClusterOptions(It.IsAny<string>()))
                .Returns(clusterOptions);

            // Act
            var storage = AzureTableGrainStorageFactory.Create(serviceProviderMock.Object, "TestProvider");

            // Assert
            Assert.NotNull(storage);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<AzureTableStorageOptions>>(), Times.Once);
        }
    }

    // Extension methods to mock GetRequiredService and GetProviderClusterOptions
    internal static class ServiceProviderExtensions
    {
        public static T GetRequiredService<T>(this IServiceProvider provider)
        {
            return (T)provider.GetService(typeof(T))!;
        }

        public static ClusterOptions GetProviderClusterOptions(this IServiceProvider provider, string name)
        {
            // For test, return a default ClusterOptions
            return new ClusterOptions { ClusterId = "test-cluster" };
        }
    }
}
