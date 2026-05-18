using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public async Task AddOpenAITextEmbeddingGeneration_ServiceProvider_GetService_Called()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton(loggerFactoryMock.Object);

            // Act
            services.AddOpenAITextEmbeddingGeneration("modelId", "apiKey", "orgId", "serviceId", 128);

            // Assert
            var serviceProvider2 = services.BuildServiceProvider();
            var openAITextEmbeddingGenerationService = serviceProvider2.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(openAITextEmbeddingGenerationService);

            loggerFactoryMock.Verify(x => x.CreateLogger(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task AddOpenAITextEmbeddingGeneration_ServiceProvider_GetRequiredService_Called()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var openAIClientMock = new Mock<OpenAIClient>();
            services.AddSingleton(openAIClientMock.Object);

            // Act
            services.AddOpenAITextEmbeddingGeneration("modelId", openAIClientMock.Object, "serviceId", 128);

            // Assert
            var serviceProvider2 = services.BuildServiceProvider();
            var openAITextEmbeddingGenerationService = serviceProvider2.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(openAITextEmbeddingGenerationService);

            openAIClientMock.Verify(x => x.GetEmbeddingsAsync(It.IsAny<string>(), It.IsAny<IList<string>>(), It.IsAny<Kernel?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
