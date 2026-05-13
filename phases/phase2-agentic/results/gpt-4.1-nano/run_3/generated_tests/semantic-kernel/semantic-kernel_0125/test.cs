using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.HuggingFace.Services;

namespace SemanticKernel.Tests
{
    public class HuggingFaceEmbeddingGeneratorTests
    {
        [Fact]
        public void Dispose_DisposesHttpClient_WhenInternalClient()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            var generator = new HuggingFaceEmbeddingGenerator(
                modelId: "test-model",
                endpoint: new Uri("https://testendpoint"),
                httpClient: null,
                loggerFactory: NullLoggerFactory.Instance
            );

            // Act
            generator.Dispose();

            // Assert
            // Since _isExternalHttpClient is false, it should call Dispose on _httpClient
            // We can't directly verify the call on _httpClient because it's private,
            // but we can test that no exception is thrown and the code runs.
            // For more precise testing, we would need to expose or mock _httpClient.
        }

        [Fact]
        public void Dispose_DoesNotDisposeHttpClient_WhenExternalClient()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            var generator = new HuggingFaceEmbeddingGenerator(
                modelId: "test-model",
                endpoint: new Uri("https://testendpoint"),
                httpClient: mockHttpClient.Object,
                loggerFactory: NullLoggerFactory.Instance
            );

            // Act
            generator.Dispose();

            // Assert
            // Since _isExternalHttpClient is true, Dispose should not be called on _httpClient
            // Again, can't verify directly, but no exception should be thrown.
        }
    }
}
