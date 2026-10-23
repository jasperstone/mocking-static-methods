using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
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
    public async Task UploadStream_CallsSendAsyncWithResponseContentRead()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<HttpCompletionOption>(), 
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
            .Verifiable();

        var httpClient = new HttpClient(mockHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act
        var result = await httpClient.UploadStream(request);

        // Assert
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req == request),
                HttpCompletionOption.ResponseContentRead,
                ItExpr.IsAny<CancellationToken>());
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task UploadStream_PropagatesCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<HttpCompletionOption>(), 
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException(cts.Token));

        var httpClient = new HttpClient(mockHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            httpClient.UploadStream(request, cts.Token));
    }

    [Fact]
    public async Task DownloadFile_StreamWithoutProgress_CopiesContent()
    {
        // Arrange
        var expectedContent = "test content";
        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(expectedContent)
        };

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<HttpCompletionOption>(), 
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);

        var httpClient = new HttpClient(mockHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        var memoryStream = new MemoryStream();

        // Act
        await httpClient.DownloadFile(request, memoryStream);

        // Assert
        memoryStream.Position = 0;
        var result = await new StreamReader(memoryStream, Encoding.UTF8).ReadToEndAsync();
        Assert.Equal(expectedContent, result);
    }

    [Fact]
    public async Task DownloadFile_WithProgress_ReportsProgress()
    {
        // Arrange
        var progressCalled = false;
        Action<long> progressAction = bytes => {
            progressCalled = true;
        };

        var contentStream = new MemoryStream(Encoding.UTF8.GetBytes("test"));
        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(contentStream)
        };

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<HttpCompletionOption>(), 
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);

        var httpClient = new HttpClient(mockHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        var memoryStream = new MemoryStream();

        // Act
        await httpClient.DownloadFile(request, memoryStream, progressAction);

        // Assert
        Assert.True(progressCalled);
    }

    [Fact]
    public async Task DownloadFile_FailsOnNonSuccessStatus()
    {
        // Arrange
        var mockResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<HttpCompletionOption>(), 
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);

        var httpClient = new HttpClient(mockHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        var memoryStream = new MemoryStream();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => 
            httpClient.DownloadFile(request, memoryStream));
        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
    }
}
