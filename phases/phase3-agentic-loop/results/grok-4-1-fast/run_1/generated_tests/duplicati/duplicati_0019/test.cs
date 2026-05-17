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
using System.Collections.Concurrent;

namespace Duplicati.Library.Utility.Tests;

public class HttpClientExtensionsTests
{
    [Fact]
    public async Task DownloadFile_ToFileName_SuccessfulDownloadWithoutProgress()
    {
        // Arrange
        var mockMessageHandler = new Mock<HttpMessageHandler>();
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(new byte[] { 1, 2, 3 })
            });

        using var httpClient = new HttpClient(mockMessageHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com/file");
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            await httpClient.DownloadFile(request, tempFile);

            // Assert
            mockMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());

            Assert.True(File.Exists(tempFile));
            var fileContent = await File.ReadAllBytesAsync(tempFile);
            Assert.Equal(new byte[] { 1, 2, 3 }, fileContent);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task DownloadFile_ToFileName_WithProgressReporting()
    {
        // Arrange
        var progressValues = new ConcurrentBag<long>();
        Action<long> progressAction = l => progressValues.Add(l);

        var mockMessageHandler = new Mock<HttpMessageHandler>();
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(new byte[] { 1, 2, 3, 4, 5 })
            });

        using var httpClient = new HttpClient(mockMessageHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com/file");
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            await httpClient.DownloadFile(request, tempFile, progressAction);

            // Assert
            mockMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());

            Assert.True(File.Exists(tempFile));
            var fileContent = await File.ReadAllBytesAsync(tempFile);
            Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, fileContent);
            Assert.NotEmpty(progressValues);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task DownloadFile_ToStream_SuccessfulDownloadWithoutProgress()
    {
        // Arrange
        var mockMessageHandler = new Mock<HttpMessageHandler>();
        var expectedContent = new byte[] { 1, 2, 3, 4 };
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(expectedContent)
            });

        using var httpClient = new HttpClient(mockMessageHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com/file");
        var memoryStream = new MemoryStream();

        // Act
        await httpClient.DownloadFile(request, memoryStream);

        // Assert
        mockMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());

        Assert.Equal(expectedContent, memoryStream.ToArray());
    }

    [Fact]
    public async Task DownloadFile_FailsOnNonSuccessStatusCode()
    {
        // Arrange
        var mockMessageHandler = new Mock<HttpMessageHandler>();
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

        using var httpClient = new HttpClient(mockMessageHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com/notfound");
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpRequestException>(
                () => httpClient.DownloadFile(request, tempFile));

            mockMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task UploadStream_CallsSendAsyncWithResponseContentRead()
    {
        // Arrange
        var mockMessageHandler = new Mock<HttpMessageHandler>();
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        using var httpClient = new HttpClient(mockMessageHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com/upload");

        // Act
        await httpClient.UploadStream(request);

        // Assert
        mockMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }
}
