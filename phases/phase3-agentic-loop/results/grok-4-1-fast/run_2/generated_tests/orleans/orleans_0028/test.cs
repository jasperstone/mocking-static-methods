using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Xunit;

namespace Orleans.Storage
{
    public class AzureTableGrainStorageFactoryTests
    {
        [Fact]
        public void Create_ThrowsWhenIOptionsMonitorNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ClusterOptions>(new ClusterOptions());
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => AzureTableGrainStorageFactory.Create(serviceProvider, "testName"));
            Assert.Contains("IOptionsMonitor<AzureTableStorageOptions>", exception.Message);
        }

        [Fact]
        public void Create_SucceedsWithRequiredServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get("testName")).Returns(new AzureTableStorageOptions());
            services.AddSingleton<IOptionsMonitor<AzureTableStorageOptions>>(optionsMonitorMock.Object);
            services.AddSingleton<ClusterOptions>(new ClusterOptions());
            services.AddSingleton<ILogger<AzureTableGrainStorage>>(Mock.Of<ILogger<AzureTableGrainStorage>>());
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = AzureTableGrainStorageFactory.Create(serviceProvider, "testName");

            // Assert
            Assert.NotNull(result);
            optionsMonitorMock.Verify(m => m.Get("testName"), Times.Once);
        }

        [Fact]
        public void Create_CallsGetWithCorrectNameOnIOptionsMonitor()
        {
            // Arrange
            var name = "testProvider";
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get(name)).Returns(new AzureTableStorageOptions());
            var services = new ServiceCollection();
            services.AddSingleton<IOptionsMonitor<AzureTableStorageOptions>>(optionsMonitorMock.Object);
            services.AddSingleton<ClusterOptions>(new ClusterOptions());
            services.AddSingleton<ILogger<AzureTableGrainStorage>>(Mock.Of<ILogger<AzureTableGrainStorage>>());
            var serviceProvider = services.BuildServiceProvider();

            // Act
            _ = AzureTableGrainStorageFactory.Create(serviceProvider, name);

            // Assert
            optionsMonitorMock.Verify(m => m.Get(name), Times.Once);
        }
    }
}
