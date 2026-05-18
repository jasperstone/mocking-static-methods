using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Orleans.Storage;
using Orleans.Providers.Azure;
using Microsoft.Extensions.Logging;

namespace Orleans.Tests
{
    public class AzureTableGrainStorageFactoryTests
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

            var optionsMonitor = optionsMonitorMock.Object;
            var optionsMock = new Mock<IOptions<AzureTableStorageOptions>>();
            var options = new AzureTableStorageOptions();
            optionsMock.SetupGet(o => o.Value).Returns(options);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(optionsMonitor);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            servicesMock.Setup(s => s.GetRequiredService<IOptionsMonitor<AzureTableStorageOptions>>())
                .Returns(optionsMonitor);

            // Act
            var storage = AzureTableGrainStorageFactory.Create(servicesMock.Object, "testProvider");

            // Assert
            servicesMock.Verify(s => s.GetRequiredService<IOptionsMonitor<AzureTableStorageOptions>>(), Times.Once);
        }
    }
}
