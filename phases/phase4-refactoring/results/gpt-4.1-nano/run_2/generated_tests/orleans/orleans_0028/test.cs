using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Orleans.Storage;
using Orleans.Configuration;

namespace Orleans.Tests
{
    public class AzureTableStorageFactoryTests
    {
        [Fact]
        public void Create_Should_Call_GetService_ForOptionsMonitors()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorAzureMock = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();
            var optionsMonitorClusterMock = new Mock<IOptionsMonitor<ClusterOptions>>();

            // Setup GetService to return the mock options monitors for specific types
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptionsMonitor<AzureTableStorageOptions>)))
                .Returns(optionsMonitorAzureMock.Object);
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptionsMonitor<ClusterOptions>)))
                .Returns(optionsMonitorClusterMock.Object);

            // Setup the Get method on the cluster options monitor
            var clusterOptions = new ClusterOptions();
            optionsMonitorClusterMock
                .Setup(om => om.Get(It.IsAny<string>()))
                .Returns(clusterOptions);

            // Setup the Get method on the Azure options monitor
            var azureOptions = new AzureTableStorageOptions();
            optionsMonitorAzureMock
                .Setup(om => om.Get(It.IsAny<string>()))
                .Returns(azureOptions);

            // Act
            var storage = Orleans.Storage.AzureTableGrainStorageFactory.Create(serviceProviderMock.Object, "TestProvider");

            // Assert
            // Verify that GetService was called for both options monitors
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptionsMonitor<AzureTableStorageOptions>)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptionsMonitor<ClusterOptions>)), Times.Once);
            Assert.NotNull(storage);
        }
    }
}
