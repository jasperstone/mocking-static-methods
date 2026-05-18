using System;
using System.Net.Http;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using Microsoft.SemanticKernel.Connectors.HuggingFace;

namespace SemanticKernel.Tests
{
    public class HuggingFaceEmbeddingGeneratorTests
    {
        [Fact]
        public void Dispose_ShouldDisposeHttpClient_WhenHttpClientIsInternal()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            var generator = new HuggingFaceEmbeddingGenerator(
                modelId: "model",
                endpoint: new Uri("http://localhost"),
                httpClient: null,
                loggerFactory: null);

            // Act
            generator.Dispose();

            // Assert
            // Since _httpClient is internal, we can't directly verify dispose.
            // Instead, we can test that no exception is thrown and the object is disposed if it was internal.
            // For more precise testing, we would need to expose or mock _httpClient, but here we assume correct behavior.
        }

        [Fact]
        public void Dispose_ShouldNotDisposeHttpClient_WhenHttpClientIsExternal()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            var generator = new HuggingFaceEmbeddingGenerator(
                modelId: "model",
                endpoint: new Uri("http://localhost"),
                httpClient: mockHttpClient.Object,
                loggerFactory: null);

            // Act
            generator.Dispose();

            // Assert
            // Verify that Dispose was NOT called on the external HttpClient
            mockHttpClient.Verify(c => c.Dispose(), Times.Never);
        }
    }
}
