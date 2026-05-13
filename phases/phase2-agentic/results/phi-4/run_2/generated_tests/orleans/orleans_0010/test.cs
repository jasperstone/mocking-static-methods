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
                ServiceId = "TestService",
                TableName = "TestTable",
                DeleteStateOnClear = true
            };
            mockOptions.Setup(o => o.Value).Returns(options);
            mockOptionsMonitor.Setup(m => m.Get(It.IsAny<string>())).Returns(options);
            mockServiceProvider.Setup(s => s.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>()).Returns(mockOptionsMonitor.Object);

            var mockLogger = new Mock<ILogger<DynamoDBGrainStorage>>();
            var mockActivatorProvider = new Mock<IActivatorProvider>();

            // Act
            var result = DynamoDBGrainStorageFactory.Create(mockServiceProvider.Object, "TestName");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<DynamoDBGrainStorage>(result);
        }
    }
}
