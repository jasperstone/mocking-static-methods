using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.HuggingFace.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_Should_Call_GetService_ILoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                               .Returns(loggerFactoryMock.Object);

            // Act
            services.AddHuggingFaceTextEmbeddingGeneration(
                model: "test-model",
                endpoint: new Uri("https://test.endpoint"),
                apiKey: "test-api-key",
                serviceId: "test-service",
                httpClient: null);

            // Build the service provider to verify the call
            var serviceProvider = services.BuildServiceProvider();

            // Trigger the registration to ensure the GetService call occurs
            var _ = serviceProvider.GetService<Microsoft.SemanticKernel.Embeddings.ITextEmbeddingGenerationService>();

            // Assert
            // Verify that GetService<ILoggerFactory>() was called during registration
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce);
        }
    }
}
