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
        public void AddAzureTableTransactionalStateStorage_CallsInternalExtensionAndRegistersServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var builder = new TestSiloBuilder(services);

            // Add a dummy IOptionsMonitor<AzureTableTransactionalStateOptions> to satisfy the GetRequiredService call
            var optionsMonitor = new TestOptionsMonitor();
            services.AddSingleton<IOptionsMonitor<AzureTableTransactionalStateOptions>>(optionsMonitor);

            // Act
            var returnedBuilder = builder.AddAzureTableTransactionalStateStorage("testName", null);

            // Assert
            Assert.Same(builder, returnedBuilder);

            var provider = services.BuildServiceProvider();

            // The IConfigurationValidator should be registered and resolvable
            var validator = provider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);

            // The IOptionsMonitor should be the same instance we registered
            var resolvedOptionsMonitor = provider.GetService<IOptionsMonitor<AzureTableTransactionalStateOptions>>();
            Assert.Same(optionsMonitor, resolvedOptionsMonitor);
        }

        private class TestOptionsMonitor : IOptionsMonitor<AzureTableTransactionalStateOptions>
        {
            public AzureTableTransactionalStateOptions CurrentValue => Get("testName");

            public AzureTableTransactionalStateOptions Get(string name)
            {
                return new AzureTableTransactionalStateOptions();
            }

            public IDisposable OnChange(Action<AzureTableTransactionalStateOptions, string> listener)
            {
                return null;
            }
        }

        private class TestSiloBuilder : ISiloBuilder
        {
            private readonly IServiceCollection _services;

            public TestSiloBuilder(IServiceCollection services)
            {
                _services = services;
            }

            public IServiceCollection Services => _services;

            public ISiloBuilder ConfigureServices(Action<IServiceCollection> configureDelegate)
            {
                configureDelegate(_services);
                return this;
            }
        }
    }
}
