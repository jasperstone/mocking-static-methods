using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Utility;
using Xunit;
using Moq;
using Moq.Protected;

public class HttpClientExtensionsTests
{
    [Fact]
    public async Task DownloadFile_ShouldDownloadFileSuccessfully()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("Test content")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/file");
        var tempFilePath = Path.GetTempFileName();

        // Act
        await HttpClientExtensions.DownloadFile(httpClient, request, tempFilePath);

        // Assert
        Assert.True(File.Exists(tempFilePath));
        File.Delete(tempFilePath);
    }

    [Fact]
    public async Task DownloadFile_ShouldDownloadFileWithProgressReporting()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("Test content")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/file");
        var tempFilePath = Path.GetTempFileName();
        long reportedProgress = 0;
        Action<long> progressReportingAction = progress => reportedProgress = progress;

        // Act
        await HttpClientExtensions.DownloadFile(httpClient, request, tempFilePath, progressReportingAction);

        // Assert
        Assert.True(File.Exists(tempFilePath));
        Assert.True(reportedProgress > 0);
        File.Delete(tempFilePath);
    }

    [Fact]
    public async Task DownloadFile_ShouldDownloadFileToStream()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("Test content")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/file");
        using var memoryStream = new MemoryStream();

        // Act
        await HttpClientExtensions.DownloadFile(httpClient, request, memoryStream);

        // Assert
        Assert.True(memoryStream.Length > 0);
    }

    [Fact]
    public async Task DownloadFile_ShouldDownloadFileToStreamWithProgressReporting()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("Test content")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/file");
        using var memoryStream = new MemoryStream();
        long reportedProgress = 0;
        Action<long> progressReportingAction = progress => reportedProgress = progress;

        // Act
        await HttpClientExtensions.DownloadFile(httpClient, request, memoryStream, progressReportingAction);

        // Assert
        Assert.True(memoryStream.Length > 0);
        Assert.True(reportedProgress > 0);
    }

    [Fact]
    public async Task UploadStream_ShouldUploadStreamSuccessfully()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://example.com/upload");
        using var content = new StringContent("Test content");
        request.Content = content;

        // Act
        var response = await HttpClientExtensions.UploadStream(httpClient, request);

        // Assert
        response.EnsureSuccessStatusCode();
    }
}
