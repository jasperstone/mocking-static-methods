using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Transactions.Abstractions;
using Orleans.Transactions.AzureStorage;
using Orleans.Hosting;

namespace Orleans.Tests
{
    public class AzureTableTransactionServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureTableTransactionalStateStorage_ShouldAddRequiredServices()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableTransactionalStateOptions>>();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(new AzureTableTransactionalStateOptions());

            serviceCollection.AddSingleton(optionsMonitorMock.Object);

            // Act
            serviceCollection.AddAzureTableTransactionalStateStorage("TestStorage");

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetService<IConfigurationValidator>());
            Assert.NotNull(serviceProvider.GetService<ITransactionalStateStorageFactory>());
            Assert.NotNull(serviceProvider.GetKeyedService<ITransactionalStateStorageFactory>("TestStorage"));
        }
    }
}
