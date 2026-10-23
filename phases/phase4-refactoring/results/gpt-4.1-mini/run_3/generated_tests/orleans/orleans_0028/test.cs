using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Storage;
using Orleans.Configuration;
using Xunit;

namespace Orleans.Persistence.AzureStorage.Tests
{
    public class AzureTableGrainStorageFactoryTests
    {
        [Fact]
        public void Create_ShouldCallGetRequiredServiceAndCreateInstance()
        {
            // Arrange
            var services = new ServiceCollection();

            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();
            var options = new AzureTableStorageOptions();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);
            services.AddSingleton(optionsMonitorMock.Object);

            var clusterOptions = new ClusterOptions();
            var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();
            clusterOptionsMock.SetupGet(c => c.Value).Returns(clusterOptions);
            services.AddSingleton(clusterOptionsMock.Object);

            // Add a logger for AzureTableGrainStorage to satisfy DI
            var loggerMock = new Mock<ILogger<AzureTableGrainStorage>>();
            services.AddSingleton(loggerMock.Object);

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var storage = AzureTableGrainStorageFactory.Create(serviceProvider, "TestName");

            // Assert
            Assert.NotNull(storage);
            Assert.IsType<AzureTableGrainStorage>(storage);
        }
    }
}
