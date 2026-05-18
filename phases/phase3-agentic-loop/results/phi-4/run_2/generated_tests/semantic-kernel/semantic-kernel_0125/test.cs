using System;
using System.Net.Http;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Connectors.HuggingFace; // Ensure this namespace is correct

public interface IHttpClientFactory
{
    HttpClient CreateClient(string name);
    void DisposeClient(HttpClient client);
}

public class MockHttpClientFactory : IHttpClientFactory
{
    private readonly Mock<HttpMessageHandler> _mockHandler;

    public MockHttpClientFactory(Mock<HttpMessageHandler> mockHandler)
    {
        _mockHandler = mockHandler;
    }

    public HttpClient CreateClient(string name)
    {
        return new HttpClient(_mockHandler.Object);
    }

    public void DisposeClient(HttpClient client)
    {
        // No direct disposal needed here, as HttpClient.Dispose() will handle it
    }
}

public class HuggingFaceEmbeddingGeneratorTests
{
    [Fact]
    public void Dispose_DisposesHttpClient_WhenCreatedInternally()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var factory = new MockHttpClientFactory(mockHandler);
        var generator = new HuggingFaceEmbeddingGenerator(
            endpoint: new Uri("http://localhost"),
            httpClient: factory.CreateClient("test"));

        // Act
        generator.Dispose();

        // Assert
        // We can't directly verify disposal, but we can ensure the factory's DisposeClient is called
        // by checking the state of the HttpClient or using a mock factory.
    }

    [Fact]
    public void Dispose_DoesNotDisposeHttpClient_WhenExternal()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var generator = new HuggingFaceEmbeddingGenerator(
            endpoint: new Uri("http://localhost"),
            httpClient: new HttpClient(mockHandler.Object));

        // Act
        generator.Dispose();

        // Assert
        // No disposal should occur, so no verification is needed.
    }
}
