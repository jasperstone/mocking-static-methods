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
        _wrapper?.Dispose();
    }

    [Fact]
    public async Task GetStreamAsync_CallsHttpClientGetStreamAsync_ReturnsStream()
    {
        // Arrange
        var expectedStream = new MemoryStream();
        _mockHttpClient.Setup(x => x.GetStreamAsync(It.IsAny<string>())).ReturnsAsync(expectedStream);

        // Act
        var result = await _wrapper.GetStreamAsync("https://example.com");

        // Assert
        Assert.Same(expectedStream, result);
        _mockHttpClient.Verify(x => x.GetStreamAsync("https://example.com"), Times.Once);
    }

    [Fact]
    public async Task GetStreamAsync_WhenHttpClientThrows_PropagatesException()
    {
        // Arrange
        var expectedException = new HttpRequestException("Test");
        _mockHttpClient.Setup(x => x.GetStreamAsync(It.IsAny<string>())).ThrowsAsync(expectedException);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => _wrapper.GetStreamAsync("https://example.com"));
        Assert.Equal("Test", ex.Message);
    }

    [Fact]
    public void Dispose_CallsHttpClientDispose()
    {
        // Act
        _wrapper.Dispose();

        // Assert
        _mockHttpClient.Verify(x => x.Dispose(), Times.Once);
    }
}
