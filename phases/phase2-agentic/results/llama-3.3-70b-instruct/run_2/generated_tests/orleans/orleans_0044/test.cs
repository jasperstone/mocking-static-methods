using Microsoft.Extensions.DependencyInjection;
using Orleans.Transactions.AzureStorage;
using Xunit;

namespace Orleans.Tests
{
    public class AzureTableTransactionServicecollectionExtensionsTests
    {
        [Fact]
        public void AddAzureTableTransactionalStateStorage_RegistersRequiredServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddAzureTableTransactionalStateStorage("test");

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
            var factory = serviceProvider.GetService<ITransactionalStateStorageFactory>();
            Assert.NotNull(factory);
            var lifecycleParticipant = serviceProvider.GetService<ILifecycleParticipant<ISiloLifecycle>>();
            Assert.NotNull(lifecycleParticipant);
        }

        [Fact]
        public void AddAzureTableTransactionalStateStorage_ThrowsException_WhenNameIsNull()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => services.AddAzureTableTransactionalStateStorage(null));
        }

        [Fact]
        public void AddAzureTableTransactionalStateStorage_ThrowsException_WhenNameIsEmpty()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act and Assert
            Assert.Throws<ArgumentException>(() => services.AddAzureTableTransactionalStateStorage(string.Empty));
        }
    }
}
