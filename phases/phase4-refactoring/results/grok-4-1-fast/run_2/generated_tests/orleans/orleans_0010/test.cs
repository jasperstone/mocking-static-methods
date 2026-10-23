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
        public void Create_ValidServiceProviderAndName_ReturnsDynamoDBGrainStorage()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new DynamoDBStorageOptions { TableName = "test-table" };
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get("test-name")).Returns(options);

            services.AddSingleton(optionsMonitorMock.Object);
            // Create mock object without type reference
            var activatorProviderMock = new Mock<object>().Object;
            services.AddSingleton(activatorProviderMock);
            services.AddSingleton<ILogger<DynamoDBGrainStorage>>(NullLogger<DynamoDBGrainStorage>.Instance);

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var storage = DynamoDBGrainStorageFactory.Create(serviceProvider, "test-name");

            // Assert
            Assert.NotNull(storage);
            Assert.IsType<DynamoDBGrainStorage>(storage);
        }

        [Fact]
        public void Create_MissingIOptionsMonitor_ThrowsInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            var activatorProviderMock = new Mock<object>().Object;
            services.AddSingleton(activatorProviderMock);
            services.AddSingleton<ILogger<DynamoDBGrainStorage>>(NullLogger<DynamoDBGrainStorage>.Instance);
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => 
                DynamoDBGrainStorageFactory.Create(serviceProvider, "test-name"));
            Assert.Contains("IOptionsMonitor<DynamoDBStorageOptions>", ex.Message);
        }

        [Fact]
        public void Create_ServiceProviderGetRequiredServiceThrows_PropagatesInvalidOperationException()
        {
            // Arrange - ServiceProvider that throws on GetRequiredService
            var mockProvider = new Mock<IServiceProvider>();
            mockProvider.Setup(p => p.GetService(typeof(IOptionsMonitor<DynamoDBStorageOptions>)))
                       .Returns((IOptionsMonitor<DynamoDBStorageOptions>)null);
            mockProvider.Setup(p => p.GetRequiredService(typeof(IOptionsMonitor<DynamoDBStorageOptions>)))
                       .Throws(new InvalidOperationException("Service not registered."));

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => 
                DynamoDBGrainStorageFactory.Create(mockProvider.Object, "test-name"));
            Assert.Equal("Service not registered.", ex.Message);
        }

        [Fact]
        public void Create_OptionsMonitorReturnsNullOptions_ThrowsInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get("test-name")).Returns((DynamoDBStorageOptions)null);

            services.AddSingleton(optionsMonitorMock.Object);
            var activatorProviderMock = new Mock<object>().Object;
            services.AddSingleton(activatorProviderMock);
            services.AddSingleton<ILogger<DynamoDBGrainStorage>>(NullLogger<DynamoDBGrainStorage>.Instance);

            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => 
                DynamoDBGrainStorageFactory.Create(serviceProvider, "test-name"));
            Assert.Contains("Unable to resolve service for type", ex.Message);
        }
    }
}
