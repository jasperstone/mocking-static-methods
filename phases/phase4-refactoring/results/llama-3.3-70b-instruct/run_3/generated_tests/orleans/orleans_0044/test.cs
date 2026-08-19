using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Transactions.AzureStorage;
using Xunit;

namespace Orleans.Tests
{
    public class AzureTableTransactionServicecollectionExtensionsTests
    {
        [Fact]
        public void AddAzureTableTransactionalStateStorage_ValidOptions_DoesNotThrow()
        {
            // Arrange
            var siloBuilder = new SiloHostBuilder();

            // Act and Assert
            siloBuilder.AddAzureTableTransactionalStateStorage("test", options =>
            {
                options.TableName = "test-table";
                options.ConnectionString = "test-connection-string";
            });
        }

        [Fact]
        public void AddAzureTableTransactionalStateStorage_InvalidOptions_Throws()
        {
            // Arrange
            var siloBuilder = new SiloHostBuilder();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                siloBuilder.AddAzureTableTransactionalStateStorage("test", options =>
                {
                    options.TableName = null;
                    options.ConnectionString = "test-connection-string";
                });
            });
        }

        [Fact]
        public void AddAzureTableTransactionalStateStorage_GetRequiredService_Called()
        {
            // Arrange
            var siloBuilder = new SiloHostBuilder();
            siloBuilder.Services.AddOptions<AzureTableTransactionalStateOptions>("test");

            // Act
            siloBuilder.AddAzureTableTransactionalStateStorage("test", options =>
            {
                options.TableName = "test-table";
                options.ConnectionString = "test-connection-string";
            });

            // Assert
            var serviceProvider = siloBuilder.Services.BuildServiceProvider();
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<AzureTableTransactionalStateOptions>>();
            var options = optionsMonitor.Get("test");
            Assert.NotNull(options);
        }
    }
}
