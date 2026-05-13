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
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
        var fileStream = new MemoryStream();

        // Act
        await client.DownloadFile(request, fileStream);

        // Assert
        Assert.Equal(new byte[] { 1, 2, 3 }, fileStream.ToArray());
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
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
        var filename = "testfile.txt";
        var progressCalled = false;

        Action<long> progressAction = (progress) =>
        {
            progressCalled = true;
        };

        // Act
        await client.DownloadFile(request, filename, progressAction);

        // Assert
        Assert.True(progressCalled);
        Assert.True(File.Exists(filename));
        var fileContent = File.ReadAllBytes(filename);
        Assert.Equal(new byte[] { 1, 2, 3 }, fileContent);
        File.Delete(filename);
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
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
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
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
        var fileStream = new MemoryStream();
        var cts = new CancellationTokenSource();
        cts.CancelAfter(100); // Cancel after 100ms

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => client.DownloadFile(request, fileStream, null, cts.Token));
    }
}
