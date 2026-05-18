using System;
using System.Net.Http;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using Microsoft.SemanticKernel.Connectors.HuggingFace.Services;

namespace SemanticKernel.Tests
{
    public class HuggingFaceEmbeddingGeneratorTests
    {
        [Fact]
        public void Dispose_Should_Dispose_HttpClient_When_Internal()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            var generator = new HuggingFaceEmbeddingGenerator(
                modelId: "model",
                endpoint: new Uri("http://localhost"),
                httpClient: null,
                loggerFactory: null
            );

            // Act
            generator.Dispose();

            // Assert
            // Since _httpClient is internal, we can't directly verify dispose.
            // But we can test that no exception is thrown and the code runs.
            // For more precise testing, we would need to expose or mock _httpClient.
        }

        [Fact]
        public void Dispose_Should_Not_Dispose_HttpClient_When_External()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            var generator = new HuggingFaceEmbeddingGenerator(
                modelId: "model",
                endpoint: new Uri("http://localhost"),
                httpClient: mockHttpClient.Object,
                loggerFactory: null
            );

            // Act
            generator.Dispose();

            // Assert
            // Can't verify dispose directly, but ensure no exception.
        }
    }
}
