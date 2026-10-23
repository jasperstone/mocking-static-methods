using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Transactions.AzureStorage;
using Xunit;

namespace Orleans.Transactions.AzureStorage.Tests
{
    public class AzureTableTransactionSiloBuilderExtensionsTests
    {
        [Fact]
        public void AddAzureTableTransactionalStateStorage_RegistersServicesCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var siloBuilder = new SiloBuilderMock(services);

            // Act
            siloBuilder.AddAzureTableTransactionalStateStorage("testName", optionsBuilder =>
            {
                optionsBuilder.Configure(options =>
                {
                    options.TableName = "TestTable";
                    options.InitStage = 123;
                });
            });

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            // Check that IConfigurationValidator is registered and can be resolved
            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);

            // Check that IOptionsMonitor<AzureTableTransactionalStateOptions> is registered and can be resolved
            var optionsMonitor = serviceProvider.GetService<IOptionsMonitor<AzureTableTransactionalStateOptions>>();
            Assert.NotNull(optionsMonitor);

            // Check that the named options can be retrieved
            var namedOptions = optionsMonitor.Get("testName");
            Assert.NotNull(namedOptions);
            Assert.Equal("TestTable", namedOptions.TableName);
            Assert.Equal(123, namedOptions.InitStage);
        }

        // Minimal mock of ISiloBuilder to test extension methods
        private class SiloBuilderMock : ISiloBuilder
        {
            public IServiceCollection Services { get; }

            public SiloBuilderMock(IServiceCollection services)
            {
                Services = services;
            }

            public ISiloBuilder ConfigureServices(Action<IServiceCollection> configureDelegate)
            {
                configureDelegate(Services);
                return this;
            }
        }
    }
}
