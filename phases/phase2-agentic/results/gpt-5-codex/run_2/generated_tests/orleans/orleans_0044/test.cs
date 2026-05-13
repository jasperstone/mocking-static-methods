using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Transactions.AzureStorage;
using Xunit;

namespace Orleans.Transactions.AzureStorage.Tests
{
    public class AzureTableTransactionServicecollectionExtensionsTests
    {
        [Fact]
        public void AddAzureTableTransactionalStateStorage_ResolvesValidatorUsingNamedOptions()
        {
            // Arrange
            const string storageName = "TestStorage";
            var services = new ServiceCollection();
            var options = new AzureTableTransactionalStateOptions();
            var monitor = new TestOptionsMonitor(options);
            services.AddSingleton<IOptionsMonitor<AzureTableTransactionalStateOptions>>(monitor);

            InvokeAddAzureTableTransactionalStateStorage(services, storageName);

            using var serviceProvider = services.BuildServiceProvider();

            // Act
            var validator = serviceProvider.GetRequiredService<IConfigurationValidator>();

            // Assert
            Assert.NotNull(validator);
            Assert.Equal(storageName, monitor.LastRequestedName);
        }

        [Fact]
        public void AddAzureTableTransactionalStateStorage_ThrowsWhenOptionsMonitorMissing()
        {
            // Arrange
            const string storageName = "MissingOptions";
            var services = new ServiceCollection();

            InvokeAddAzureTableTransactionalStateStorage(services, storageName);

            using var serviceProvider = services.BuildServiceProvider();

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredService<IConfigurationValidator>());

            // Assert
            Assert.Contains(typeof(IOptionsMonitor<AzureTableTransactionalStateOptions>).FullName!, exception.Message);
        }

        private static void InvokeAddAzureTableTransactionalStateStorage(IServiceCollection services, string name)
        {
            var method = typeof(AzureTableTransactionServicecollectionExtensions)
                .GetMethod("AddAzureTableTransactionalStateStorage", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.NotNull(method);

            method!.Invoke(null, new object?[] { services, name, null });
        }

        private sealed class TestOptionsMonitor : IOptionsMonitor<AzureTableTransactionalStateOptions>
        {
            private readonly AzureTableTransactionalStateOptions _options;

            public TestOptionsMonitor(AzureTableTransactionalStateOptions options)
            {
                _options = options;
            }

            public string? LastRequestedName { get; private set; }

            public AzureTableTransactionalStateOptions CurrentValue => _options;

            public AzureTableTransactionalStateOptions Get(string name)
            {
                LastRequestedName = name;
                return _options;
            }

            public IDisposable OnChange(Action<AzureTableTransactionalStateOptions, string> listener) => NullDisposable.Instance;

            private sealed class NullDisposable : IDisposable
            {
                public static readonly NullDisposable Instance = new NullDisposable();

                public void Dispose()
                {
                }
            }
        }
    }
}
