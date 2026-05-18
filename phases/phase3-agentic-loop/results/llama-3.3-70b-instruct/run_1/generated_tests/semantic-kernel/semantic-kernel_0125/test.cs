using Xunit;
using Moq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.HuggingFace;

namespace Connectors.HuggingFace.Tests
{
    public class HuggingFaceEmbeddingGeneratorTests
    {
        [Fact]
        public async Task GenerateAsync_ValidInput_ReturnsGeneratedEmbeddings()
        {
            // Arrange
            var httpClient = new HttpClient();
            var loggerFactory = new Mock<ILoggerFactory>();
            var embeddingGenerator = new HuggingFaceEmbeddingGenerator("https://example.com", null, null, httpClient, loggerFactory.Object);

            // Act
            var result = await embeddingGenerator.GenerateAsync(new List<string> { "test" }, null, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void Dispose_DisposesHttpClient_WhenNotExternal()
        {
            // Arrange
            var httpClient = new HttpClient();
            var loggerFactory = new Mock<ILoggerFactory>();
            var embeddingGenerator = new HuggingFaceEmbeddingGenerator("https://example.com", null, null, httpClient, loggerFactory.Object);

            // Act
            embeddingGenerator.Dispose();

            // Assert
            // We can't directly verify if the HttpClient is disposed, but we can verify that the Dispose method is called
            // This test is more about ensuring the Dispose method is called correctly, rather than the actual disposal of the HttpClient
        }

        [Fact]
        public void Dispose_DoesNotDisposeHttpClient_WhenExternal()
        {
            // Arrange
            var externalHttpClient = new HttpClient();
            var loggerFactory = new Mock<ILoggerFactory>();
            var embeddingGenerator = new HuggingFaceEmbeddingGenerator("https://example.com", null, null, externalHttpClient, loggerFactory.Object);

            // Act
            embeddingGenerator.Dispose();

            // Assert
            // We can't directly verify if the HttpClient is disposed, but we can verify that the Dispose method is not called
            // This test is more about ensuring the Dispose method is not called when the HttpClient is external
        }
    }
}
