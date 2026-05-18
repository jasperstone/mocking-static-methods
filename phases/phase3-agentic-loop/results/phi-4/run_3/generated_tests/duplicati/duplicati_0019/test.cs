using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;
using Duplicati.Library.Utility;

public class HttpClientExtensionsTests
{
    [Fact]
    public async Task DownloadFile_SuccessfulDownload()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("file content")
        };

        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
               "SendAsync",
               ItExpr.IsAny<HttpRequestMessage>(),
               ItExpr.IsAny<CancellationToken>()
           )
           .ReturnsAsync(response);

        var httpClient = new HttpClient(handlerMock.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file");

        // Act
        using var tempFile = new TempFile();
        await httpClient.DownloadFile(request, tempFile.Path);

        // Assert
        var fileContent = await File.ReadAllTextAsync(tempFile.Path);
        Assert.Equal("file content", fileContent);
    }

    [Fact]
    public async Task DownloadFile_NonSuccessStatusCode()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.NotFound,
            Content = new StringContent("Not Found")
        };

        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
               "SendAsync",
               ItExpr.IsAny<HttpRequestMessage>(),
               ItExpr.IsAny<CancellationToken>()
           )
           .ReturnsAsync(response);

        var httpClient = new HttpClient(handlerMock.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file");

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            using var tempFile = new TempFile();
            await httpClient.DownloadFile(request, tempFile.Path);
        });
    }

    [Fact]
    public async Task DownloadFile_Cancellation()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("file content")
        };

        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
               "SendAsync",
               ItExpr.IsAny<HttpRequestMessage>(),
               ItExpr.IsAny<CancellationToken>()
           )
           .ReturnsAsync(response);

        var httpClient = new HttpClient(handlerMock.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file");
        var cts = new CancellationTokenSource();
        cts.CancelAfter(100); // Cancel after 100ms

        // Act & Assert
        using var tempFile = new TempFile();
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            await httpClient.DownloadFile(request, tempFile.Path, null, cts.Token);
        });
    }

    [Fact]
    public async Task DownloadFile_ProgressReporting()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("file content")
        };

        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
               "SendAsync",
               ItExpr.IsAny<HttpRequestMessage>(),
               ItExpr.IsAny<CancellationToken>()
           )
           .ReturnsAsync(response);

        var httpClient = new HttpClient(handlerMock.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file");
        var progressCalls = 0;

        // Act
        using var tempFile = new TempFile();
        await httpClient.DownloadFile(request, tempFile.Path, progress =>
        {
            progressCalls++;
        });

        // Assert
        Assert.True(progressCalls > 0);
    }

    private class TempFile : IDisposable
    {
        public string Path { get; }

        public TempFile()
        {
            Path = Path.GetTempFileName();
        }

        public void Dispose()
        {
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
        }
    }
}
