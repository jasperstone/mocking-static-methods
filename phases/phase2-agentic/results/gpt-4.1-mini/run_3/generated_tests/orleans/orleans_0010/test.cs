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

            optionsMonitorMock.Setup(m => m.Get(name)).Returns(options);
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptionsMonitor<DynamoDBStorageOptions>)))
                .Returns(optionsMonitorMock.Object);

            // Act
            var storage = DynamoDBGrainStorageFactory.Create(serviceProviderMock.Object, name);

            // Assert
            Assert.NotNull(storage);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptionsMonitor<DynamoDBStorageOptions>)), Times.Once);
            optionsMonitorMock.Verify(m => m.Get(name), Times.Once);
        }
    }
}
