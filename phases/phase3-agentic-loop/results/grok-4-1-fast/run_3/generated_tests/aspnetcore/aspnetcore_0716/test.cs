using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.OpenApi;
using Microsoft.DotNet.Openapi.Tools;
using Moq;
using Xunit;

namespace Microsoft.DotNet.Openapi.Tools.Tests;

public class HttpClientWrapperTests
{
    [Fact]
    public void Constructor_WithValidHttpClient_Succeeds()
    {
        // Arrange
        using var httpClient = new HttpClient();

        // Act
        var wrapper = new HttpClientWrapper(httpClient);

        // Assert
        Assert.NotNull(wrapper);
    }

    [Fact]
    public void Dispose_CallsDisposeOnClient()
    {
        // Arrange
        var mockClient = new Mock<HttpClient>();
        mockClient.Setup(c => c.Dispose());
        var wrapper = new HttpClientWrapper(mockClient.Object);

        // Act
        wrapper.Dispose();

        // Assert
        mockClient.Verify(c => c.Dispose(), Times.Once);
    }

    [Fact]
    public async Task GetResponseAsync_ValidUrl_CallsGetAsync()
    {
        // Arrange
        var mockClient = new Mock<HttpClient>();
        var mockResponse = new HttpResponseMessage();
        mockClient.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(mockResponse);

        var wrapper = new HttpClientWrapper(mockClient.Object);

        // Act
        var result = await wrapper.GetResponseAsync("https://example.com");

        // Assert
        mockClient.Verify(c => c.GetAsync("https://example.com", It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetStreamAsync_ValidUrl_CallsGetStreamAsync()
    {
        // Arrange
        var mockClient = new Mock<HttpClient>();
        var mockStream = new MemoryStream();
        mockClient.Setup(c => c.GetStreamAsync(It.IsAny<string>()))
                  .ReturnsAsync(mockStream);

        var wrapper = new HttpClientWrapper(mockClient.Object);

        // Act
        var result = await wrapper.GetStreamAsync("https://example.com");

        // Assert
        mockClient.Verify(c => c.GetStreamAsync("https://example.com"), Times.Once);
        Assert.Same(mockStream, result);
    }

    [Fact]
    public async Task GetStreamAsync_NullUrl_ThrowsArgumentNullException()
    {
        // Arrange
        using var httpClient = new HttpClient();
        var wrapper = new HttpClientWrapper(httpClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => wrapper.GetStreamAsync(null!));
        Assert.Equal("url", exception.ParamName);
    }
}
