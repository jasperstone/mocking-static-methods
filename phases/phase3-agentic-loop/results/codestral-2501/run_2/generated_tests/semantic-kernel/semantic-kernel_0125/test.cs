using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Connectors.HuggingFace.Core;
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
            var generator = new HuggingFaceEmbeddingGenerator(
                modelId: "test-model",
                endpoint: new Uri("http://test-endpoint"));

            // Act
            generator.Dispose();

            // Assert
            // Since we cannot directly verify the disposal of HttpClient, we can check if the generator is disposed
            Assert.Throws<ObjectDisposedException>(() => generator.GenerateAsync(new List<string> { "test" }));
        }

        [Fact]
        public void Dispose_ExternalHttpClient_DoesNotDisposeHttpClient()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            var generator = new HuggingFaceEmbeddingGenerator(
                modelId: "test-model",
                endpoint: new Uri("http://test-endpoint"),
                httpClient: mockHttpClient.Object);

            // Act
            generator.Dispose();

            // Assert
            mockHttpClient.Verify(client => client.Dispose(), Times.Never);
        }

        [Fact]
        public async Task GenerateAsync_CallsClientGenerateEmbeddingsAsync()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            var mockHuggingFaceClient = new Mock<HuggingFaceClient>(mockHttpClient.Object);
            var generator = new HuggingFaceEmbeddingGenerator(
                modelId: "test-model",
                endpoint: new Uri("http://test-endpoint"),
                httpClient: mockHttpClient.Object);

            var values = new List<string> { "test" };
            var cancellationToken = new CancellationToken();

            // Act
            await generator.GenerateAsync(values, null, cancellationToken);

            // Assert
            mockHuggingFaceClient.Verify(client => client.GenerateEmbeddingsAsync(values, null, cancellationToken), Times.Once);
        }
    }
}
