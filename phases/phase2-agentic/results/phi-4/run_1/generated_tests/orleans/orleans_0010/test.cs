using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Orleans.Persistence.DynamoDB.Tests
{
    public class DynamoDBGrainStorageFactoryTests
    {
        [Fact]
        public void Create_ShouldReturnDynamoDBGrainStorage_WhenCalledWithValidServices()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockOptionsMonitor = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            var mockOptions = new Mock<IOptions<DynamoDBStorageOptions>>();
            var options = new DynamoDBStorageOptions
            {
                // Set necessary properties for the test
                ServiceId = "TestServiceId",
                TableName = "TestTableName",
                DeleteStateOnClear = false
            };
            mockOptions.Setup(o => o.Value).Returns(options);
            mockOptionsMonitor.Setup(m => m.Get(It.IsAny<string>())).Returns(options);
            mockServiceProvider.Setup(s => s.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>()).Returns(mockOptionsMonitor.Object);

            var factory = new DynamoDBGrainStorageFactory();

            // Act
            var result = factory.Create(mockServiceProvider.Object, "TestName");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<DynamoDBGrainStorage>(result);
        }
    }
}
