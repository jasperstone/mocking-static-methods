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

public class HttpClientExtensionsTests : IDisposable
{
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly HttpClient _httpClient;
    private readonly HttpRequestMessage _request;

    public HttpClientExtensionsTests()
    {
        _handlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_handlerMock.Object);
        _request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/test");
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    private void SetupSuccessResponse(byte[] content = null)
    {
        content ??= new byte[] { 1, 2, 3 };
        var responseContent = new ByteArrayContent(content);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<HttpCompletionOption>(),
                ItExpr.IsAny<IWebProxy>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    private void SetupErrorResponse(HttpStatusCode statusCode)
    {
        var response = new HttpResponseMessage(statusCode);
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<HttpCompletionOption>(),
                ItExpr.IsAny<IWebProxy>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    [Fact]
    public async Task DownloadFile_ToFilename_WithoutProgress_Success()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            SetupSuccessResponse(new byte[] { 1, 2, 3, 4, 5 });

            // Act
            await _httpClient.DownloadFile(_request, tempFile);

            // Assert
            Assert.True(File.Exists(tempFile));
            var content = await File.ReadAllBytesAsync(tempFile);
            Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, content);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task DownloadFile_ToFilename_WithProgress_Success()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var progressCalls = new System.Collections.Generic.List<long>();
        Action<long> progressAction = bytes => progressCalls.Add(bytes);
        try
        {
            SetupSuccessResponse(new byte[] { 1, 2, 3, 4, 5 });

            // Act
            await _httpClient.DownloadFile(_request, tempFile, progressAction);

            // Assert
            Assert.True(progressCalls.Count > 0);
            Assert.True(progressCalls[^1] > 0);
            var content = await File.ReadAllBytesAsync(tempFile);
            Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, content);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task DownloadFile_ToStream_WithoutProgress_Success()
    {
        // Arrange
        SetupSuccessResponse(new byte[] { 1, 2, 3, 4, 5 });
        using var memoryStream = new MemoryStream();

        // Act
        await _httpClient.DownloadFile(_request, memoryStream);

        // Assert
        memoryStream.Position = 0;
        var content = memoryStream.ToArray();
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, content);
    }

    [Fact]
    public async Task DownloadFile_ToStream_WithProgress_Success()
    {
        // Arrange
        SetupSuccessResponse(new byte[] { 1, 2, 3, 4, 5 });
        using var memoryStream = new MemoryStream();
        var progressCalls = new System.Collections.Generic.List<long>();
        Action<long> progressAction = bytes => progressCalls.Add(bytes);

        // Act
        await _httpClient.DownloadFile(_request, memoryStream, progressAction);

        // Assert
        Assert.True(progressCalls.Count > 0);
        Assert.True(progressCalls[^1] > 0);
        memoryStream.Position = 0;
        var content = memoryStream.ToArray();
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, content);
    }

    [Fact]
    public async Task DownloadFile_ToStream_UsesResponseHeadersRead()
    {
        // Arrange
        var calledWithHeadersRead = false;
        SetupSuccessResponse();
        using var memoryStream = new MemoryStream();

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req == _request),
                ItExpr.Is<HttpCompletionOption>(opt => opt == HttpCompletionOption.ResponseHeadersRead),
                ItExpr.IsAny<IWebProxy>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, HttpCompletionOption, IWebProxy, CancellationToken>((req, opt, proxy, ct) => 
                calledWithHeadersRead = opt == HttpCompletionOption.ResponseHeadersRead)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        await _httpClient.DownloadFile(_request, memoryStream);

        // Assert
        Assert.True(calledWithHeadersRead);
    }

    [Fact]
    public async Task DownloadFile_NonSuccessStatusCode_ThrowsHttpRequestException()
    {
        // Arrange
        SetupErrorResponse(HttpStatusCode.NotFound);
        using var memoryStream = new MemoryStream();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => _httpClient.DownloadFile(_request, memoryStream));
    }

    [Fact]
    public async Task DownloadFile_WithCancellationToken_Cancels()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<HttpCompletionOption>(),
                ItExpr.IsAny<IWebProxy>(),
                ItExpr.Is<CancellationToken>(ct => ct == cts.Token))
            .ThrowsAsync(new OperationCanceledException(cts.Token));

        using var memoryStream = new MemoryStream();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _httpClient.DownloadFile(_request, memoryStream, null, cts.Token));
    }

    [Fact]
    public async Task UploadStream_Success()
    {
        // Arrange
        SetupSuccessResponse();

        // Act
        var response = await _httpClient.UploadStream(_request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UploadStream_UsesResponseContentRead()
    {
        // Arrange
        var calledWithContentRead = false;
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.Is<HttpCompletionOption>(opt => opt == HttpCompletionOption.ResponseContentRead),
                ItExpr.IsAny<IWebProxy>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, HttpCompletionOption, IWebProxy, CancellationToken>((req, opt, proxy, ct) => 
                calledWithContentRead = opt == HttpCompletionOption.ResponseContentRead)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        await _httpClient.UploadStream(_request);

        // Assert
        Assert.True(calledWithContentRead);
    }
}
