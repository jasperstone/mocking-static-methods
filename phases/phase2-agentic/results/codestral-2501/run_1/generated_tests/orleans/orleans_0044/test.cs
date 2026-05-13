using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Transactions.Abstractions;
using Orleans.Transactions.AzureStorage;
using Moq;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Providers;
using Orleans.Runtime;

namespace Orleans.Tests
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
            var optionsMock = new Mock<IOptions<AzureTableTransactionalStateOptions>>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptionsMonitor<AzureTableTransactionalStateOptions>))).Returns(optionsMonitorMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<AzureTableTransactionalStateOptions>))).Returns(optionsMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ITransactionalStateStorageFactory))).Returns(Mock.Of<ITransactionalStateStorageFactory>());
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILifecycleParticipant<ISiloLifecycle>))).Returns(Mock.Of<ILifecycleParticipant<ISiloLifecycle>>());

            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Act
            var result = serviceCollection.AddAzureTableTransactionalStateStorage("TestName");

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetService<IConfigurationValidator>());
            Assert.NotNull(serviceProvider.GetService<ITransactionalStateStorageFactory>());
            Assert.NotNull(serviceProvider.GetService<ILifecycleParticipant<ISiloLifecycle>>());
        }
    }
}
