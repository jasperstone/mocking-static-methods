using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
            var name = "test";
            var configureOptions = new Action<OptionsBuilder<AzureTableTransactionalStateOptions>>(optionsBuilder =>
            {
                optionsBuilder.Configure(options => options.TableName = "test-table");
            });

            // Act
            services.AddAzureTableTransactionalStateStorage(name, configureOptions);

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
            var name = "test";
            var configureOptions = new Action<OptionsBuilder<AzureTableTransactionalStateOptions>>(optionsBuilder =>
            {
                optionsBuilder.Configure(options => options.TableName = null);
            });

            // Act and Assert
            Assert.Throws<Exception>(() => services.AddAzureTableTransactionalStateStorage(name, configureOptions));
        }
    }
}
