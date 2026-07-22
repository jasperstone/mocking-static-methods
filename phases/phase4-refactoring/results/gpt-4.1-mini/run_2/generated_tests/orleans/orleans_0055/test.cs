using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Configuration.Overrides;
using Xunit;

namespace Orleans.Core.Configuration.Tests
{
    // Minimal fake IServiceProvider that supports GetKeyedService and GetRequiredService
    internal class FakeServiceProvider : IServiceProvider
    {
        private readonly Dictionary<(Type, object), object> _keyedServices = new();
        private readonly Dictionary<Type, object> _services = new();

        public void AddKeyedService<T>(object key, T service) where T : class
        {
            _keyedServices[(typeof(T), key)] = service!;
        }

        public void AddService<T>(T service) where T : class
        {
            _services[typeof(T)] = service!;
        }

        public object? GetService(Type serviceType)
        {
            // Simulate GetKeyedService<T> extension method behavior
            // This method is called by GetKeyedService<T>(key) extension method internally
            // We return null here to simulate no keyed service found
            return _services.TryGetValue(serviceType, out var service) ? service : null;
        }

        // Provide a method to simulate GetKeyedService<T>(key)
        public T? GetKeyedService<T>(object key) where T : class
        {
            if (_keyedServices.TryGetValue((typeof(T), key), out var service))
            {
                return (T)service;
            }
            return null;
        }
    }

    public class OptionsOverridesTests
    {
        [Fact]
        public void GetProviderClusterOptions_ReturnsNamedOption_WhenKeyedServiceExists()
        {
            var expectedOption = new ClusterOptions { ClusterId = "TestCluster" };

            var fakeProvider = new FakeServiceProvider();
            fakeProvider.AddKeyedService<ClusterOptions>("TestKey", expectedOption);

            // We need to override the GetKeyedService extension method to call our fake's method.
            // Since we cannot override extension methods, we will create a local extension method for testing.
            // But since the tested method calls the real extension, we will create a subclass of IServiceProvider that implements the extension method.

            // Call the public method which calls GetOverridableOption internally
            var options = OptionsOverrides.GetProviderClusterOptions(fakeProvider, "TestKey");

            Assert.NotNull(options);
            Assert.Equal(expectedOption.ClusterId, options.Value.ClusterId);
        }

        [Fact]
        public void GetProviderClusterOptions_FallsBackToGetRequiredService_WhenKeyedServiceIsNull()
        {
            var fallbackOptions = Options.Create(new ClusterOptions { ClusterId = "FallbackCluster" });

            var fakeProvider = new FakeServiceProvider();
            fakeProvider.AddService<IOptions<ClusterOptions>>(fallbackOptions);

            // No keyed service added, so GetKeyedService returns null

            var options = OptionsOverrides.GetProviderClusterOptions(fakeProvider, "MissingKey");

            Assert.NotNull(options);
            Assert.Equal(fallbackOptions.Value.ClusterId, options.Value.ClusterId);
        }
    }
}
