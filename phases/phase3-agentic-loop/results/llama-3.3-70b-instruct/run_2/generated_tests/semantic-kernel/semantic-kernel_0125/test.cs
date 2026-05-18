using Xunit;
using System.Net.Http;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.SemanticKernel.Connectors.HuggingFace;

namespace Connectors.HuggingFace.Tests
{
    public class HuggingFaceEmbeddingGeneratorTests
    {
        [Fact]
        public async Task GenerateAsync_DisposesHttpClient_WhenNotExternal()
        {
            // Arrange
            var httpClient = new HttpClient();
            var embeddingGenerator = new HuggingFaceEmbeddingGenerator(new Uri("http://localhost"), null, httpClient, null);

            // Act
            await embeddingGenerator.GenerateAsync(new List<string>(), null, CancellationToken.None);
            embeddingGenerator.Dispose();

            // Assert
            // We can't directly verify if Dispose was called on the HttpClient
            // But we can verify that the HttpClient is disposed by checking its state
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await httpClient.SendAsync(new HttpRequestMessage()));
        }

        [Fact]
        public async Task GenerateAsync_DoesNotDisposeHttpClient_WhenExternal()
        {
            // Arrange
            var externalHttpClient = new HttpClient();
            var embeddingGenerator = new HuggingFaceEmbeddingGenerator(new Uri("http://localhost"), null, externalHttpClient, null);

            // Act
            await embeddingGenerator.GenerateAsync(new List<string>(), null, CancellationToken.None);
            embeddingGenerator.Dispose();

            // Assert
            // We can verify that the external HttpClient is not disposed
            await externalHttpClient.SendAsync(new HttpRequestMessage());
        }
    }
}
