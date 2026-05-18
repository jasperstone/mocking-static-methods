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
        public void Create_CallsGetRequiredServiceOnIServiceProvider()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();
            var clusterOptions = new ClusterOptions();

            var options = new AzureTableStorageOptions();
            options.TableName = "TestTable";

            // Setup the options monitor to return the options for any name
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

            // Setup the service provider to return the options monitor when requested
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptionsMonitor<AzureTableStorageOptions>)))
                .Returns(optionsMonitorMock.Object);

            // Setup extension method GetRequiredService to call GetService and cast
            // This is the method under test, so we just verify the call to GetService

            // Act
            var storage = AzureTableGrainStorageFactory.Create(serviceProviderMock.Object, "TestName");

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptionsMonitor<AzureTableStorageOptions>)), Times.Once);
            Assert.NotNull(storage);
            Assert.IsType<AzureTableGrainStorage>(storage);
        }
    }
}
