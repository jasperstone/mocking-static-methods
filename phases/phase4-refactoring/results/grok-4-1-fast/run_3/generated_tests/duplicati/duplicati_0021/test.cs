using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Utility;
using Xunit;

public class HttpClientExtensionsTests
{
    [Fact]
    public async Task DownloadFile_Stream_NoProgress_Success()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.Response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(new byte[] { 1, 2, 3 })
        };
        var httpClient = new HttpClient(mockHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
        var memoryStream = new MemoryStream();

        // Act
        await httpClient.DownloadFile(request, memoryStream);

        // Assert
        Assert.True(memoryStream.Position > 0);
        var content = memoryStream.ToArray();
        Assert.Equal(new byte[] { 1, 2, 3 }, content);
    }

    [Fact]
    public async Task DownloadFile_Stream_WithProgress_Success()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        var responseContent = new byte[1024];
        new Random().NextBytes(responseContent);
        mockHandler.Response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(responseContent)
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
        Assert.Equal(responseContent.Length, memoryStream.Length);
    }

    [Fact]
    public async Task DownloadFile_Stream_FailureStatusCode_Throws()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.Response = new HttpResponseMessage(System.Net.HttpStatusCode.BadGateway);
        var httpClient = new HttpClient(mockHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
        var memoryStream = new MemoryStream();

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => httpClient.DownloadFile(request, memoryStream));
    }

    [Fact]
    public async Task UploadStream_Success()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.Response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        var httpClient = new HttpClient(mockHandler);
        var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com")
        {
            Content = new ByteArrayContent(new byte[] { 4, 5, 6 })
        };

        // Act
        var response = await httpClient.UploadStream(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    private class MockHttpMessageHandler : DelegatingHandler
    {
        public HttpResponseMessage? Response { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Response ?? throw new InvalidOperationException("Response not set"));
        }
    }
}
