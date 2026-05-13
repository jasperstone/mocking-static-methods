using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Storage;
using Xunit;

namespace Orleans.Persistence.DynamoDB.Tests
{
    public class DynamoDBGrainStorageFactoryTests
    {
        [Fact]
        public void Create_ShouldReturnDynamoDBGrainStorageInstance()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            var options = new DynamoDBStorageOptions();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptionsMonitor<DynamoDBStorageOptions>)))
                .Returns(optionsMonitorMock.Object);

            // Act
            var result = DynamoDBGrainStorageFactory.Create(serviceProviderMock.Object, "test");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<DynamoDBGrainStorage>(result);
        }

        [Fact]
        public void Create_ShouldThrowInvalidOperationException_WhenOptionsMonitorNotRegistered()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptionsMonitor<DynamoDBStorageOptions>)))
                .Returns((IOptionsMonitor<DynamoDBStorageOptions>)null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => DynamoDBGrainStorageFactory.Create(serviceProviderMock.Object, "test"));
        }
    }
}
