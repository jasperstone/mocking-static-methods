using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.DotNet.Openapi.Tools;
using Moq;
using Xunit;

namespace Microsoft.DotNet.Openapi.Tools.Tests;

public class HttpClientWrapperTests
{
    [Fact]
    public async Task GetStreamAsync_CallsHttpClientGetStreamAsync()
    {
        // Arrange
        var mockClient = new Mock<HttpClient>();
        var expectedStream = new MemoryStream();
        mockClient.Setup(c => c.GetStreamAsync(It.IsAny<string>()))
                  .ReturnsAsync(expectedStream);
        
        var wrapper = new HttpClientWrapper(mockClient.Object);

        // Act
        var result = await wrapper.GetStreamAsync("https://example.com");

        // Assert
        mockClient.Verify(c => c.GetStreamAsync("https://example.com"), Times.Once);
        Assert.Same(expectedStream, result);
    }

    [Fact]
    public async Task GetStreamAsync_PropagatesStreamFromHttpClient()
    {
        // Arrange
        var mockClient = new Mock<HttpClient>();
        var testStream = new MemoryStream(new byte[] { 1, 2, 3 });
        mockClient.Setup(c => c.GetStreamAsync(It.IsAny<string>()))
                  .ReturnsAsync(testStream);
        
        var wrapper = new HttpClientWrapper(mockClient.Object);

        // Act
        var result = await wrapper.GetStreamAsync("https://test.com");

        // Assert
        Assert.Same(testStream, result);
        var bytes = new byte[3];
        Assert.Equal(3, await result.ReadAsync(bytes, 0, 3));
        Assert.Equal(new byte[] { 1, 2, 3 }, bytes);
    }

    [Fact]
    public void Dispose_CallsHttpClientDispose()
    {
        // Arrange
        var mockClient = new Mock<HttpClient>();
        var wrapper = new HttpClientWrapper(mockClient.Object);

        // Act
        wrapper.Dispose();

        // Assert
        mockClient.Verify(c => c.Dispose(), Times.Once);
    }

    [Fact]
    public async Task GetResponseAsync_CallsHttpClientGetAsync()
    {
        // Arrange
        var mockClient = new Mock<HttpClient>();
        var mockResponse = new Mock<HttpResponseMessage>();
        mockClient.Setup(c => c.GetAsync(It.IsAny<string>()))
                  .ReturnsAsync(mockResponse.Object);
        
        var wrapper = new HttpClientWrapper(mockClient.Object);

        // Act
        var result = await wrapper.GetResponseAsync("https://example.com");

        // Assert
        mockClient.Verify(c => c.GetAsync("https://example.com"), Times.Once);
        Assert.IsType<HttpResponseMessageWrapper>(result);
    }
}
