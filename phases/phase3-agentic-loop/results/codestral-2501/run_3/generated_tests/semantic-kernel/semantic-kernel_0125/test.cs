using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Connectors.HuggingFace;

namespace Connectors.HuggingFace.Services.Tests
{
    public class HuggingFaceEmbeddingGeneratorTests
    {
        [Fact]
        public async Task Dispose_ShouldDisposeHttpClient_WhenHttpClientIsCreatedInternally()
        {
            // Arrange
            var httpClient = new HttpClient();
            var loggerFactory = new NullLoggerFactory();
            var endpoint = new Uri("https://api.huggingface.com");

            var generator = new HuggingFaceEmbeddingGenerator(endpoint, "apiKey", httpClient, loggerFactory);

            // Act
            generator.Dispose();

            // Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => httpClient.SendAsync(new HttpRequestMessage()));
        }

        [Fact]
        public async Task Dispose_ShouldNotDisposeHttpClient_WhenHttpClientIsProvidedExternally()
        {
            // Arrange
            var httpClient = new HttpClient();
            var loggerFactory = new NullLoggerFactory();
            var endpoint = new Uri("https://api.huggingface.com");

            var generator = new HuggingFaceEmbeddingGenerator(endpoint, "apiKey", httpClient, loggerFactory);

            // Act
            generator.Dispose();

            // Assert
            var response = await httpClient.SendAsync(new HttpRequestMessage());
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task GenerateAsync_ShouldReturnEmbeddings()
        {
            // Arrange
            var httpClient = new HttpClient();
            var loggerFactory = new NullLoggerFactory();
            var endpoint = new Uri("https://api.huggingface.com");

            var generator = new HuggingFaceEmbeddingGenerator(endpoint, "apiKey", httpClient, loggerFactory);
            var values = new List<string> { "test" };

            // Act
            var result = await generator.GenerateAsync(values, null, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Embeddings);
        }
    }
}
