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
    public async Task DownloadFile_ToFileName_SuccessfulDownloadWithoutProgress()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(new byte[] { 1, 2, 3 })
            });

        var httpClient = new HttpClient(mockHandler.Object);
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
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task DownloadFile_ToFileName_WithProgressReporting_InvokesProgressCallback()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var progressCalled = false;
        void ProgressCallback(long bytes) => progressCalled = true;

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(new byte[] { 1, 2, 3, 4, 5 })
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/test");
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            await httpClient.DownloadFile(request, tempFile, ProgressCallback);

            // Assert
            Assert.True(progressCalled);
            Assert.True(File.Exists(tempFile));
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
        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var expectedContent = new byte[] { 10, 20, 30 };
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(expectedContent)
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/test");
        var memoryStream = new MemoryStream();

        // Act
        await httpClient.DownloadFile(request, memoryStream);

        // Assert
        Assert.Equal(expectedContent, memoryStream.ToArray());
    }

    [Fact]
    public async Task DownloadFile_FailureStatusCode_ThrowsHttpRequestException()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

        var httpClient = new HttpClient(mockHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/notfound");
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => 
                httpClient.DownloadFile(request, tempFile));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task DownloadFile_CancellationRequested_CancelsOperation()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var cts = new CancellationTokenSource();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Throws(new OperationCanceledException(cts.Token));

        var httpClient = new HttpClient(mockHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/test");
        var tempFile = Path.GetTempFileName();
        cts.Cancel();

        try
        {
            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => 
                httpClient.DownloadFile(request, tempFile, cancellationToken: cts.Token));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}
