using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Persistence.DynamoDB;
using Orleans.Runtime;
using Orleans.Serialization.Serializers;
using Orleans.Storage;
using Xunit;

namespace Orleans.Persistence.DynamoDB.Tests
{
    public class DynamoDBGrainStorageTests
    {
        [Fact]
        public void Create_ShouldCallGetRequiredService()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            var optionsMock = new Mock<DynamoDBStorageOptions>();
            var activatorProviderMock = new Mock<IActivatorProvider>();
            var loggerMock = new Mock<ILogger<DynamoDBGrainStorage>>();

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
                .Returns(optionsMock.Object);

            // Act
            var result = DynamoDBGrainStorageFactory.Create(serviceProviderMock.Object, "test");

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptionsMonitor<DynamoDBStorageOptions>)), Times.Once);
            optionsMonitorMock.Verify(om => om.Get("test"), Times.Once);
            Assert.NotNull(result);
        }
    }
}
