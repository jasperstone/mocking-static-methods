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
        public void Create_WhenServiceProviderMissingIOptionsMonitor_ThrowsInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(
                () => Orleans.Storage.DynamoDBGrainStorage.DynamoDBGrainStorageFactory.Create(serviceProvider, "test-name"));
            Assert.Contains("IOptionsMonitor<DynamoDBStorageOptions>", exception.Message);
        }

        [Fact]
        public void Create_WithValidServiceProvider_CallsGetRequiredServiceAndCreatesInstance()
        {
            // Arrange
            var mockOptionsMonitor = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            var expectedOptions = new DynamoDBStorageOptions { TableName = "test-table" };
            mockOptionsMonitor.Setup(m => m.Get("test-name")).Returns(expectedOptions);

            var services = new ServiceCollection();
            services.AddSingleton(mockOptionsMonitor.Object);
            services.AddSingleton<ILogger<DynamoDBGrainStorage>>(NullLogger<DynamoDBGrainStorage>.Instance);
            
            // Add required dependencies that don't require IActivatorProvider
            var mockActivatorProvider = new Mock<object>();
            services.AddSingleton(mockActivatorProvider.Object);
            
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var storage = Orleans.Storage.DynamoDBGrainStorage.DynamoDBGrainStorageFactory.Create(serviceProvider, "test-name");

            // Assert
            Assert.NotNull(storage);
            mockOptionsMonitor.Verify(m => m.Get("test-name"), Times.Once);
        }

        [Fact]
        public void Create_WhenOptionsMonitorReturnsNullOptions_CreatesInstance()
        {
            // Arrange
            var mockOptionsMonitor = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            mockOptionsMonitor.Setup(m => m.Get("test-name")).Returns<DynamoDBStorageOptions>(null);

            var services = new ServiceCollection();
            services.AddSingleton(mockOptionsMonitor.Object);
            services.AddSingleton<ILogger<DynamoDBGrainStorage>>(NullLogger<DynamoDBGrainStorage>.Instance);
            services.AddSingleton(new object()); // placeholder for activator provider
            
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var storage = Orleans.Storage.DynamoDBGrainStorage.DynamoDBGrainStorageFactory.Create(serviceProvider, "test-name");

            // Assert
            Assert.NotNull(storage);
            mockOptionsMonitor.Verify(m => m.Get("test-name"), Times.Once);
        }
    }
}
