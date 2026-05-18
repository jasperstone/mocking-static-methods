using Xunit;
using Moq;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Persistence.DynamoDB;
using Orleans.Storage;

namespace Orleans.Tests
{
    public class DynamoDBGrainStorageFactoryTests
    {
        [Fact]
        public void Create_ShouldReturnDynamoDBGrainStorage_WhenServiceProviderReturnsOptionsMonitor()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            var options = new DynamoDBStorageOptions { ServiceId = "test", TableName = "testTable" };
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>())
                .Returns(optionsMonitorMock.Object);

            string storageName = "testName";

            // Act
            var storage = DynamoDBGrainStorageFactory.Create(serviceProviderMock.Object, storageName);

            // Assert
            Assert.NotNull(storage);
            Assert.IsType<DynamoDBGrainStorage>(storage);
        }
    }
}
