using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Serialization.Serializers;
using Orleans.Storage;
using Xunit;

namespace Orleans.Storage.Tests
{
    public class AzureTableGrainStorageFactoryTests
    {
        [Fact]
        public void Create_ThrowsInvalidOperationException_WhenIOptionsMonitorNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection().BuildServiceProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => AzureTableGrainStorageFactory.Create(services, "test"));
            Assert.Contains("IOptionsMonitor", exception.Message);
        }

        [Fact]
        public void Create_ThrowsInvalidOperationException_WhenClusterOptionsNotAvailable()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<AzureTableStorageOptions>()
                .Configure(o => o.TableName = "test-table");
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => AzureTableGrainStorageFactory.Create(serviceProvider, "test"));
            Assert.Contains("Unable to resolve service for type", exception.Message);
        }

        [Fact]
        public void Create_Succeeds_WhenAllRequiredServicesRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<ClusterOptions>(new ClusterOptions());
            services.AddSingleton<IGrainStorageSerializer>(Mock.Of<IGrainStorageSerializer>());
            services.AddSingleton<IActivatorProvider>(Mock.Of<IActivatorProvider>());
            services.AddOptions<AzureTableStorageOptions>()
                .Configure(o => o.TableName = "test-table");
            
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = AzureTableGrainStorageFactory.Create(serviceProvider, "test");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<AzureTableGrainStorage>(result);
        }

        [Fact]
        public void Create_UsesOptionsMonitorGet_WithProviderName()
        {
            // Arrange
            var mockOptionsMonitor = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();
            var options = new AzureTableStorageOptions { TableName = "test-table" };
            mockOptionsMonitor.Setup(m => m.Get("test")).Returns(options);

            var services = new ServiceCollection();
            services.AddSingleton(mockOptionsMonitor.Object);
            services.AddSingleton<ClusterOptions>(new ClusterOptions());
            services.AddSingleton<IGrainStorageSerializer>(Mock.Of<IGrainStorageSerializer>());
            services.AddSingleton<IActivatorProvider>(Mock.Of<IActivatorProvider>());
            services.AddLogging();
            
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = AzureTableGrainStorageFactory.Create(serviceProvider, "test");

            // Assert
            mockOptionsMonitor.Verify(m => m.Get("test"), Times.Once);
        }
    }
}
