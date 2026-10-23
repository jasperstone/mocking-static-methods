using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Transactions.AzureStorage;
using Xunit;

namespace Orleans.Tests
{
    public class AzureTableTransactionServicecollectionExtensionsTests
    {
        [Fact]
        public void AddAzureTableTransactionalStateStorage_ValidOptions_AddsServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddAzureTableTransactionalStateStorage("test", options => { });

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
        }

        [Fact]
        public void AddAzureTableTransactionalStateStorage_InvalidOptions_ThrowsException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act and Assert
            Assert.Throws<Exception>(() => services.AddAzureTableTransactionalStateStorage(null));
        }
    }
}
