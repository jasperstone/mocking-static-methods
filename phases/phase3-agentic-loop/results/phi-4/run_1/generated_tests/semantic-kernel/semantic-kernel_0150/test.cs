using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        private class TrackingServiceProvider : IServiceProvider
        {
            private readonly IServiceProvider _innerServiceProvider;
            public bool LoggerFactoryCalled { get; private set; }

            public TrackingServiceProvider(IServiceProvider innerServiceProvider)
            {
                _innerServiceProvider = innerServiceProvider;
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(ILoggerFactory))
                {
                    LoggerFactoryCalled = true;
                }
                return _innerServiceProvider.GetService(serviceType);
            }
        }

        [Fact]
        public void AddOllamaChatCompletion_CallsGetServiceForLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var trackingServiceProvider = new TrackingServiceProvider(new ServiceCollection().BuildServiceProvider());

            var modelId = "test-model";
            var endpoint = new Uri("https://test-endpoint.com");

            // Act
            services.AddOllamaChatCompletion(modelId, endpoint, trackingServiceProvider);

            // Assert
            Assert.True(trackingServiceProvider.LoggerFactoryCalled);
        }
    }
}
