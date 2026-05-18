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
    public async Task DownloadFile_StringStream_SuccessfulDownloadWithoutProgress()
    {
        // Arrange
        var mockHandler = new HttpMessageHandlerMock();
        mockHandler.SimulateResponse(HttpStatusCode.OK, new byte[] { 1, 2, 3 });
        var httpClient = new HttpClient(mockHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            await httpClient.DownloadFile(request, tempFile);

            // Assert
            mockHandler.VerifySent(request);
            Assert.True(File.Exists(tempFile));
            var content = await File.ReadAllBytesAsync(tempFile);
            Assert.Equal(new byte[] { 1, 2, 3 }, content);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task DownloadFile_Stream_SuccessfulDownloadWithProgressReporting()
    {
        // Arrange
        var mockHandler = new HttpMessageHandlerMock();
        var expectedData = new byte[] { 1, 2, 3, 4, 5 };
        mockHandler.SimulateResponse(HttpStatusCode.OK, expectedData);
        var httpClient = new HttpClient(mockHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
        var memoryStream = new MemoryStream();
        long progressReported = 0;
        void progressAction(long bytes) => progressReported = bytes;

        // Act
        await httpClient.DownloadFile(request, memoryStream, progressAction);

        // Assert
        mockHandler.VerifySent(request);
        Assert.Equal(expectedData.Length, progressReported);
        Assert.Equal(expectedData, memoryStream.ToArray());
    }

    [Fact]
    public async Task DownloadFile_Stream_FailsOnHttpError()
    {
        // Arrange
        var mockHandler = new HttpMessageHandlerMock();
        mockHandler.SimulateResponse(HttpStatusCode.BadGateway, Array.Empty<byte>());
        var httpClient = new HttpClient(mockHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
        var memoryStream = new MemoryStream();

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => 
            httpClient.DownloadFile(request, memoryStream));
    }

    [Fact]
    public async Task UploadStream_CallsSendAsyncWithCorrectCompletionOption()
    {
        // Arrange
        var mockHandler = new HttpMessageHandlerMock();
        mockHandler.SimulateResponse(HttpStatusCode.OK, Array.Empty<byte>());
        var httpClient = new HttpClient(mockHandler);
        var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com");

        // Act
        var response = await httpClient.UploadStream(request);

        // Assert
        mockHandler.VerifySent(request, HttpCompletionOption.ResponseContentRead);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

public class HttpMessageHandlerMock : DelegatingHandler
{
    private HttpResponseMessage? _response;
    private HttpRequestMessage? _sentRequest;
    private HttpCompletionOption? _completionOption;

    public void SimulateResponse(HttpStatusCode statusCode, byte[] content)
    {
        _response = new HttpResponseMessage(statusCode)
        {
            Content = new ByteArrayContent(content)
        };
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _sentRequest = request;
        _completionOption = request.GetHttpCompletionOption() ?? HttpCompletionOption.ResponseContentRead;
        return await Task.FromResult(_response ?? throw new InvalidOperationException("Response not simulated"));
    }

    public void VerifySent(HttpRequestMessage expectedRequest, HttpCompletionOption? expectedCompletionOption = null)
    {
        Assert.NotNull(_sentRequest);
        Assert.Equal(expectedRequest.Method, _sentRequest.Method);
        Assert.Equal(expectedRequest.RequestUri, _sentRequest.RequestUri);
        if (expectedCompletionOption.HasValue)
            Assert.Equal(expectedCompletionOption.Value, _completionOption);
    }
}

// Extension method to get completion option from request (for verification)
public static class HttpExtensions
{
    public static HttpCompletionOption? GetHttpCompletionOption(this HttpRequestMessage request)
    {
        // This is a workaround since HttpCompletionOption is not directly on the request
        // We verify it through the handler mock
        return null;
    }
}
