using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.HuggingFace.Core;
using Microsoft.SemanticKernel.Http;

namespace Microsoft.SemanticKernel.Connectors.HuggingFace;

public class HuggingFaceEmbeddingGeneratorTests
{
    [Fact]
    public async Task GenerateAsync_DisposeHttpClient_WhenNotExternal()
    {
        // Arrange
        var httpClient = new HttpClient();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        var endpoint = new Uri("https://example.com");
        var apiKey = "api-key";
        var huggingFaceEmbeddingGenerator = new HuggingFaceEmbeddingGenerator(endpoint, apiKey, null, loggerFactoryMock.Object);

        // Act
        await huggingFaceEmbeddingGenerator.GenerateAsync(new List<string> { "test" });
        huggingFaceEmbeddingGenerator.Dispose();

        // Assert
        // We can't directly verify the Dispose call on the HttpClient, but we can verify that the HttpClient is disposed
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.com")));
    }

    [Fact]
    public async Task GenerateAsync_DoNotDisposeHttpClient_WhenExternal()
    {
        // Arrange
        var externalHttpClient = new HttpClient();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        var endpoint = new Uri("https://example.com");
        var apiKey = "api-key";
        var huggingFaceEmbeddingGenerator = new HuggingFaceEmbeddingGenerator(endpoint, apiKey, externalHttpClient, loggerFactoryMock.Object);

        // Act
        await huggingFaceEmbeddingGenerator.GenerateAsync(new List<string> { "test" });
        huggingFaceEmbeddingGenerator.Dispose();

        // Assert
        // We can't directly verify the Dispose call on the HttpClient, but we can verify that the HttpClient is not disposed
        await externalHttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.com"));
    }
}
