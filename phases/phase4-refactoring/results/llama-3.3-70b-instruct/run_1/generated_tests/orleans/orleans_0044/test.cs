using Microsoft.Extensions.DependencyInjection;
using Orleans;
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
            var builder = new HostBuilder();

            // Act and Assert
            builder.ConfigureServices(services => services.AddAzureTableTransactionalStateStorage("test", options =>
            {
                options.TableName = "test-table";
            }));
        }

        [Fact]
        public void AddAzureTableTransactionalStateStorage_InvalidOptions_Throws()
        {
            // Arrange
            var builder = new HostBuilder();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => builder.ConfigureServices(services => services.AddAzureTableTransactionalStateStorage("test", options =>
            {
                options.TableName = null;
            })));
        }

        [Fact]
        public void AddAzureTableTransactionalStateStorage_MultipleCalls_DoesNotThrow()
        {
            // Arrange
            var builder = new HostBuilder();

            // Act and Assert
            builder.ConfigureServices(services => services.AddAzureTableTransactionalStateStorage("test1", options =>
            {
                options.TableName = "test-table1";
            }));

            builder.ConfigureServices(services => services.AddAzureTableTransactionalStateStorage("test2", options =>
            {
                options.TableName = "test-table2";
            }));
        }
    }
}
