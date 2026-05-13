using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.DotNet.OpenApi;
using Microsoft.DotNet.OpenApi.Tools;
using Moq;
using Xunit;

namespace Microsoft.DotNet.Openapi.Tools.Tests;

public class HttpClientWrapperTests
{
    [Fact]
    public async Task GetStreamAsync_CallsHttpClientGetStreamAsync()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var httpClientWrapper = new HttpClientWrapper(httpClientMock.Object);
        var url = "https://example.com";

        var stream = new MemoryStream();
        httpClientMock.Setup(c => c.GetStreamAsync(url)).ReturnsAsync(stream);

        // Act
        var result = await httpClientWrapper.GetStreamAsync(url);

        // Assert
        Assert.Same(stream, result);
    }

    [Fact]
    public async Task GetResponseAsync_CallsHttpClientGetAsync()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var httpClientWrapper = new HttpClientWrapper(httpClientMock.Object);
        var url = "https://example.com";

        var response = new HttpResponseMessage(HttpStatusCode.OK);
        httpClientMock.Setup(c => c.GetAsync(url)).ReturnsAsync(response);

        // Act
        var result = await httpClientWrapper.GetResponseAsync(url);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void Dispose_CallsHttpClientDispose()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var httpClientWrapper = new HttpClientWrapper(httpClientMock.Object);

        // Act
        httpClientWrapper.Dispose();

        // Assert
        httpClientMock.Verify(c => c.Dispose(), Times.Once);
    }
}
