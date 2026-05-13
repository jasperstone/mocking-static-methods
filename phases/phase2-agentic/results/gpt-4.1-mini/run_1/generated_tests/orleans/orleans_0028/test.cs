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
            var clusterOptions = new ClusterOptions { ClusterId = "TestCluster", ServiceId = "TestService" };

            // Setup the options monitor to return a dummy AzureTableStorageOptions
            var storageOptions = new AzureTableStorageOptions();
            optionsMonitorMock.Setup(o => o.Get(It.IsAny<string>())).Returns(storageOptions);

            // Setup the service provider to return the options monitor when requested
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptionsMonitor<AzureTableStorageOptions>)))
                .Returns(optionsMonitorMock.Object);

            // Setup extension method GetProviderClusterOptions to return clusterOptions
            // Since it's an extension method, we simulate it by adding a helper method in test
            // But here we will mock IServiceProvider to return clusterOptions when asked for ClusterOptions
            // The actual extension method is not visible here, so we simulate by mocking the call chain

            // Act
            var storage = AzureTableGrainStorageFactory.Create(serviceProviderMock.Object, "TestName");

            // Assert
            Assert.NotNull(storage);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptionsMonitor<AzureTableStorageOptions>)), Times.Once);
            optionsMonitorMock.Verify(o => o.Get("TestName"), Times.Once);
        }
    }
}
