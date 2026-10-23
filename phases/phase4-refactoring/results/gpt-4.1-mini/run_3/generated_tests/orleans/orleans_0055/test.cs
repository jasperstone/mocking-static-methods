using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Configuration.Overrides;
using Xunit;
using Moq;

namespace Orleans.Core.Configuration.Tests
{
    public class OptionsOverridesTests
    {
        private class TestOptions
        {
            public string Value { get; set; }
        }

        // We cannot mock extension methods directly, so we create a fake IServiceProvider implementation
        // that returns expected values for GetKeyedService and GetRequiredService calls.

        private class FakeServiceProvider : IServiceProvider
        {
            private readonly Func<Type, string, object> _getKeyedServiceFunc;
            private readonly Func<Type, object> _getRequiredServiceFunc;

            public FakeServiceProvider(Func<Type, string, object> getKeyedServiceFunc, Func<Type, object> getRequiredServiceFunc)
            {
                _getKeyedServiceFunc = getKeyedServiceFunc;
                _getRequiredServiceFunc = getRequiredServiceFunc;
            }

            public object GetService(Type serviceType)
            {
                // This method is not used by the tested code directly.
                return null;
            }

            // We add methods to simulate the extension methods
            public T GetKeyedService<T>(string key) where T : class
            {
                return (T)_getKeyedServiceFunc(typeof(T), key);
            }

            public T GetRequiredService<T>()
            {
                return (T)_getRequiredServiceFunc(typeof(T));
            }
        }

        [Fact]
        public void GetProviderClusterOptions_ReturnsNamedOption_WhenKeyedServiceExists()
        {
            var expectedOption = new ClusterOptions { ClusterId = "namedCluster" };
            var providerName = "namedProvider";

            var serviceProvider = new FakeServiceProvider(
                (type, key) => type == typeof(ClusterOptions) && key == providerName ? expectedOption : null,
                type => throw new InvalidOperationException("Should not call GetRequiredService when keyed service exists")
            );

            // Call extension method on IServiceProvider
            var options = OptionsOverrides.GetProviderClusterOptions(serviceProvider, providerName);

            Assert.NotNull(options);
            Assert.Equal(expectedOption.ClusterId, options.Value.ClusterId);
        }

        [Fact]
        public void GetProviderClusterOptions_ReturnsDefaultOption_WhenKeyedServiceIsNull()
        {
            var providerName = "missingProvider";
            var defaultOptions = Options.Create(new ClusterOptions { ClusterId = "defaultCluster" });

            var serviceProvider = new FakeServiceProvider(
                (type, key) => null,
                type => type == typeof(IOptions<ClusterOptions>) ? defaultOptions : null
            );

            var options = OptionsOverrides.GetProviderClusterOptions(serviceProvider, providerName);

            Assert.NotNull(options);
            Assert.Equal(defaultOptions.Value.ClusterId, options.Value.ClusterId);
        }
    }
}
