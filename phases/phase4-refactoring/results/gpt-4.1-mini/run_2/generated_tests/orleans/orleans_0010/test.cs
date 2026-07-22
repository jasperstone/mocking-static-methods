using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Storage;
using Xunit;
using Moq;

namespace Orleans.Persistence.DynamoDB.Tests
{
    public class DynamoDBGrainStorageFactoryTests
    {
        [Fact]
        public void Create_ShouldCallGetRequiredServiceOnIServiceProvider_AndReturnInstance()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            var activatorProviderMock = new Mock<IActivatorProvider>();
            var loggerMock = new Mock<ILogger<DynamoDBGrainStorage>>();
            var options = new DynamoDBStorageOptions();
            var name = "TestStorage";

            optionsMonitorMock.Setup(m => m.Get(name)).Returns(options);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptionsMonitor<DynamoDBStorageOptions>)))
                .Returns(optionsMonitorMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IActivatorProvider)))
                .Returns(activatorProviderMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILogger<DynamoDBGrainStorage>)))
                .Returns(loggerMock.Object);

            // Act
            var storage = DynamoDBGrainStorageFactory.Create(serviceProviderMock.Object, name);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>(), Times.Once);
            Assert.NotNull(storage);
            Assert.IsType<DynamoDBGrainStorage>(storage);
        }
    }
}
