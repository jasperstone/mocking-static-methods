using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAITextEmbeddingGeneration_Should_Call_GetService_ILoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(mockLoggerFactory.Object);

            // Use a wrapper service provider to verify call
            var called = false;
            var wrappedProvider = new ServiceProviderWrapper(mockServiceProvider.Object, () => called = true);

            // Act
            var result = OpenAIServiceCollectionExtensions.AddOpenAITextEmbeddingGeneration(
                services,
                "model-id",
                "api-key",
                orgId: null,
                serviceId: "testService",
                dimensions: 128);

            // Assert
            Assert.NotNull(result);
            Assert.Contains(services, s => s == result);
        }

        private class ServiceProviderWrapper : IServiceProvider
        {
            private readonly IServiceProvider _inner;
            private readonly Action _onGetLoggerFactory;

            public ServiceProviderWrapper(IServiceProvider inner, Action onGetLoggerFactory)
            {
                _inner = inner;
                _onGetLoggerFactory = onGetLoggerFactory;
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(ILoggerFactory))
                {
                    _onGetLoggerFactory();
                }
                return _inner.GetService(serviceType);
            }
        }
    }
}
