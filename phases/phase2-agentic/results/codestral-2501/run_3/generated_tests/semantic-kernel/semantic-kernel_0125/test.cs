using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Connectors.HuggingFace.Services.Tests
{
    public class HuggingFaceEmbeddingGeneratorTests
    {
        [Fact]
        public void Dispose_InternalHttpClient_DisposesHttpClient()
        {
            // Arrange
            var httpClient = new HttpClient();
            var generator = new HuggingFaceEmbeddingGenerator(
                endpoint: new Uri("https://api.huggingface.co"),
                httpClient: null,
                apiKey: "fake-api-key",
                loggerFactory: new NullLoggerFactory()
            );

            // Act
            generator.Dispose();

            // Assert
            Assert.Throws<ObjectDisposedException>(() => httpClient.SendAsync(new HttpRequestMessage()));
        }

        [Fact]
        public void Dispose_ExternalHttpClient_DoesNotDisposeHttpClient()
        {
            // Arrange
            var httpClient = new HttpClient();
            var generator = new HuggingFaceEmbeddingGenerator(
                endpoint: new Uri("https://api.huggingface.co"),
                httpClient: httpClient,
                apiKey: "fake-api-key",
                loggerFactory: new NullLoggerFactory()
            );

            // Act
            generator.Dispose();

            // Assert
            Assert.False(httpClient.SendAsync(new HttpRequestMessage()).IsCompleted);
        }
    }
}
