using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.ImageToText;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.HuggingFace.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_RegistersService_WithLoggerFactoryFromServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton(loggerFactoryMock.Object);

            var endpoint = new Uri("https://fake-endpoint");
            string apiKey = "fake-api-key";
            string serviceId = "test-service";

            // Act
            var returnedServices = services.AddHuggingFaceTextEmbeddingGeneration(
                endpoint,
                apiKey,
                serviceId);

            var provider = returnedServices.BuildServiceProvider();

            // Resolve the service by serviceId key
            var embeddingService = provider.GetService<ITextEmbeddingGenerationService>();

            // Assert
            Assert.Same(services, returnedServices);
            Assert.NotNull(embeddingService);
            Assert.IsType<HuggingFaceTextEmbeddingGenerationService>(embeddingService);
        }

        [Fact]
        public void AddHuggingFaceImageToText_RegistersService_WithLoggerFactoryFromServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton(loggerFactoryMock.Object);

            string model = "test-model";
            Uri endpoint = new Uri("https://fake-endpoint");
            string apiKey = "fake-api-key";
            string serviceId = "test-service";

            // Act
            var returnedServices = services.AddHuggingFaceImageToText(
                model,
                endpoint,
                apiKey,
                serviceId);

            var provider = returnedServices.BuildServiceProvider();

            var imageToTextService = provider.GetService<IImageToTextService>();

            // Assert
            Assert.Same(services, returnedServices);
            Assert.NotNull(imageToTextService);
            Assert.IsType<HuggingFaceImageToTextService>(imageToTextService);
        }
    }
}
