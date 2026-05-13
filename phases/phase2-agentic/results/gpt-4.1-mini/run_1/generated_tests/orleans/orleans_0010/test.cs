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
        public void Create_ShouldCallGetRequiredServiceAndReturnInstance()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            var options = new DynamoDBStorageOptions();
            var name = "TestStorage";

            // Setup GetRequiredService to return the mocked IOptionsMonitor
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptionsMonitor<DynamoDBStorageOptions>)))
                .Returns(optionsMonitorMock.Object);

            // Setup optionsMonitor.Get(name) to return the options instance
            optionsMonitorMock
                .Setup(monitor => monitor.Get(name))
                .Returns(options);

            // Act
            var storage = DynamoDBGrainStorage.DynamoDBGrainStorageFactory.Create(serviceProviderMock.Object, name);

            // Assert
            Assert.NotNull(storage);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptionsMonitor<DynamoDBStorageOptions>)), Times.Once);
            optionsMonitorMock.Verify(monitor => monitor.Get(name), Times.Once);
        }
    }
}
