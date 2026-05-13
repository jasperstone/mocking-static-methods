using System;
using System.Net.Http;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.HuggingFace.Tests
{
    public class HuggingFaceEmbeddingGeneratorTests
    {
        [Fact]
        public void Dispose_DisposesHttpClient_WhenNotExternal()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            var generator = new HuggingFaceEmbeddingGenerator(
                endpoint: new Uri("http://localhost"),
                httpClient: mockHttpClient.Object);

            // Act
            generator.Dispose();

            // Assert
            mockHttpClient.Verify(m => m.Dispose(), Times.Once);
        }

        [Fact]
        public void Dispose_DoesNotDisposeHttpClient_WhenExternal()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            var generator = new HuggingFaceEmbeddingGenerator(
                endpoint: new Uri("http://localhost"),
                httpClient: mockHttpClient.Object);

            // Act
            generator.Dispose();

            // Assert
            mockHttpClient.Verify(m => m.Dispose(), Times.Never);
        }
    }
}
