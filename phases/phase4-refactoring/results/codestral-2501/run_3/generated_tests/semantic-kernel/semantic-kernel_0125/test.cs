using Xunit;
using System.Net.Http;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Microsoft.SemanticKernel.Connectors.HuggingFace.Tests
{
    public class HuggingFaceEmbeddingGeneratorTests
    {
        [Fact]
        public async Task Dispose_ShouldDisposeHttpClient_WhenCreatedInternally()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var generator = new HuggingFaceEmbeddingGenerator(
                modelId: "test-model",
                endpoint: new Uri("http://test-endpoint"),
                httpClient: null
            );

            // Act
            generator.Dispose();

            // Assert
            mockHttpMessageHandler.Verify(client => client.Dispose(), Times.Once);
        }

        [Fact]
        public async Task Dispose_ShouldNotDisposeHttpClient_WhenProvidedExternally()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var generator = new HuggingFaceEmbeddingGenerator(
                modelId: "test-model",
                endpoint: new Uri("http://test-endpoint"),
                httpClient: httpClient
            );

            // Act
            generator.Dispose();

            // Assert
            mockHttpMessageHandler.Verify(client => client.Dispose(), Times.Never);
        }
    }
}
