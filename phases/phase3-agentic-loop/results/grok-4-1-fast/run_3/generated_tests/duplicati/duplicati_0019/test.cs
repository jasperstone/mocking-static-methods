using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Utility;
using Xunit;

public class HttpClientExtensionsTests
{
    [Fact]
    public async Task DownloadFile_ToFilename_SuccessfulDownload_CallsSendAsync()
    {
        // Arrange
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        mockHttpMessageHandler.SimulateResponse(HttpStatusCode.OK, new byte[] { 1, 2, 3 });
        var httpClient = new HttpClient(mockHttpMessageHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/test");

        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            await httpClient.DownloadFile(request, tempFile);

            // Assert
            Assert.True(mockHttpMessageHandler.SendAsyncCalled);
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
    public async Task DownloadFile_ToFilename_WithProgressReporting_CallsSendAsync()
    {
        // Arrange
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        mockHttpMessageHandler.SimulateResponse(HttpStatusCode.OK, new byte[] { 1, 2, 3, 4, 5 });
        var httpClient = new HttpClient(mockHttpMessageHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/test");
        var progressCalled = false;
        void ProgressCallback(long bytes) => progressCalled = true;

        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            await httpClient.DownloadFile(request, tempFile, ProgressCallback);

            // Assert
            Assert.True(mockHttpMessageHandler.SendAsyncCalled);
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
    public async Task DownloadFile_ToStream_SuccessfulDownload_CallsSendAsync()
    {
        // Arrange
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        mockHttpMessageHandler.SimulateResponse(HttpStatusCode.OK, new byte[] { 1, 2, 3 });
        var httpClient = new HttpClient(mockHttpMessageHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/test");
        var memoryStream = new MemoryStream();

        // Act
        await httpClient.DownloadFile(request, memoryStream);

        // Assert
        Assert.True(mockHttpMessageHandler.SendAsyncCalled);
        Assert.Equal(new byte[] { 1, 2, 3 }, memoryStream.ToArray());
    }

    [Fact]
    public async Task DownloadFile_ToStream_WithProgressReporting_CallsSendAsync()
    {
        // Arrange
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        mockHttpMessageHandler.SimulateResponse(HttpStatusCode.OK, new byte[] { 1, 2, 3, 4, 5 });
        var httpClient = new HttpClient(mockHttpMessageHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/test");
        var memoryStream = new MemoryStream();
        var progressCalled = false;
        void ProgressCallback(long bytes) => progressCalled = true;

        // Act
        await httpClient.DownloadFile(request, memoryStream, ProgressCallback);

        // Assert
        Assert.True(mockHttpMessageHandler.SendAsyncCalled);
        Assert.True(progressCalled);
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, memoryStream.ToArray());
    }

    [Fact]
    public async Task DownloadFile_NonSuccessStatus_ThrowsHttpRequestException()
    {
        // Arrange
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        mockHttpMessageHandler.SimulateResponse(HttpStatusCode.NotFound, Array.Empty<byte>());
        var httpClient = new HttpClient(mockHttpMessageHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/notfound");
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => httpClient.DownloadFile(request, tempFile));
            Assert.True(mockHttpMessageHandler.SendAsyncCalled);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}

public class MockHttpMessageHandler : DelegatingHandler
{
    public bool SendAsyncCalled { get; private set; }
    private HttpResponseMessage? _response;

    public void SimulateResponse(HttpStatusCode statusCode, byte[] content)
    {
        _response = new HttpResponseMessage(statusCode)
        {
            Content = new ByteArrayContent(content)
        };
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        SendAsyncCalled = true;
        return Task.FromResult(_response ?? throw new InvalidOperationException("Response not simulated"));
    }
}
