using System;
using System.Net.Http;
using Moq;
using Moq.Protected;
using Xunit;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using System.Linq.Expressions;

public class HuggingFaceEmbeddingGeneratorTests
{
    [Fact]
    public void Dispose_DisposesHttpMessageHandler_WhenNotExternal()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var generator = new HuggingFaceEmbeddingGenerator(
            endpoint: new Uri("http://localhost"),
            httpClient: httpClient);

        // Act
        generator.Dispose();

        // Assert
        mockHttpMessageHandler.Protected()
            .Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
    }

    [Fact]
    public void Dispose_DoesNotDisposeHttpMessageHandler_WhenExternal()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var generator = new HuggingFaceEmbeddingGenerator(
            endpoint: new Uri("http://localhost"),
            httpClient: httpClient);

        // Act
        generator.Dispose();

        // Assert
        mockHttpMessageHandler.Protected()
            .Verify("Dispose", Times.Never(), ItExpr.IsAny<bool>());
    }
}
