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
    public async Task DownloadFile_ToFileName_SuccessfulDownload()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.Response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(new byte[] { 1, 2, 3 })
        };
        var httpClient = new HttpClient(mockHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com/file");
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
    public async Task DownloadFile_ToFileName_WithProgressReporting_ReportsProgress()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        var responseContent = new byte[1024 * 10]; // 10KB
        mockHandler.Response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(responseContent)
        };
        var httpClient = new HttpClient(mockHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com/file");
        var tempFile = Path.GetTempFileName();
        long progressReported = 0;

        try
        {
            // Act
            await httpClient.DownloadFile(request, tempFile, bytesTransferred => 
            {
                progressReported = bytesTransferred;
            });

            // Assert
            Assert.True(progressReported > 0);
            Assert.True(File.Exists(tempFile));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task DownloadFile_ToFileName_NonSuccessfulStatus_ThrowsException()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.Response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var httpClient = new HttpClient(mockHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com/notfound");
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(
                () => httpClient.DownloadFile(request, tempFile));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task DownloadFile_ToStream_SuccessfulDownload()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        var expectedContent = new byte[] { 1, 2, 3, 4, 5 };
        mockHandler.Response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(expectedContent)
        };
        var httpClient = new HttpClient(mockHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com/file");
        var memoryStream = new MemoryStream();

        // Act
        await httpClient.DownloadFile(request, memoryStream);

        // Assert
        Assert.Equal(expectedContent, memoryStream.ToArray());
    }

    [Fact]
    public async Task DownloadFile_CancellationToken_CancelsOnRequest()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.DelayBeforeResponse = TimeSpan.FromSeconds(1);
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        var httpClient = new HttpClient(mockHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com/file");
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(
                () => httpClient.DownloadFile(request, tempFile, cancellationToken: cts.Token));
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
    public HttpResponseMessage? Response { get; set; }
    public TimeSpan DelayBeforeResponse { get; set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (DelayBeforeResponse > TimeSpan.Zero)
        {
            try
            {
                await Task.Delay(DelayBeforeResponse, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw new TaskCanceledException();
            }
        }

        return Response ?? new HttpResponseMessage(HttpStatusCode.OK);
    }
}
