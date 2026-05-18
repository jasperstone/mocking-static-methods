using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Storage;
using Xunit;

namespace Orleans.Storage.Tests
{
    public class AzureTableGrainStorageFactoryTests
    {
        [Fact]
        public void Create_CallsGetRequiredServiceAndReturnsInstance()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();
            var clusterOptions = new ClusterOptions();
            var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();
            clusterOptionsMock.SetupGet(c => c.Value).Returns(clusterOptions);

            var azureTableStorageOptions = new AzureTableStorageOptions();
            optionsMonitorMock.Setup(o => o.Get(It.IsAny<string>())).Returns(azureTableStorageOptions);

            // Setup GetRequiredService to return the mocked IOptionsMonitor<AzureTableStorageOptions>
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptionsMonitor<AzureTableStorageOptions>)))
                .Returns(optionsMonitorMock.Object);

            // Setup extension method GetRequiredService to call GetService and throw if null
            // We simulate this by setting up the serviceProviderMock to return the optionsMonitorMock

            // Setup GetProviderClusterOptions extension method
            serviceProviderMock
                .Setup(sp => sp.GetProviderClusterOptions(It.IsAny<string>()))
                .Returns(clusterOptions);

            // Act
            var storage = AzureTableGrainStorageFactory.Create(serviceProviderMock.Object, "TestName");

            // Assert
            Assert.NotNull(storage);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptionsMonitor<AzureTableStorageOptions>)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetProviderClusterOptions("TestName"), Times.Once);
        }
    }

    // Extension method mock for GetProviderClusterOptions
    internal static class ServiceProviderExtensions
    {
        public static ClusterOptions GetProviderClusterOptions(this IServiceProvider services, string name)
        {
            // This is a stub for the extension method used in the factory
            return services.GetService(typeof(IOptions<ClusterOptions>)) is IOptions<ClusterOptions> options
                ? options.Value
                : new ClusterOptions();
        }
    }
}
