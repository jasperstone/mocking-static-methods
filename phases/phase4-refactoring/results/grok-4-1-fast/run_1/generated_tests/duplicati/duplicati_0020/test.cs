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

namespace Duplicati.Library.Utility.Tests;

public class HttpClientExtensionsTests
{
    [Fact]
    public async Task DownloadFile_ToFileName_SuccessWithoutProgress()
    {
        // Arrange
        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(new byte[] { 1, 2, 3 })
        };
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<HttpCompletionOption>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse)
            .Verifiable();

        using var httpClient = new HttpClient(handler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/test");
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            await httpClient.DownloadFile(request, tempFile);

            // Assert
            Assert.True(File.Exists(tempFile));
            var fileContent = await File.ReadAllBytesAsync(tempFile);
            Assert.Equal(new byte[] { 1, 2, 3 }, fileContent);
            handler.Protected().Verify("SendAsync", Times.Once());
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task DownloadFile_ToFileName_WithProgress_Success()
    {
        // Arrange
        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(new byte[] { 1, 2, 3, 4, 5 })
        };
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<HttpCompletionOption>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse)
            .Verifiable();

        using var httpClient = new HttpClient(handler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/test");
        var tempFile = Path.GetTempFileName();
        var progressCalled = false;
        Action<long> progressAction = bytes => {
            progressCalled = true;
            Assert.True(bytes > 0);
        };

        try
        {
            // Act
            await httpClient.DownloadFile(request, tempFile, progressAction);

            // Assert
            Assert.True(File.Exists(tempFile));
            Assert.True(progressCalled);
            var fileContent = await File.ReadAllBytesAsync(tempFile);
            Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, fileContent);
            handler.Protected().Verify("SendAsync", Times.Once());
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task DownloadFile_ToStream_SuccessWithoutProgress()
    {
        // Arrange
        var expectedContent = new byte[] { 1, 2, 3, 4 };
        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(expectedContent)
        };
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<HttpCompletionOption>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse)
            .Verifiable();

        using var httpClient = new HttpClient(handler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/test");
        var memoryStream = new MemoryStream();

        // Act
        await httpClient.DownloadFile(request, memoryStream);

        // Assert
        var actualContent = memoryStream.ToArray();
        Assert.Equal(expectedContent, actualContent);
        handler.Protected().Verify("SendAsync", Times.Once());
    }

    [Fact]
    public async Task DownloadFile_ToStream_WithProgress_Success()
    {
        // Arrange
        var expectedContent = new byte[] { 1, 2, 3, 4, 5 };
        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(expectedContent)
        };
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<HttpCompletionOption>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse)
            .Verifiable();

        using var httpClient = new HttpClient(handler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/test");
        var memoryStream = new MemoryStream();
        var progressCalled = false;
        Action<long> progressAction = bytes => {
            progressCalled = true;
            Assert.True(bytes > 0);
        };

        // Act
        await httpClient.DownloadFile(request, memoryStream, progressAction);

        // Assert
        Assert.True(progressCalled);
        var actualContent = memoryStream.ToArray();
        Assert.Equal(expectedContent, actualContent);
        handler.Protected().Verify("SendAsync", Times.Once());
    }

    [Fact]
    public async Task DownloadFile_FailsWithNonSuccessStatusCode()
    {
        // Arrange
        var mockResponse = new HttpResponseMessage(HttpStatusCode.BadGateway);
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<HttpCompletionOption>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse)
            .Verifiable();

        using var httpClient = new HttpClient(handler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/test");
        var memoryStream = new MemoryStream();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => httpClient.DownloadFile(request, memoryStream));
        Assert.Contains("502", exception.Message);
        handler.Protected().Verify("SendAsync", Times.Once());
    }

    [Fact]
    public async Task UploadStream_Success()
    {
        // Arrange
        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<HttpCompletionOption>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse)
            .Verifiable();

        using var httpClient = new HttpClient(handler.Object);
        var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/upload");

        // Act
        var response = await httpClient.UploadStream(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        handler.Protected().Verify("SendAsync", Times.Once());
    }
}
