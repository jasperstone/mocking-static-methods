using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Storage;
using Orleans.Serialization.Serializers;
using Xunit;

namespace Orleans.Persistence.DynamoDB.Tests
{
    public class DynamoDBGrainStorageFactoryTests
    {
        [Fact]
        public void Create_ShouldReturnDynamoDBGrainStorage_WhenServicesAreProvided()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            var activatorProviderMock = new Mock<IActivatorProvider>();
            var loggerMock = new Mock<ILogger<DynamoDBGrainStorage>>();
            var options = new DynamoDBStorageOptions();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptionsMonitor<DynamoDBStorageOptions>)))
                .Returns(optionsMonitorMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IActivatorProvider)))
                .Returns(activatorProviderMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ILogger<DynamoDBGrainStorage>)))
                .Returns(loggerMock.Object);

            optionsMonitorMock
                .Setup(om => om.Get(It.IsAny<string>()))
                .Returns(options);

            // Act
            var result = DynamoDBGrainStorageFactory.Create(serviceProviderMock.Object, "TestName");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<DynamoDBGrainStorage>(result);
        }

        [Fact]
        public void Create_ShouldThrowInvalidOperationException_WhenOptionsMonitorIsNotRegistered()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptionsMonitor<DynamoDBStorageOptions>)))
                .Returns((IOptionsMonitor<DynamoDBStorageOptions>)null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => DynamoDBGrainStorageFactory.Create(serviceProviderMock.Object, "TestName"));
        }
    }
}
