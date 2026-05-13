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
    public async Task DownloadFile_WithFileStream_Success()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3 }))
        };
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        var client = new HttpClient(mockHttpMessageHandler.Object);
        var request = new HttpRequestMessage();
        var fileStream = new MemoryStream();

        // Act
        await client.DownloadFile(request, fileStream);

        // Assert
        Assert.Equal(3, fileStream.Length);
    }

    [Fact]
    public async Task DownloadFile_WithFileNameAndProgress_Success()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3 }))
        };
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        var client = new HttpClient(mockHttpMessageHandler.Object);
        var request = new HttpRequestMessage();
        var progressCalled = false;
        Action<long> progressAction = _ => progressCalled = true;

        // Act
        await client.DownloadFile(request, "testfile", progressAction);

        // Assert
        Assert.True(progressCalled);
        Assert.True(File.Exists("testfile"));
        Assert.Equal(3, new FileInfo("testfile").Length);
        File.Delete("testfile");
    }

    [Fact]
    public async Task DownloadFile_NonSuccessStatusCode_Throws()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.BadRequest
        };
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        var client = new HttpClient(mockHttpMessageHandler.Object);
        var request = new HttpRequestMessage();
        var fileStream = new MemoryStream();

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => client.DownloadFile(request, fileStream));
    }

    [Fact]
    public async Task DownloadFile_Cancellation_Cancels()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3 }))
        };
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        var client = new HttpClient(mockHttpMessageHandler.Object);
        var request = new HttpRequestMessage();
        var fileStream = new MemoryStream();
        var cts = new CancellationTokenSource();
        cts.CancelAfter(100); // Cancel after 100ms

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => client.DownloadFile(request, fileStream, null, cts.Token));
    }
}
