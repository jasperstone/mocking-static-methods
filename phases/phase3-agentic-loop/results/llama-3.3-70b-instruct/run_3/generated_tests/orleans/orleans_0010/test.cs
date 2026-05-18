using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Persistence.DynamoDB;

namespace Orleans.Storage.Tests
{
    public class DynamoDBGrainStorageFactoryTests
    {
        [Fact]
        public void Create_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var optionsMonitor = new Mock<Microsoft.Extensions.Options.IOptionsMonitor<Orleans.Persistence.DynamoDB.DynamoDBStorageOptions>>();
            services.AddSingleton(optionsMonitor.Object);
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var storage = DynamoDBGrainStorageFactory.Create(serviceProvider, "test");

            // Assert
            optionsMonitor.Verify(m => m.Get("test"), Times.Once);
        }

        [Fact]
        public void Create_ReturnsDynamoDBGrainStorageInstance()
        {
            // Arrange
            var services = new ServiceCollection();
            var optionsMonitor = new Mock<Microsoft.Extensions.Options.IOptionsMonitor<Orleans.Persistence.DynamoDB.DynamoDBStorageOptions>>();
            services.AddSingleton(optionsMonitor.Object);
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var storage = DynamoDBGrainStorageFactory.Create(serviceProvider, "test");

            // Assert
            Assert.IsType<DynamoDBGrainStorage>(storage);
        }
    }
}
