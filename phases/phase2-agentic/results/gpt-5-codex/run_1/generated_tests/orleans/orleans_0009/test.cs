using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Persistence.DynamoDB;
using Orleans.Providers;
using Orleans.Runtime;
using Xunit;

namespace Orleans.Persistence.DynamoDB.Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorage_UsesOptionsMonitorFromServiceProvider()
        {
            var services = new ServiceCollection();
            var providerName = "CustomProvider";
            var configureCalled = false;
            var monitor = new RecordingOptionsMonitor(new DynamoDBStorageOptions { TableName = "CustomTable" });

            services.AddDynamoDBGrainStorage(providerName, builder =>
            {
                configureCalled = true;
                builder.Configure(options => options.TableName = "ConfiguredTable");
            });

            Assert.True(configureCalled);
            services.AddSingleton<IOptionsMonitor<DynamoDBStorageOptions>>(monitor);

            using var provider = services.BuildServiceProvider(validateScopes: true);
            Assert.Same(monitor, provider.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>());

            var validator = Assert.Single(provider.GetServices<IConfigurationValidator>().OfType<DynamoDBGrainStorageOptionsValidator>());
            validator.ValidateConfiguration();

            Assert.Equal(new[] { providerName }, monitor.RequestedNames);
        }

        [Fact]
        public void AddDynamoDBGrainStorageAsDefault_UsesDefaultProviderNameForOptionsResolution()
        {
            var services = new ServiceCollection();
            var monitor = new RecordingOptionsMonitor(new DynamoDBStorageOptions { TableName = "DefaultTable" });

            services.AddDynamoDBGrainStorageAsDefault();
            services.AddSingleton<IOptionsMonitor<DynamoDBStorageOptions>>(monitor);

            using var provider = services.BuildServiceProvider(validateScopes: true);
            Assert.Same(monitor, provider.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>());

            var validator = Assert.Single(provider.GetServices<IConfigurationValidator>().OfType<DynamoDBGrainStorageOptionsValidator>());
            validator.ValidateConfiguration();

            Assert.Equal(new[] { ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME }, monitor.RequestedNames);
        }

        private sealed class RecordingOptionsMonitor : IOptionsMonitor<DynamoDBStorageOptions>
        {
            private readonly List<string> _requestedNames = new();
            private readonly DynamoDBStorageOptions _options;

            public RecordingOptionsMonitor(DynamoDBStorageOptions options)
            {
                _options = options;
            }

            public IReadOnlyList<string> RequestedNames => _requestedNames;
            public DynamoDBStorageOptions CurrentValue => _options;

            public DynamoDBStorageOptions Get(string name)
            {
                _requestedNames.Add(name);
                return _options;
            }

            public IDisposable OnChange(Action<DynamoDBStorageOptions, string> listener) => EmptyDisposable.Instance;

            private sealed class EmptyDisposable : IDisposable
            {
                public static readonly EmptyDisposable Instance = new();

                public void Dispose()
                {
                }
            }
        }
    }
}
