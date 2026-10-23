using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Connectors.HuggingFace.Core;
using Microsoft.SemanticKernel.Http;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.HuggingFace.Tests
{
    public class HuggingFaceEmbeddingGeneratorTests
    {
        [Fact]
        public void Dispose_ShouldDisposeHttpClient_WhenHttpClientIsNotExternal()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            var generator = new HuggingFaceEmbeddingGenerator(
                modelId: "test-model",
                endpoint: new Uri("http://test-endpoint")
            );

            // Act
            generator.Dispose();

            // Assert
            mockHttpClient.Verify(client => client.Dispose(), Times.Once);
        }

        [Fact]
        public void Dispose_ShouldNotDisposeHttpClient_WhenHttpClientIsExternal()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            var generator = new HuggingFaceEmbeddingGenerator(
                modelId: "test-model",
                endpoint: new Uri("http://test-endpoint"),
                httpClient: mockHttpClient.Object
            );

            // Act
            generator.Dispose();

            // Assert
            mockHttpClient.Verify(client => client.Dispose(), Times.Never);
        }
    }
}
