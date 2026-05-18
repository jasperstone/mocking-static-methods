using Moq;
using Moq.Protected;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.HuggingFace.Tests
{
    public class HuggingFaceEmbeddingGeneratorTests
    {
        [Fact]
        public void Dispose_ShouldDisposeHttpClient_WhenCreatedInternally()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var generator = new HuggingFaceEmbeddingGenerator(
                endpoint: new Uri("http://localhost"),
                httpClient: null);

            // Act
            generator.Dispose();

            // Assert
            mockHttpMessageHandler.Protected()
                .Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
        }

        [Fact]
        public void Dispose_ShouldNotDisposeHttpClient_WhenExternalHttpClientProvided()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var generator = new HuggingFaceEmbeddingGenerator(
                endpoint: new Uri("http://localhost"),
                httpClient: httpClient);

            // Act
            generator.Dispose();

            // Assert
            mockHttpMessageHandler.Protected()
                .Verify("Dispose", Times.Never(), ItExpr.IsAny<bool>());
        }
    }
}
