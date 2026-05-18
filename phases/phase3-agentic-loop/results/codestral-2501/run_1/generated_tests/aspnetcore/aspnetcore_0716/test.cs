using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.DotNet.OpenApi;
using Microsoft.DotNet.Openapi.Tools;
using Moq;
using Xunit;

public class HttpClientWrapperTests
{
    [Fact]
    public async Task GetStreamAsync_ShouldReturnStream()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var expectedStream = new MemoryStream();
        mockHttpClient.Setup(client => client.GetStreamAsync(It.IsAny<string>()))
                      .ReturnsAsync(expectedStream);

        var wrapper = new HttpClientWrapper(mockHttpClient.Object);

        // Act
        var result = await wrapper.GetStreamAsync("http://example.com");

        // Assert
        Assert.Equal(expectedStream, result);
    }

    [Fact]
    public async Task GetResponseAsync_ShouldReturnHttpResponseMessageWrapper()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var mockHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
        mockHttpClient.Setup(client => client.GetAsync(It.IsAny<string>()))
                      .ReturnsAsync(mockHttpResponseMessage);

        var wrapper = new HttpClientWrapper(mockHttpClient.Object);

        // Act
        var result = await wrapper.GetResponseAsync("http://example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
}
