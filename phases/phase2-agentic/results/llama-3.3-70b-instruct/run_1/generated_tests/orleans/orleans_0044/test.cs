using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Orleans.Transactions.Abstractions;
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
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<AzureTableTransactionalStateOptions>>();
            var options = optionsMonitor.Get("test");
            Assert.NotNull(options);
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
