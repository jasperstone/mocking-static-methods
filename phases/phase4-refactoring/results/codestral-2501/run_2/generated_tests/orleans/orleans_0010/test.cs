using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Storage;
using Orleans.Serialization.Serializers;
using Microsoft.Extensions.Logging;
using System;

namespace Orleans.Persistence.DynamoDB.Tests
{
    public class DynamoDBGrainStorageFactoryTests
    {
        [Fact]
        public void Create_ShouldReturnDynamoDBGrainStorage()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            var activatorProviderMock = new Mock<IActivatorProvider>();
            var loggerMock = new Mock<ILogger<DynamoDBGrainStorage>>();
            var options = new DynamoDBStorageOptions();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptionsMonitor<DynamoDBStorageOptions>)))
                .Returns(optionsMonitorMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IActivatorProvider)))
                .Returns(activatorProviderMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILogger<DynamoDBGrainStorage>)))
                .Returns(loggerMock.Object);

            // Act
            var result = DynamoDBGrainStorageFactory.Create(serviceProviderMock.Object, "test");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<DynamoDBGrainStorage>(result);
        }
    }
}
