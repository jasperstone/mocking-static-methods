using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Connectors.HuggingFace.Core;
using Moq;
using Xunit;

namespace Connectors.HuggingFace.Services.Tests
{
    public class HuggingFaceEmbeddingGeneratorTests
    {
        [Fact]
        public void Constructor_InitializesCorrectly()
        {
            // Arrange
            var httpClient = new HttpClient();
            var endpoint = new Uri("https://api.huggingface.com");
            var apiKey = "test-api-key";
            var loggerFactory = new NullLoggerFactory();

            // Act
            var generator = new HuggingFaceEmbeddingGenerator(endpoint, apiKey, httpClient, loggerFactory);

            // Assert
            var clientField = typeof(HuggingFaceEmbeddingGenerator).GetField("Client", BindingFlags.NonPublic | BindingFlags.Instance);
            var metadataField = typeof(HuggingFaceEmbeddingGenerator).GetField("_metadata", BindingFlags.NonPublic | BindingFlags.Instance);
            var isExternalHttpClientField = typeof(HuggingFaceEmbeddingGenerator).GetField("_isExternalHttpClient", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.NotNull(clientField.GetValue(generator));
            Assert.NotNull(metadataField.GetValue(generator));
            Assert.False((bool)isExternalHttpClientField.GetValue(generator));
        }

        [Fact]
        public async Task GenerateAsync_CallsHuggingFaceClientGenerateEmbeddingsAsync()
        {
            // Arrange
            var httpClient = new HttpClient();
            var endpoint = new Uri("https://api.huggingface.com");
            var apiKey = "test-api-key";
            var loggerFactory = new NullLoggerFactory();
            var generator = new HuggingFaceEmbeddingGenerator(endpoint, apiKey, httpClient, loggerFactory);

            var mockHuggingFaceClient = new Mock<HuggingFaceClient>(httpClient, null, endpoint, apiKey, NullLogger.Instance);
            mockHuggingFaceClient.Setup(client => client.GenerateEmbeddingsAsync(It.IsAny<List<string>>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<float[]> { new float[] { 0.1f, 0.2f } });

            var clientField = typeof(HuggingFaceEmbeddingGenerator).GetField("Client", BindingFlags.NonPublic | BindingFlags.Instance);
            clientField.SetValue(generator, mockHuggingFaceClient.Object);

            var values = new List<string> { "test" };

            // Act
            var result = await generator.GenerateAsync(values);

            // Assert
            mockHuggingFaceClient.Verify(client => client.GenerateEmbeddingsAsync(It.IsAny<List<string>>(), null, It.IsAny<CancellationToken>()), Times.Once);
            Assert.NotNull(result);
            Assert.Single(result.Embeddings);
        }

        [Fact]
        public async Task Dispose_DisposesHttpClientIfCreatedInternally()
        {
            // Arrange
            var httpClient = new HttpClient();
            var endpoint = new Uri("https://api.huggingface.com");
            var apiKey = "test-api-key";
            var loggerFactory = new NullLoggerFactory();
            var generator = new HuggingFaceEmbeddingGenerator(endpoint, apiKey, httpClient, loggerFactory);

            // Act
            generator.Dispose();

            // Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => httpClient.SendAsync(new HttpRequestMessage()));
        }
    }
}
