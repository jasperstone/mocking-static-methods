using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Moq.Protected;
using Duplicati.Library.Utility;

public class HttpClientExtensionsTests
{
    [Fact]
    public async Task DownloadFile_ShouldDownloadFileSuccessfully()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file");
        var filename = "testfile.txt";

        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("Test content")
        };

        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        await HttpClientExtensions.DownloadFile(httpClient, request, filename);

        // Assert
        Assert.True(File.Exists(filename));
        var content = await File.ReadAllTextAsync(filename);
        Assert.Equal("Test content", content);
    }

    [Fact]
    public async Task DownloadFile_ShouldThrowExceptionOnFailure()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file");
        var filename = "testfile.txt";

        var response = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);

        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => HttpClientExtensions.DownloadFile(httpClient, request, filename));
    }
}
