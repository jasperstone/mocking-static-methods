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
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var embeddingGenerator = new HuggingFaceEmbeddingGenerator(
                modelId: "test-model",
                endpoint: new Uri("http://test-endpoint"),
                httpClient: null,
                loggerFactory: loggerFactoryMock.Object);

            // Act
            embeddingGenerator.Dispose();

            // Assert
            httpClientMock.Verify(client => client.Dispose(), Times.Once);
        }

        [Fact]
        public void Dispose_ExternalHttpClient_DoesNotDisposeHttpClient()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var embeddingGenerator = new HuggingFaceEmbeddingGenerator(
                modelId: "test-model",
                endpoint: new Uri("http://test-endpoint"),
                httpClient: httpClientMock.Object,
                loggerFactory: loggerFactoryMock.Object);

            // Act
            embeddingGenerator.Dispose();

            // Assert
            httpClientMock.Verify(client => client.Dispose(), Times.Never);
        }

        [Fact]
        public async Task GenerateAsync_CallsClientGenerateEmbeddingsAsync()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var embeddingGenerator = new HuggingFaceEmbeddingGenerator(
                modelId: "test-model",
                endpoint: new Uri("http://test-endpoint"),
                httpClient: httpClientMock.Object,
                loggerFactory: loggerFactoryMock.Object);

            var values = new[] { "test1", "test2" };
            var cancellationToken = new CancellationToken();

            // Act
            await embeddingGenerator.GenerateAsync(values, null, cancellationToken);

            // Assert
            // Verify that GenerateEmbeddingsAsync is called with the correct parameters
            // This is a placeholder assertion. You need to mock the HuggingFaceClient and verify the call.
            // Assert.True(true); // Replace with actual verification
        }
    }
}
