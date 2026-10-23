using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Utility;
using Xunit;

namespace Duplicati.Library.Utility.Tests;

public class HttpClientExtensionsTests
{
    [Fact]
    public async Task DownloadFile_Stream_WithoutProgress_Success()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.Response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(new byte[] { 1, 2, 3 })
        };
        var httpClient = new HttpClient(mockHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
        var memoryStream = new MemoryStream();

        // Act
        await httpClient.DownloadFile(request, memoryStream);

        // Assert
        Assert.True(memoryStream.Length > 0);
        var bytes = memoryStream.ToArray();
        Assert.Equal(new byte[] { 1, 2, 3 }, bytes);
    }

    [Fact]
    public async Task DownloadFile_Stream_WithProgress_Success()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        var content = new byte[1024];
        new Random().NextBytes(content);
        mockHandler.Response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(content)
        };
        var httpClient = new HttpClient(mockHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
        var memoryStream = new MemoryStream();
        long progressReported = 0;
        Action<long> progressAction = bytes => progressReported = bytes;

        // Act
        await httpClient.DownloadFile(request, memoryStream, progressAction);

        // Assert
        Assert.True(progressReported > 0);
        Assert.Equal(content.Length, memoryStream.Length);
    }

    [Fact]
    public async Task DownloadFile_Stream_FailureStatusCode_Throws()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.Response = new HttpResponseMessage(HttpStatusCode.BadGateway);
        var httpClient = new HttpClient(mockHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
        var memoryStream = new MemoryStream();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => httpClient.DownloadFile(request, memoryStream));
        Assert.NotNull(ex);
    }

    [Fact]
    public async Task UploadStream_Success_ReturnsResponse()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.Response = new HttpResponseMessage(HttpStatusCode.OK);
        var httpClient = new HttpClient(mockHandler);
        var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com")
        {
            Content = new ByteArrayContent(new byte[] { 4, 5, 6 })
        };

        // Act
        var response = await httpClient.UploadStream(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UploadStream_WithCancellationToken_Cancels()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.Response = new HttpResponseMessage(HttpStatusCode.OK);
        var cts = new CancellationTokenSource();
        var httpClient = new HttpClient(mockHandler);
        var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com");

        // Act & Assert
        cts.Cancel();
        var ex = await Assert.ThrowsAnyAsync<Exception>(
            () => httpClient.UploadStream(request, cts.Token));
        Assert.NotNull(ex);
        Assert.True(cts.Token.IsCancellationRequested);
    }
}

public class MockHttpMessageHandler : DelegatingHandler
{
    public HttpResponseMessage? Response { get; set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Response ?? throw new InvalidOperationException("Response not set"));
    }
}
