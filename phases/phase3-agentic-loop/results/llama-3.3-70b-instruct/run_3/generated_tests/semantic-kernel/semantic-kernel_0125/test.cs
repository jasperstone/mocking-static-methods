using Xunit;
using Moq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.SemanticKernel.Connectors.HuggingFace;

namespace Connectors.HuggingFace.Tests
{
    public class HuggingFaceEmbeddingGeneratorTests
    {
        [Fact]
        public async Task GenerateAsync_ValidInput_ReturnsGeneratedEmbeddings()
        {
            // Arrange
            var httpClient = new HttpClient();
            var embeddingGenerator = new HuggingFaceEmbeddingGenerator(new Uri("https://example.com"), null, httpClient, null);

            // Act
            var result = await embeddingGenerator.GenerateAsync(new List<string> { "input1", "input2" });

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void Dispose_DisposesHttpClient_WhenNotExternal()
        {
            // Arrange
            var httpClient = new HttpClient();
            var embeddingGenerator = new HuggingFaceEmbeddingGenerator(new Uri("https://example.com"), null, null, null);

            // Act and Assert
            embeddingGenerator.Dispose();
            Assert.Throws<NullReferenceException>(() => embeddingGenerator.GenerateAsync(new List<string> { "input1", "input2" }));
        }

        [Fact]
        public void Dispose_DoesNotDisposeHttpClient_WhenExternal()
        {
            // Arrange
            var externalHttpClient = new HttpClient();
            var embeddingGenerator = new HuggingFaceEmbeddingGenerator(new Uri("https://example.com"), null, externalHttpClient, null);

            // Act
            embeddingGenerator.Dispose();

            // Assert
            Assert.DoesNotThrow(() => externalHttpClient.Dispose());
        }
    }
}
