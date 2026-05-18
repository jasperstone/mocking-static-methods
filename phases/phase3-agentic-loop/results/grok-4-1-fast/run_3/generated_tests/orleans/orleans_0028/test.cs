using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Runtime;
using Xunit;

namespace Orleans.Storage
{
    public class AzureTableGrainStorageFactoryTests
    {
        [Fact]
        public void Create_CallsGetRequiredServiceOnIServiceProvider()
        {
            // Arrange
            var services = new Mock<IServiceProvider>();
            var optionsSnapshotMock = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();
            optionsSnapshotMock.Setup(m => m.Get(It.IsAny<string>())).Returns(new AzureTableStorageOptions());
            services.Setup(s => s.GetService(typeof(IOptionsMonitor<AzureTableStorageOptions>))).Returns(optionsSnapshotMock.Object);
            services.Setup(s => s.GetService(typeof(ClusterOptions))).Returns(new ClusterOptions());
            services.Setup(s => s.GetService(typeof(IActivatorProvider))).Returns(Mock.Of<IActivatorProvider>());

            // Act
            _ = AzureTableGrainStorageFactory.Create(services.Object, "test");

            // Assert - verify the GetService call was made
            services.Verify(s => s.GetService(typeof(IOptionsMonitor<AzureTableStorageOptions>)), Times.Once());
        }

        [Fact]
        public void Create_ThrowsInvalidOperationException_WhenOptionsMonitorNotRegistered()
        {
            // Arrange
            var services = new Mock<IServiceProvider>();
            services.Setup(s => s.GetService(typeof(IOptionsMonitor<AzureTableStorageOptions>))).Returns((object?)null);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => AzureTableGrainStorageFactory.Create(services.Object, "test"));
            Assert.Contains("IOptionsMonitor<AzureTableStorageOptions>", exception.Message);
        }

        [Fact]
        public void Create_SucceedsWithValidServices()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<IOptionsMonitor<AzureTableStorageOptions>>(provider =>
            {
                var mock = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();
                mock.Setup(m => m.Get(It.IsAny<string>())).Returns(new AzureTableStorageOptions());
                return mock.Object;
            });
            services.AddSingleton(provider => new ClusterOptions());
            services.AddSingleton<IActivatorProvider>(Mock.Of<IActivatorProvider>());
            services.AddSingleton(provider => (IOptions<ClusterOptions>)Options.Create(new ClusterOptions()));
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = AzureTableGrainStorageFactory.Create(serviceProvider, "test");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<AzureTableGrainStorage>(result);
        }
    }
}
