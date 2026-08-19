using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Xunit;

namespace Orleans.Transactions.AzureStorage.Tests
{
    public class AzureTableTransactionServicecollectionExtensionsReflectionTests
    {
        [Fact]
        public void AddAzureTableTransactionalStateStorage_RegistersServicesCorrectly_UsingReflection()
        {
            // Arrange
            var services = new ServiceCollection();

            // Use reflection to get the internal static extension method
            var extensionsType = typeof(AzureTableTransactionServicecollectionExtensions);
            var method = extensionsType.GetMethod("AddAzureTableTransactionalStateStorage", BindingFlags.Static | BindingFlags.NonPublic, null,
                new Type[] { typeof(IServiceCollection), typeof(string), typeof(Action<OptionsBuilder<AzureTableTransactionalStateOptions>>) }, null);
            Assert.NotNull(method);

            // Prepare the configureOptions argument
            Action<OptionsBuilder<AzureTableTransactionalStateOptions>> configureOptions = options =>
            {
                options.Configure(opts =>
                {
                    opts.TableName = "TestTable";
                    opts.InitStage = 123;
                });
            };

            // Act
            var returnedServices = method.Invoke(null, new object[] { services, "TestName", configureOptions }) as IServiceCollection;

            // Assert
            Assert.Same(services, returnedServices);

            // Build service provider to test resolution
            var serviceProvider = services.BuildServiceProvider();

            // IConfigurationValidator should be registered as transient and resolvable
            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);

            // IOptionsMonitor<AzureTableTransactionalStateOptions> should be registered and configured
            var optionsMonitor = serviceProvider.GetService<IOptionsMonitor<AzureTableTransactionalStateOptions>>();
            Assert.NotNull(optionsMonitor);
            var options = optionsMonitor.Get("TestName");
            Assert.Equal("TestTable", options.TableName);
            Assert.Equal(123, options.InitStage);
        }
    }
}
