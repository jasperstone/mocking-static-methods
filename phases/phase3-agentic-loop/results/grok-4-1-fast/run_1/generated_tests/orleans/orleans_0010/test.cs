using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Storage;
using Orleans.Persistence.DynamoDB;
using Xunit;

namespace Orleans.Storage.Tests
{
    public class DynamoDBGrainStorageFactoryTests
    {
        [Fact]
        public void Create_ThrowsArgumentNullException_WhenServicesNull()
        {
            // Arrange
            IServiceProvider services = null;
            string name = "test";

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => DynamoDBGrainStorageFactory.Create(services, name));
            Assert.Equal("services", exception.ParamName);
        }

        [Fact]
        public void Create_ThrowsArgumentNullException_WhenNameNull()
        {
            // Arrange
            var services = new ServiceCollection().BuildServiceProvider();
            string name = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => DynamoDBGrainStorageFactory.Create(services, name));
            Assert.Equal("name", exception.ParamName);
        }

        [Fact]
        public void Create_ThrowsInvalidOperationException_WhenIOptionsMonitorNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                DynamoDBGrainStorageFactory.Create(services, "test"));
            
            Assert.Contains("Unable to resolve service for type", exception.Message);
            Assert.Contains("IOptionsMonitor<DynamoDBStorageOptions>", exception.Message);
        }

        [Fact]
        public void Create_CallsGetRequiredService_Successfully()
        {
            // Arrange
            var mockOptionsMonitor = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            mockOptionsMonitor.Setup(m => m.Get(It.IsAny<string>())).Returns(new DynamoDBStorageOptions());

            var services = new ServiceCollection()
                .AddSingleton(mockOptionsMonitor.Object)
                .AddSingleton<Orleans.Serialization.Serializers.IActivatorProvider>(Mock.Of<Orleans.Serialization.Serializers.IActivatorProvider>())
                .AddSingleton<ILogger<DynamoDBGrainStorage>>(NullLogger<DynamoDBGrainStorage>.Instance)
                .BuildServiceProvider();

            // Act
            var result = DynamoDBGrainStorageFactory.Create(services, "test");

            // Assert
            Assert.NotNull(result);
            mockOptionsMonitor.Verify(m => m.Get("test"), Times.Once);
        }

        [Fact]
        public void Create_CallsOptionsMonitorGet_WithCorrectName()
        {
            // Arrange
            const string name = "test-storage";
            var options = new DynamoDBStorageOptions();
            var mockOptionsMonitor = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            mockOptionsMonitor.Setup(m => m.Get(name)).Returns(options);

            var services = new ServiceCollection()
                .AddSingleton(mockOptionsMonitor.Object)
                .AddSingleton<Orleans.Serialization.Serializers.IActivatorProvider>(Mock.Of<Orleans.Serialization.Serializers.IActivatorProvider>())
                .AddSingleton<ILogger<DynamoDBGrainStorage>>(NullLogger<DynamoDBGrainStorage>.Instance)
                .BuildServiceProvider();

            // Act
            var result = DynamoDBGrainStorageFactory.Create(services, name);

            // Assert
            mockOptionsMonitor.Verify(m => m.Get(name), Times.Once);
            Assert.NotNull(result);
        }

        [Fact]
        public void Create_SuccessfullyCreatesDynamoDBGrainStorageInstance()
        {
            // Arrange
            var options = new DynamoDBStorageOptions { TableName = "test-table" };
            var mockOptionsMonitor = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            mockOptionsMonitor.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

            var services = new ServiceCollection()
                .AddSingleton(mockOptionsMonitor.Object)
                .AddSingleton<Orleans.Serialization.Serializers.IActivatorProvider>(Mock.Of<Orleans.Serialization.Serializers.IActivatorProvider>())
                .AddSingleton<ILogger<DynamoDBGrainStorage>>(NullLogger<DynamoDBGrainStorage>.Instance)
                .BuildServiceProvider();

            // Act
            var result = DynamoDBGrainStorageFactory.Create(services, "test");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<DynamoDBGrainStorage>(result);
        }
    }
}
