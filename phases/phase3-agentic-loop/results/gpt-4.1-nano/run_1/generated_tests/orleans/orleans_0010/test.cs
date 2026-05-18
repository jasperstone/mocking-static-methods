using System;
using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Persistence.DynamoDB;
using Orleans.Runtime;
using Microsoft.Extensions.Logging;

namespace Orleans.Tests
{
    public class DynamoDBGrainStorageFactoryTests
    {
        [Fact]
        public void Create_Should_Call_GetRequiredService_For_IOptionsMonitor()
        {
            // Arrange
            var servicesMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            var options = new DynamoDBStorageOptions();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

            servicesMock.Setup(s => s.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>())
                        .Returns(optionsMonitorMock.Object);

            string storageName = "testStorage";

            // Act
            var storage = DynamoDBGrainStorageFactory.Create(servicesMock.Object, storageName);

            // Assert
            servicesMock.Verify(s => s.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>(), Times.Once);
            Assert.NotNull(storage);
        }
    }
}
