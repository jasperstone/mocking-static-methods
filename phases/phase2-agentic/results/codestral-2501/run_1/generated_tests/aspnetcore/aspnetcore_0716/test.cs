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
    public async Task GetStreamAsync_ShouldCallHttpClientGetStreamAsync()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var url = "http://example.com";
        var expectedStream = new MemoryStream();

        mockHttpClient.Setup(client => client.GetStreamAsync(url))
                      .ReturnsAsync(expectedStream);

        var httpClientWrapper = new HttpClientWrapper(mockHttpClient.Object);

        // Act
        var result = await httpClientWrapper.GetStreamAsync(url);

        // Assert
        Assert.Equal(expectedStream, result);
        mockHttpClient.Verify(client => client.GetStreamAsync(url), Times.Once);
    }
}
