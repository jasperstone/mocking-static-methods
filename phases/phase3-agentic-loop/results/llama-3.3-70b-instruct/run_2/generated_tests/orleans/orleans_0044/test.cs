using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Transactions.Abstractions;
using Orleans.Transactions.AzureStorage;
using Orleans.Hosting;
using Xunit;

namespace Orleans.Hosting
{
    public class AzureTableTransactionServicecollectionExtensionsTests
    {
        [Fact]
        public void AddAzureTableTransactionalStateStorage_ValidOptions_AddsServices()
        {
            // Arrange
            var siloBuilder = new SiloHostBuilder();

            // Act
            siloBuilder.AddAzureTableTransactionalStateStorage("test", options => { });

            // Assert
            var serviceProvider = siloBuilder.BuildServiceProvider();
            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
            var factory = serviceProvider.GetService<ITransactionalStateStorageFactory>();
            Assert.NotNull(factory);
            var lifecycleParticipant = serviceProvider.GetService<ILifecycleParticipant<ISiloLifecycle>>();
            Assert.NotNull(lifecycleParticipant);
        }

        [Fact]
        public void AddAzureTableTransactionalStateStorage_InvalidOptions_ThrowsException()
        {
            // Arrange
            var siloBuilder = new SiloHostBuilder();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => siloBuilder.AddAzureTableTransactionalStateStorage(null));
        }
    }
}
