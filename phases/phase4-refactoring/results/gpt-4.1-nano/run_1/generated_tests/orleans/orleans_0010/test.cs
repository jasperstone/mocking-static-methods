using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Orleans.Storage;
using Orleans.Runtime;

namespace Orleans.Tests
{
    public class DynamoDBGrainStorageFactoryTests
    {
        [Fact]
        public void Create_ShouldCallGetRequiredServiceAndReturnDynamoDBGrainStorage()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            var storageOptions = new DynamoDBStorageOptions
            {
                ServiceId = "test-service",
                TableName = "test-table"
            };
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(storageOptions);

            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>())
                .Returns(optionsMonitorMock.Object);

            var loggerMock = new Mock<ILogger<DynamoDBGrainStorage>>();

            // Act
            var storage = DynamoDBGrainStorageFactory.Create(serviceProviderMock.Object, "default");

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>(), Times.Once);
            Assert.NotNull(storage);
            Assert.IsType<DynamoDBGrainStorage>(storage);
        }
    }
}
