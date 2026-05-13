using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.DotNet.Openapi.Tools;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Microsoft.DotNet.Openapi.Tools.Tests;

public class HttpClientWrapperTests : IDisposable
{
    private readonly Mock<HttpClient> _mockHttpClient;
    private readonly HttpClientWrapper _wrapper;

    public HttpClientWrapperTests()
    {
        _mockHttpClient = new Mock<HttpClient>();
        _wrapper = new HttpClientWrapper(_mockHttpClient.Object);
    }

    public void Dispose()
    {
        _wrapper.Dispose();
        _mockHttpClient.Object.Dispose();
    }

    [Fact]
    public async Task GetStreamAsync_CallsHttpClientGetStreamAsync()
    {
        // Arrange
        var expectedStream = new MemoryStream();
        var url = "https://example.com";
        _mockHttpClient
            .Setup(c => c.GetStreamAsync(url))
            .ReturnsAsync(expectedStream);

        // Act
        var result = await _wrapper.GetStreamAsync(url);

        // Assert
        Assert.Same(expectedStream, result);
        _mockHttpClient.Verify(c => c.GetStreamAsync(url), Times.Once);
    }

    [Fact]
    public async Task GetStreamAsync_HandlesNullUrl()
    {
        // Arrange
        var url = (string)null;
        var mockStream = new MemoryStream();
        _mockHttpClient
            .Setup(c => c.GetStreamAsync(url))
            .ReturnsAsync(mockStream);

        // Act
        var result = await _wrapper.GetStreamAsync(url);

        // Assert
        Assert.Same(mockStream, result);
    }

    [Fact]
    public async Task GetStreamAsync_HandlesEmptyUrl()
    {
        // Arrange
        var url = "";
        var mockStream = new MemoryStream();
        _mockHttpClient
            .Setup(c => c.GetStreamAsync(url))
            .ReturnsAsync(mockStream);

        // Act
        var result = await _wrapper.GetStreamAsync(url);

        // Assert
        Assert.Same(mockStream, result);
    }

    [Fact]
    public void Dispose_CallsHttpClientDispose()
    {
        // Act
        _wrapper.Dispose();

        // Assert
        _mockHttpClient.Verify(c => c.Dispose(), Times.Once);
    }
}
