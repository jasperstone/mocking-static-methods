using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Embeddings; // Ensure this using directive is included

namespace Microsoft.SemanticKernel.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_CallsGetServiceForLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            // Setup a method to wrap the GetService call
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            // Mock the AddKeyedSingleton method
            var servicesMock = new Mock<IServiceCollection>(MockBehavior.Strict);
            servicesMock
                .Setup(s => s.AddKeyedSingleton<ITextEmbeddingGenerationService>(
                    It.IsAny<string>(),
                    It.IsAny<Func<IServiceProvider, object, ITextEmbeddingGenerationService>>()))
                .Callback<string, Func<IServiceProvider, object, ITextEmbeddingGenerationService>>((_, factory) =>
                {
                    // Invoke the factory with our mocked service provider
                    factory(serviceProviderMock.Object, null);
                });

            // Act
            servicesMock.Object.AddHuggingFaceTextEmbeddingGeneration(
                new Uri("https://example.com"),
                "test-api-key",
                "test-service-id",
                new HttpClient());

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
        }
    }
}
