using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Microsoft.SemanticKernel.Connectors.HuggingFace
{
    public class HuggingFaceEmbeddingGeneratorTests
    {
        [Fact]
        public async Task GenerateAsync_ValidInput_ReturnsGeneratedEmbeddings()
        {
            // Arrange
            var httpClient = new HttpClient();
            var loggerFactory = new LoggerFactory();
            var endpoint = new Uri("https://example.com");
            var apiKey = "api-key";
            var values = new List<string> { "value1", "value2" };

            var huggingFaceClientMock = new Mock<HuggingFaceClient>(endpoint, apiKey, httpClient, loggerFactory);
            huggingFaceClientMock
                .Setup(c => c.GenerateEmbeddingsAsync(It.IsAny<List<string>>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Embedding<float>> { new Embedding<float>(new float[] { 1.0f, 2.0f }) });

            var huggingFaceEmbeddingGenerator = new HuggingFaceEmbeddingGenerator(endpoint, apiKey, httpClient, loggerFactory);

            // Act
            var result = await huggingFaceEmbeddingGenerator.GenerateAsync(values);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Embeddings);
        }

        [Fact]
        public void Dispose_DisposesHttpClient_WhenNotExternal()
        {
            // Arrange
            var httpClient = new HttpClient();
            var loggerFactory = new LoggerFactory();
            var endpoint = new Uri("https://example.com");
            var apiKey = "api-key";

            var huggingFaceEmbeddingGenerator = new HuggingFaceEmbeddingGenerator(endpoint, apiKey, null, loggerFactory);

            // Act
            huggingFaceEmbeddingGenerator.Dispose();

            // Assert
            Assert.Throws<ObjectDisposedException>(() => huggingFaceEmbeddingGenerator.GenerateAsync(new List<string>()));
        }

        [Fact]
        public void Dispose_DoesNotDisposeHttpClient_WhenExternal()
        {
            // Arrange
            var httpClient = new HttpClient();
            var loggerFactory = new LoggerFactory();
            var endpoint = new Uri("https://example.com");
            var apiKey = "api-key";

            var huggingFaceEmbeddingGenerator = new HuggingFaceEmbeddingGenerator(endpoint, apiKey, httpClient, loggerFactory);

            // Act
            huggingFaceEmbeddingGenerator.Dispose();

            // Assert
            Assert.DoesNotThrow(() => huggingFaceEmbeddingGenerator.GenerateAsync(new List<string>()));
        }
    }
}
