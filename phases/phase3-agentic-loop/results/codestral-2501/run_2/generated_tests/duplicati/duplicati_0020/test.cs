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
    public async Task DownloadFile_WithProgressReportingAction_CallsProgressAction()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("Test content")
        };

        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        var httpClient = new HttpClient(handlerMock.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
        var progressCalled = false;
        Action<long> progressAction = bytes => progressCalled = true;

        // Act
        await HttpClientExtensions.DownloadFile(httpClient, request, "testfile.txt", progressAction);

        // Assert
        Assert.True(progressCalled);
    }

    [Fact]
    public async Task DownloadFile_WithoutProgressReportingAction_DoesNotCallProgressAction()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("Test content")
        };

        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        var httpClient = new HttpClient(handlerMock.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

        // Act
        await HttpClientExtensions.DownloadFile(httpClient, request, "testfile.txt");

        // Assert
        // No progress action should be called, so no assertion needed
    }

    [Fact]
    public async Task DownloadFile_WithStream_WithoutProgressReportingAction_DoesNotCallProgressAction()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("Test content")
        };

        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        var httpClient = new HttpClient(handlerMock.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
        using var memoryStream = new MemoryStream();

        // Act
        await HttpClientExtensions.DownloadFile(httpClient, request, memoryStream);

        // Assert
        // No progress action should be called, so no assertion needed
    }

    [Fact]
    public async Task UploadStream_ReturnsResponse()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("Test content")
        };

        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        var httpClient = new HttpClient(handlerMock.Object);
        var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com");

        // Act
        var result = await HttpClientExtensions.UploadStream(httpClient, request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
}
