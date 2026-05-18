using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Utility;
using Moq;
using Moq.Protected;
using Xunit;

public class HttpClientExtensionsTests
{
    [Fact]
    public async Task DownloadFile_ShouldDownloadFileSuccessfully()
    {
        // Arrange
        var httpClientHandlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("Test content")
        };
        httpClientHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        var httpClient = new HttpClient(httpClientHandlerMock.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
        var tempFilePath = Path.GetTempFileName();

        // Act
        await httpClient.DownloadFile(request, tempFilePath);

        // Assert
        Assert.True(File.Exists(tempFilePath));
        var fileContent = await File.ReadAllTextAsync(tempFilePath);
        Assert.Equal("Test content", fileContent);

        // Clean up
        File.Delete(tempFilePath);
    }

    [Fact]
    public async Task DownloadFile_ShouldThrowExceptionOnFailure()
    {
        // Arrange
        var httpClientHandlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        httpClientHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        var httpClient = new HttpClient(httpClientHandlerMock.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
        var tempFilePath = Path.GetTempFileName();

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => httpClient.DownloadFile(request, tempFilePath));

        // Clean up
        if (File.Exists(tempFilePath))
        {
            File.Delete(tempFilePath);
        }
    }

    [Fact]
    public async Task DownloadFile_ShouldReportProgress()
    {
        // Arrange
        var httpClientHandlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("Test content")
        };
        httpClientHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        var httpClient = new HttpClient(httpClientHandlerMock.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
        var tempFilePath = Path.GetTempFileName();
        long reportedProgress = 0;
        Action<long> progressReportingAction = progress => reportedProgress = progress;

        // Act
        await httpClient.DownloadFile(request, tempFilePath, progressReportingAction);

        // Assert
        Assert.True(reportedProgress > 0);

        // Clean up
        if (File.Exists(tempFilePath))
        {
            File.Delete(tempFilePath);
        }
    }
}
