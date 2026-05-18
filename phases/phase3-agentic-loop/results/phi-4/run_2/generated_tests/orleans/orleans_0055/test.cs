using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration.Overrides;
using Xunit;

namespace Orleans.Tests
{
    public class OptionsOverridesTests
    {
        public class ClusterOptions
        {
            public string ServiceId { get; set; }
            public string ClusterId { get; set; }
        }

        public interface IServiceProviderWrapper
        {
            T GetKeyedService<T>(string key);
            IOptions<T> GetRequiredService<T>();
        }

        public class ServiceProviderWrapper : IServiceProviderWrapper
        {
            private readonly IServiceProvider _serviceProvider;

            public ServiceProviderWrapper(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public T GetKeyedService<T>(string key)
            {
                return _serviceProvider.GetKeyedService<T>(key);
            }

            public IOptions<T> GetRequiredService<T>()
            {
                return _serviceProvider.GetRequiredService<IOptions<T>>();
            }
        }

        [Fact]
        public void GetProviderClusterOptions_ReturnsKeyedService_WhenAvailable()
        {
            // Arrange
            var mockWrapper = new Mock<IServiceProviderWrapper>();
            var keyedService = new ClusterOptions();
            mockWrapper.Setup(w => w.GetKeyedService<ClusterOptions>("providerName")).Returns(keyedService);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(s => s.GetService(typeof(IServiceProviderWrapper))).Returns(mockWrapper.Object);

            // Act
            var result = OptionsOverrides.GetProviderClusterOptions(mockServiceProvider.Object, "providerName");

            // Assert
            Assert.Same(keyedService, result.Value);
        }

        [Fact]
        public void GetProviderClusterOptions_ReturnsRequiredService_WhenKeyedServiceNotAvailable()
        {
            // Arrange
            var mockWrapper = new Mock<IServiceProviderWrapper>();
            var requiredService = new Mock<IOptions<ClusterOptions>>();
            mockWrapper.Setup(w => w.GetKeyedService<ClusterOptions>("providerName")).Returns((ClusterOptions)null);
            mockWrapper.Setup(w => w.GetRequiredService<ClusterOptions>()).Returns(requiredService.Object);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(s => s.GetService(typeof(IServiceProviderWrapper))).Returns(mockWrapper.Object);

            // Act
            var result = OptionsOverrides.GetProviderClusterOptions(mockServiceProvider.Object, "providerName");

            // Assert
            Assert.Same(requiredService.Object, result);
        }
    }
}
