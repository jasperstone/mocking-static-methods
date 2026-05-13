using Xunit;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Providers.Azure;
using Orleans.Storage;
using Microsoft.Extensions.Logging;
using Moq;

namespace Orleans.Tests
{
    public class AzureTableStorageFactoryTests
    {
        [Fact]
        public void Create_Should_Call_GetRequiredService_ForOptionsMonitor()
        {
            // Arrange
            var servicesMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();
            var clusterOptions = new ClusterOptions { ClusterId = "cluster" };
            var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();
            clusterOptionsMock.SetupGet(c => c.Value).Returns(clusterOptions);

            var optionsMonitorInstance = optionsMonitorMock.Object;
            var serviceCollection = new ServiceCollection();

            // Setup the service provider to return optionsMonitor when requested
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IOptionsMonitor<AzureTableStorageOptions>)))
                .Returns(optionsMonitorInstance);

            // Act
            var result = AzureTableGrainStorageFactory.Create(serviceProviderMock.Object, "testProvider");

            // Assert
            Assert.NotNull(result);
            optionsMonitorMock.VerifyGet(o => o.Get(It.IsAny<string>()), Times.Once);
        }
    }
}
