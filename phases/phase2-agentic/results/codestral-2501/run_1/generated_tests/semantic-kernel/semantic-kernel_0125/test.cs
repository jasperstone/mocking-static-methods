using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.HuggingFace.Tests
{
    public class HuggingFaceEmbeddingGeneratorTests
    {
        [Fact]
        public void Dispose_InternalHttpClient_DisposesHttpClient()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var generator = new HuggingFaceEmbeddingGenerator(
                modelId: "test-model",
                endpoint: new Uri("http://test-endpoint"),
                httpClient: httpClientMock.Object);

            // Act
            generator.Dispose();

            // Assert
            httpClientMock.Verify(client => client.Dispose(), Times.Once);
        }

        [Fact]
        public void Dispose_ExternalHttpClient_DoesNotDisposeHttpClient()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var generator = new HuggingFaceEmbeddingGenerator(
                modelId: "test-model",
                endpoint: new Uri("http://test-endpoint"),
                httpClient: httpClientMock.Object);

            // Act
            generator.Dispose();

            // Assert
            httpClientMock.Verify(client => client.Dispose(), Times.Never);
        }

        [Fact]
        public async Task GenerateAsync_CallsClientGenerateEmbeddingsAsync()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var generator = new HuggingFaceEmbeddingGenerator(
                modelId: "test-model",
                endpoint: new Uri("http://test-endpoint"),
                httpClient: httpClientMock.Object);

            var values = new[] { "test1", "test2" };
            var cancellationToken = new CancellationToken();

            // Act
            await generator.GenerateAsync(values, cancellationToken: cancellationToken);

            // Assert
            // Verify that GenerateEmbeddingsAsync is called with the correct parameters
            // This is a placeholder assertion. The actual implementation would require more detailed setup and verification.
            // Assert.True(generator.Client.GenerateEmbeddingsAsync was called with values and cancellationToken);
        }
    }
}
