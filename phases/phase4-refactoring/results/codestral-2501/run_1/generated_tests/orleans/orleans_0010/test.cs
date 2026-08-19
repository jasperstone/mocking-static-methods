using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
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
            var optionsMock = new Mock<DynamoDBStorageOptions>();
            var loggerMock = new Mock<ILogger<DynamoDBGrainStorage>>();
            var activatorProviderMock = new Mock<IActivatorProvider>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>())
                .Returns(optionsMonitorMock.Object);

            optionsMonitorMock
                .Setup(om => om.Get(It.IsAny<string>()))
                .Returns(optionsMock.Object);

            // Act
            var result = DynamoDBGrainStorageFactory.Create(serviceProviderMock.Object, "testName");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<DynamoDBGrainStorage>(result);
        }
    }
}
