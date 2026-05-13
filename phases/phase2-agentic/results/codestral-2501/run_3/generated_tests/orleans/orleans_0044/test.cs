using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Transactions.Abstractions;
using Orleans.Transactions.AzureStorage;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Runtime;

namespace Orleans.Tests
{
    public class AzureTableTransactionServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureTableTransactionalStateStorage_ShouldRegisterServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableTransactionalStateOptions>>();
            services.AddSingleton(optionsMonitorMock.Object);

            // Act
            services.AddAzureTableTransactionalStateStorage("TestStorage");

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetService<IConfigurationValidator>());
            Assert.NotNull(serviceProvider.GetService<ITransactionalStateStorageFactory>());
            Assert.NotNull(serviceProvider.GetKeyedService<ITransactionalStateStorageFactory>("TestStorage"));
            Assert.NotNull(serviceProvider.GetService<ILifecycleParticipant<ISiloLifecycle>>());
        }

        [Fact]
        public void AddAzureTableTransactionalStateStorage_ShouldCallGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableTransactionalStateOptions>>();
            services.AddSingleton(optionsMonitorMock.Object);

            // Act
            services.AddAzureTableTransactionalStateStorage("TestStorage");

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var configurationValidator = serviceProvider.GetService<IConfigurationValidator>();
            optionsMonitorMock.Verify(m => m.Get("TestStorage"), Times.Once);
        }
    }
}
