using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Transactions.Abstractions;
using Orleans.Transactions.AzureStorage;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Runtime;
using Moq;
using Microsoft.Extensions.Options;
using Orleans.Configuration;

namespace Orleans.Hosting.Tests
{
    public class AzureTableTransactionServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureTableTransactionalStateStorage_ShouldRegisterServices()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableTransactionalStateOptions>>();
            var transactionalStateStorageFactoryMock = new Mock<ITransactionalStateStorageFactory>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptionsMonitor<AzureTableTransactionalStateOptions>)))
                .Returns(optionsMonitorMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ITransactionalStateStorageFactory)))
                .Returns(transactionalStateStorageFactoryMock.Object);

            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Act
            serviceCollection.AddAzureTableTransactionalStateStorage("TestStorage");

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var configurationValidator = serviceProvider.GetRequiredService<IConfigurationValidator>();
            var transactionalStateStorageFactory = serviceProvider.GetRequiredService<ITransactionalStateStorageFactory>();
            var lifecycleParticipant = serviceProvider.GetRequiredService<ILifecycleParticipant<ISiloLifecycle>>();

            Assert.NotNull(configurationValidator);
            Assert.NotNull(transactionalStateStorageFactory);
            Assert.NotNull(lifecycleParticipant);
        }
    }
}
