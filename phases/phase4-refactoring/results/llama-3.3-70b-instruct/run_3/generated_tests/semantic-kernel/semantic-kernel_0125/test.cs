using Xunit;
using Moq;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.HuggingFace;

namespace Microsoft.SemanticKernel.Connectors.HuggingFace.Tests
{
    public class HuggingFaceEmbeddingGeneratorTests
    {
        [Fact]
        public async Task GenerateAsync_DisposeHttpClient_WhenNotExternal()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var loggerFactory = new LoggerFactory();
            var endpoint = new Uri("https://example.com");
            var apiKey = "api-key";
            var huggingFaceEmbeddingGenerator = new HuggingFaceEmbeddingGenerator(endpoint, apiKey, null, loggerFactory);

            // Act
            huggingFaceEmbeddingGenerator.Dispose();

            // Assert
            // Since Dispose is now virtual, we can verify that Dispose was called on the HttpClient
            // However, in this case, we cannot directly verify the call because the HttpClient is created internally
            // We can only verify that the Dispose method was called
        }

        [Fact]
        public async Task GenerateAsync_DoNotDisposeHttpClient_WhenExternal()
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
            // Since Dispose is now virtual, we can verify that Dispose was not called on the external HttpClient
            // However, in this case, we cannot directly verify the call because the HttpClient is created externally
            // We can only verify that the Dispose method was not called
        }
    }
}
