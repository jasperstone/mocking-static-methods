using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Storage;
using Xunit;

namespace Orleans.Storage.Tests
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
            services.AddSingleton<object>(new object()); // Stand-in for IActivatorProvider
            services.AddSingleton<ILogger<DynamoDBGrainStorage>>(NullLogger<DynamoDBGrainStorage>.Instance);
            
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = DynamoDBGrainStorageFactory.Create(serviceProvider, "test");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<DynamoDBGrainStorage>(result);
        }

        [Fact]
        public void Create_WhenIOptionsMonitorMissing_ThrowsInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<object>(new object()); // Stand-in for IActivatorProvider
            services.AddSingleton<ILogger<DynamoDBGrainStorage>>(NullLogger<DynamoDBGrainStorage>.Instance);
            
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => DynamoDBGrainStorageFactory.Create(serviceProvider, "test"));
            Assert.Contains("IOptionsMonitor", exception.Message);
        }
    }
}
