using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Transactions.AzureStorage;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class AzureTableTransactionServicecollectionExtensionsTests
    {
        [Fact]
        public void AddAzureTableTransactionalStateStorage_ValidInput_ServiceCollectionUpdated()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "TestName";
            var configureOptions = new Action<OptionsBuilder<AzureTableTransactionalStateOptions>>(optionsBuilder =>
            {
                optionsBuilder.Configure(options => { });
            });

            // Act
            services.AddAzureTableTransactionalStateStorage(name, configureOptions);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var azureTableTransactionalStateOptionsValidator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(azureTableTransactionalStateOptionsValidator);
        }

        [Fact]
        public void AddAzureTableTransactionalStateStorage_GetRequiredServiceCalled_ServiceCollectionUpdated()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "TestName";
            var configureOptions = new Action<OptionsBuilder<AzureTableTransactionalStateOptions>>(optionsBuilder =>
            {
                optionsBuilder.Configure(options => { });
            });

            // Act
            services.AddAzureTableTransactionalStateStorage(name, configureOptions);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var azureTableTransactionalStateOptionsValidator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(azureTableTransactionalStateOptionsValidator);
            var optionsMonitor = serviceProvider.GetService<IOptionsMonitor<AzureTableTransactionalStateOptions>>();
            Assert.NotNull(optionsMonitor);
            var options = optionsMonitor.Get(name);
            Assert.NotNull(options);
        }
    }
}
