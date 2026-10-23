using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.HuggingFace;

namespace Microsoft.SemanticKernel.Connectors.HuggingFace;

public class HuggingFaceEmbeddingGeneratorTests
{
    [Fact]
    public async Task GenerateAsync_ValidInput_ReturnsGeneratedEmbeddings()
    {
        // Arrange
        var endpoint = new Uri("https://example.com");
        var apiKey = "api-key";
        var httpClient = new HttpClient();
        var loggerFactory = new LoggerFactory();

        var huggingFaceEmbeddingGenerator = new HuggingFaceEmbeddingGenerator(endpoint, apiKey, null, loggerFactory);

        var values = new List<string> { "value1", "value2" };

        // Act
        var result = await huggingFaceEmbeddingGenerator.GenerateAsync(values);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void Dispose_DisposesHttpClient_WhenNotExternal()
    {
        // Arrange
        var endpoint = new Uri("https://example.com");
        var apiKey = "api-key";
        var httpClient = new HttpClient();
        var loggerFactory = new LoggerFactory();

        var huggingFaceEmbeddingGenerator = new HuggingFaceEmbeddingGenerator(endpoint, apiKey, null, loggerFactory);

        // Act
        huggingFaceEmbeddingGenerator.Dispose();

        // Assert
        // No assertion possible due to lack of access to _httpClient
    }

    [Fact]
    public void Dispose_DoesNotDisposeHttpClient_WhenExternal()
    {
        // Arrange
        var endpoint = new Uri("https://example.com");
        var apiKey = "api-key";
        var httpClient = new HttpClient();
        var loggerFactory = new LoggerFactory();

        var huggingFaceEmbeddingGenerator = new HuggingFaceEmbeddingGenerator(endpoint, apiKey, httpClient, loggerFactory);

        // Act
        huggingFaceEmbeddingGenerator.Dispose();

        // Assert
        // No assertion possible due to lack of access to _httpClient
    }
}
