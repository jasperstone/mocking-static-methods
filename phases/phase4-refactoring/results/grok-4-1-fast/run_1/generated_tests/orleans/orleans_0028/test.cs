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
    public class AzureTableGrainStorageFactoryTests
    {
        [Fact]
        public void Create_WhenCalledWithValidServices_ReturnsAzureTableGrainStorage()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new AzureTableStorageOptions { TableName = "TestTable" };
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get("test-provider")).Returns(options);

            services.AddSingleton<IOptionsMonitor<AzureTableStorageOptions>>(optionsMonitorMock.Object);
            services.AddSingleton<IOptions<ClusterOptions>>(Options.Create(new ClusterOptions()));
            services.AddSingleton<ILogger<AzureTableGrainStorage>>(NullLogger<AzureTableGrainStorage>.Instance);
            // Mock IActivatorProvider using object creation
            services.AddSingleton<IActivatorProvider>(new Mock<IActivatorProvider>().Object);

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = AzureTableGrainStorageFactory.Create(serviceProvider, "test-provider");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<AzureTableGrainStorage>(result);
        }

        [Fact]
        public void Create_WhenIOptionsMonitorMissing_ThrowsInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<IOptions<ClusterOptions>>(Options.Create(new ClusterOptions()));
            services.AddSingleton<ILogger<AzureTableGrainStorage>>(NullLogger<AzureTableGrainStorage>.Instance);
            services.AddSingleton<IActivatorProvider>(new Mock<IActivatorProvider>().Object);

            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => AzureTableGrainStorageFactory.Create(serviceProvider, "test-provider"));
            Assert.Contains("IOptionsMonitor<AzureTableStorageOptions>", exception.Message);
        }

        [Fact]
        public void Create_UsesCorrectOptionsFromMonitor()
        {
            // Arrange
            var expectedOptions = new AzureTableStorageOptions { TableName = "ExpectedTable" };
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get("test-provider")).Returns(expectedOptions);

            var services = new ServiceCollection();
            services.AddSingleton<IOptionsMonitor<AzureTableStorageOptions>>(optionsMonitorMock.Object);
            services.AddSingleton<IOptions<ClusterOptions>>(Options.Create(new ClusterOptions()));
            services.AddSingleton<ILogger<AzureTableGrainStorage>>(NullLogger<AzureTableGrainStorage>.Instance);
            services.AddSingleton<IActivatorProvider>(new Mock<IActivatorProvider>().Object);

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = AzureTableGrainStorageFactory.Create(serviceProvider, "test-provider");

            // Assert
            Assert.NotNull(result);
            optionsMonitorMock.Verify(m => m.Get("test-provider"), Times.Once);
        }
    }
}
