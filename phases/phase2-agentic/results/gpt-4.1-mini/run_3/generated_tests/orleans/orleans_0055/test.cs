using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration.Overrides;
using Xunit;

namespace Orleans.Configuration.Overrides.Tests
{
    public class OptionsOverridesTests
    {
        private class TestOptions
        {
            public string Value { get; set; }
        }

        [Fact]
        public void GetProviderClusterOptions_ReturnsNamedOption_WhenKeyedServiceExists()
        {
            // Arrange
            var providerName = "testProvider";
            var expectedOption = new ClusterOptions { ClusterId = "cluster1" };

            var serviceProviderMock = new Mock<IServiceProvider>();

            // Setup GetKeyedService extension method by mocking IServiceProvider.GetService with a custom implementation
            // Since GetKeyedService is an extension method, we simulate it by mocking a helper service that returns the option
            // But since we cannot mock extension methods directly, we will create a helper interface to simulate this behavior

            // Instead, we will create a derived class to override GetKeyedService for testing
            var services = new TestServiceProviderWithKeyedService<ClusterOptions>(providerName, expectedOption);

            // Act
            var options = services.GetProviderClusterOptions(providerName);

            // Assert
            Assert.NotNull(options);
            Assert.Equal(expectedOption.ClusterId, options.Value.ClusterId);
        }

        [Fact]
        public void GetProviderClusterOptions_CallsGetRequiredService_WhenKeyedServiceIsNull()
        {
            // Arrange
            var providerName = "testProvider";
            var expectedOptions = Options.Create(new ClusterOptions { ClusterId = "defaultCluster" });

            var serviceProviderMock = new Mock<IServiceProvider>();

            // Setup GetRequiredService<IOptions<ClusterOptions>> to return expectedOptions
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptions<ClusterOptions>)))
                .Returns(expectedOptions);

            // We need to simulate GetKeyedService returning null
            // Since GetKeyedService is an extension method, we cannot mock it directly
            // We will create a derived class that overrides GetKeyedService to return null

            var services = new TestServiceProviderWithKeyedService<ClusterOptions>(providerName, null, serviceProviderMock.Object);

            // Act
            var options = services.GetProviderClusterOptions(providerName);

            // Assert
            Assert.NotNull(options);
            Assert.Equal(expectedOptions.Value.ClusterId, options.Value.ClusterId);
        }

        // Helper class to simulate IServiceProvider with GetKeyedService behavior
        private class TestServiceProviderWithKeyedService<TOptions> : IServiceProvider where TOptions : class, new()
        {
            private readonly string _key;
            private readonly TOptions _keyedService;
            private readonly IServiceProvider _innerServiceProvider;

            public TestServiceProviderWithKeyedService(string key, TOptions keyedService, IServiceProvider innerServiceProvider = null)
            {
                _key = key;
                _keyedService = keyedService;
                _innerServiceProvider = innerServiceProvider ?? new DefaultServiceProvider();
            }

            public object GetService(Type serviceType)
            {
                // Simulate GetRequiredService<IOptions<TOptions>> call
                if (serviceType == typeof(IOptions<TOptions>))
                {
                    return _innerServiceProvider.GetService(serviceType);
                }

                return null;
            }

            // Extension method simulation for GetKeyedService
            public TOptions GetKeyedService(string key)
            {
                if (key == _key)
                {
                    return _keyedService;
                }
                return null;
            }
        }

        // Default IServiceProvider that returns null for all services
        private class DefaultServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType) => null;
        }
    }
}
