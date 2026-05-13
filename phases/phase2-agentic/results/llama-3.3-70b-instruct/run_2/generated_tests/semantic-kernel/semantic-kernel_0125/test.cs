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
        public void Dispose_DisposesHttpClient_WhenNotExternal()
        {
            // Arrange
            var httpClient = new HttpClient();
            var loggerFactory = new LoggerFactory();
            var embeddingGenerator = new HuggingFaceEmbeddingGenerator(new Uri("https://example.com"), null, httpClient, loggerFactory);

            // Act
            embeddingGenerator.Dispose();

            // Assert
            Assert.Throws<ObjectDisposedException>(() => httpClient.SendAsync(new HttpRequestMessage()));
        }

        [Fact]
        public void Dispose_DoesNotDisposeHttpClient_WhenExternal()
        {
            // Arrange
            var httpClient = new HttpClient();
            var loggerFactory = new LoggerFactory();
            var embeddingGenerator = new HuggingFaceEmbeddingGenerator(new Uri("https://example.com"), null, null, loggerFactory);

            // Act
            embeddingGenerator.Dispose();

            // Assert
            Assert.DoesNotThrow(() => httpClient.SendAsync(new HttpRequestMessage()));
        }

        [Fact]
        public async Task GenerateAsync_GeneratesEmbeddings()
        {
            // Arrange
            var httpClient = new HttpClient();
            var loggerFactory = new LoggerFactory();
            var embeddingGenerator = new HuggingFaceEmbeddingGenerator(new Uri("https://example.com"), null, httpClient, loggerFactory);
            var values = new[] { "value1", "value2" };

            // Act
            var result = await embeddingGenerator.GenerateAsync(values);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(values.Length, result.Embeddings.Count);
        }
    }
}
