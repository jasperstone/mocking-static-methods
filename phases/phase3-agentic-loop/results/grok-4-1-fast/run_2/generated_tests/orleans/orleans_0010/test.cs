using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Storage;
using Xunit;

namespace Orleans.Persistence.DynamoDB.Provider.Tests
{
    public class DynamoDBGrainStorageFactoryTests
    {
        [Fact]
        public void Create_WhenCalledWithValidServices_ReturnsInstance()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new DynamoDBStorageOptions();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);
            services.AddSingleton<IOptionsMonitor<DynamoDBStorageOptions>>(optionsMonitorMock.Object);
            
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = DynamoDBGrainStorage.Create(serviceProvider, "testName");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void Create_WhenIOptionsMonitorMissing_ThrowsInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => DynamoDBGrainStorage.Create(serviceProvider, "testName"));
            Assert.Contains("IOptionsMonitor", ex.Message);
            Assert.Contains("DynamoDBStorageOptions", ex.Message);
        }
    }
}
