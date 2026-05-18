using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class TrackingServiceProvider : IServiceProvider
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

    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaChatCompletion_CallsGetServiceForLoggerFactory()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var innerServiceProvider = new ServiceCollection()
                .AddSingleton(loggerFactoryMock.Object)
                .BuildServiceProvider();

            var serviceProvider = new TrackingServiceProvider(innerServiceProvider);

            // Act
            var services = new ServiceCollection();
            services.AddSingleton(serviceProvider);
            services.AddOllamaChatCompletion("modelId", new Uri("http://localhost"), "serviceId");

            // Assert
            Assert.True(serviceProvider.LoggerFactoryCalled);
        }
    }
}
