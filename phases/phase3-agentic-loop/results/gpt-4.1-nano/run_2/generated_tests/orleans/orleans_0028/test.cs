using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Orleans.Storage;
using Orleans.Providers.Azure;
using Orleans.Configuration;
using Orleans;
using Microsoft.Extensions.Options;

namespace Orleans.Tests
{
    public class AzureTableGrainStorageFactoryTests
    {
        [Fact]
        public void Create_ShouldResolveRequiredService()
        {
            // Arrange
            var servicesMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();
            var clusterOptions = new ClusterOptions { ClusterId = "cluster" };
            var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();
            clusterOptionsMock.SetupGet(c => c.Value).Returns(clusterOptions);

            var storageOptions = new AzureTableStorageOptions
            {
                TableName = "TestTable",
                GrainStorageSerializer = new Mock<IGrainStorageSerializer>().Object
            };

            var optionsMonitor = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();
            optionsMonitor.Setup(m => m.Get(It.IsAny<string>())).Returns(storageOptions);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddSingleton(optionsMonitor.Object);
            serviceCollection.AddSingleton(clusterOptionsMock.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            servicesMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<AzureTableStorageOptions>>())
                        .Returns(optionsMonitor.Object);
            servicesMock.Setup(sp => sp.GetProviderClusterOptions(It.IsAny<string>()))
                        .Returns(clusterOptions);

            // Act
            var storage = AzureTableGrainStorageFactory.Create(servicesMock.Object, "TestProvider");

            // Assert
            Assert.NotNull(storage);
        }
    }
}
