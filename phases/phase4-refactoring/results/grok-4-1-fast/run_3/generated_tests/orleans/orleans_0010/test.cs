using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Serialization;
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

            services.AddSingleton<IOptionsMonitor<DynamoDBStorageOptions>>(optionsMonitorMock.Object);
            services.AddSingleton<IActivatorProvider>(Mock.Of<IActivatorProvider>());
            services.AddSingleton<ILogger<DynamoDBGrainStorage>>(NullLogger<DynamoDBGrainStorage>.Instance);

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = DynamoDBGrainStorageFactory.Create(serviceProvider, "test-name");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<DynamoDBGrainStorage>(result);
        }

        [Fact]
        public void Create_MissingIOptionsMonitor_ThrowsInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<IActivatorProvider>(Mock.Of<IActivatorProvider>());
            services.AddSingleton<ILogger<DynamoDBGrainStorage>>(NullLogger<DynamoDBGrainStorage>.Instance);

            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                DynamoDBGrainStorageFactory.Create(serviceProvider, "test-name"));
            Assert.Contains("IOptionsMonitor<DynamoDBStorageOptions>", exception.Message);
        }

        [Fact]
        public void Create_ServiceProviderGetRequiredServiceThrows_PropagatesException()
        {
            // Arrange
            var mockProvider = new Mock<IServiceProvider>();
            mockProvider.Setup(p => p.GetService(typeof(IOptionsMonitor<DynamoDBStorageOptions>)))
                       .Returns((IOptionsMonitor<DynamoDBStorageOptions>)null);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                DynamoDBGrainStorageFactory.Create(mockProvider.Object, "test-name"));
        }
    }
}
