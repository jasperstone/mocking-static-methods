using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaChatCompletion_CallsGetServiceForLoggerFactory()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            var customServiceProvider = new CustomServiceProvider(serviceProviderMock.Object);

            // Use a custom ServiceCollection that uses our custom IServiceProvider
            var services = new CustomServiceCollection(customServiceProvider);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            // Act
            services.AddOllamaChatCompletion("modelId", new Uri("http://localhost"), "serviceId");

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
        }

        private class CustomServiceProvider : IServiceProvider
        {
            private readonly IServiceProvider _innerServiceProvider;

            public CustomServiceProvider(IServiceProvider innerServiceProvider)
            {
                _innerServiceProvider = innerServiceProvider;
            }

            public object GetService(Type serviceType)
            {
                return _innerServiceProvider.GetService(serviceType);
            }
        }

        private class CustomServiceCollection : ServiceCollection
        {
            private readonly IServiceProvider _serviceProvider;

            public CustomServiceCollection(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public override IServiceProvider BuildServiceProvider()
            {
                return _serviceProvider;
            }
        }
    }
}
