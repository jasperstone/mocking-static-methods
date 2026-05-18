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
    public async Task DownloadFile_WithFileName_SuccessfulDownload()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<HttpCompletionOption>(),
                ItExpr.IsAny<IWebProxy>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(new byte[] { 1, 2, 3 })
            });

        using var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/test");
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            await httpClient.DownloadFile(request, tempFile);

            // Assert
            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                HttpCompletionOption.ResponseHeadersRead,
                ItExpr.IsAny<IWebProxy>(),
                ItExpr.IsAny<CancellationToken>());

            Assert.True(File.Exists(tempFile));
            var content = await File.ReadAllBytesAsync(tempFile);
            Assert.Equal(new byte[] { 1, 2, 3 }, content);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task DownloadFile_WithFileStream_SuccessfulDownload()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var expectedContent = new byte[] { 1, 2, 3, 4, 5 };
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<HttpCompletionOption>(),
                ItExpr.IsAny<IWebProxy>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(expectedContent)
            });

        using var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/test");
        using var memoryStream = new MemoryStream();

        // Act
        await httpClient.DownloadFile(request, memoryStream);

        // Assert
        mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            HttpCompletionOption.ResponseHeadersRead,
            ItExpr.IsAny<IWebProxy>(),
            ItExpr.IsAny<CancellationToken>());

        Assert.Equal(expectedContent, memoryStream.ToArray());
    }

    [Fact]
    public async Task DownloadFile_WithFileStream_ProgressReporting_SuccessfulDownload()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var expectedContent = new byte[] { 1, 2, 3, 4, 5 };
        long progressReported = 0;
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<HttpCompletionOption>(),
                ItExpr.IsAny<IWebProxy>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(expectedContent)
            });

        using var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/test");
        using var memoryStream = new MemoryStream();

        // Act
        await httpClient.DownloadFile(request, memoryStream, bytes => progressReported = bytes);

        // Assert
        mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            HttpCompletionOption.ResponseHeadersRead,
            ItExpr.IsAny<IWebProxy>(),
            ItExpr.IsAny<CancellationToken>());

        Assert.True(progressReported > 0);
        Assert.Equal(expectedContent, memoryStream.ToArray());
    }

    [Fact]
    public async Task UploadStream_CallsSendAsyncWithResponseContentRead()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<HttpCompletionOption>(),
                ItExpr.IsAny<IWebProxy>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        using var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://example.com/upload")
        {
            Content = new ByteArrayContent(new byte[] { 1, 2, 3 })
        };

        // Act
        await httpClient.UploadStream(request);

        // Assert
        mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            HttpCompletionOption.ResponseContentRead,
            ItExpr.IsAny<IWebProxy>(),
            ItExpr.IsAny<CancellationToken>());
    }
}
