using Xunit;
using System.Net.Http;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System;
using Duplicati.Library.Utility;
using Moq;
using Moq.Protected;
using System.Linq;

namespace Duplicati.Library.Utility.Tests;

public class HttpClientExtensionsTests
{
    [Fact]
    public async Task DownloadFile_ToFileName_CallsSendAsyncAndSavesFile()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var fakeResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(new byte[] { 1, 2, 3 })
        };
        
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<HttpCompletionOption>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(fakeResponse);
        
        var httpClient = new HttpClient(mockHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            await httpClient.DownloadFile(request, tempFile);

            // Assert
            mockHandler.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                HttpCompletionOption.ResponseHeadersRead,
                ItExpr.IsAny<CancellationToken>());
            
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
    public async Task DownloadFile_ToStream_CallsSendAsyncAndWritesToStream()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var expectedContent = new byte[] { 1, 2, 3, 4, 5 };
        var fakeResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(expectedContent)
        };
        
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<HttpCompletionOption>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(fakeResponse);
        
        var httpClient = new HttpClient(mockHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
        var memoryStream = new MemoryStream();

        // Act
        await httpClient.DownloadFile(request, memoryStream);

        // Assert
        mockHandler.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            HttpCompletionOption.ResponseHeadersRead,
            ItExpr.IsAny<CancellationToken>());
        
        Assert.Equal(expectedContent, memoryStream.ToArray());
    }

    [Fact]
    public async Task DownloadFile_WithProgressReporting_CallsSendAsyncAndReportsProgress()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var contentBytes = new byte[1024];
        new Random().NextBytes(contentBytes);
        var progressCalled = false;
        Action<long> progressAction = bytes => {
            progressCalled = true;
            Assert.True(bytes > 0);
        };

        var fakeResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(contentBytes)
        };
        
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<HttpCompletionOption>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(fakeResponse);
        
        var httpClient = new HttpClient(mockHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            await httpClient.DownloadFile(request, tempFile, progressAction);

            // Assert
            mockHandler.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                HttpCompletionOption.ResponseHeadersRead,
                ItExpr.IsAny<CancellationToken>());
            
            Assert.True(progressCalled);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task UploadStream_CallsSendAsyncWithResponseContentRead()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.Is<HttpCompletionOption>(o => o == HttpCompletionOption.ResponseContentRead), 
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        
        var httpClient = new HttpClient(mockHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com");

        // Act
        await httpClient.UploadStream(request);

        // Assert
        mockHandler.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            HttpCompletionOption.ResponseContentRead,
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task DownloadFile_FailsWithNonSuccessStatusCode_ThrowsException()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<HttpCompletionOption>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
        
        var httpClient = new HttpClient(mockHandler.Object);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
        var memoryStream = new MemoryStream();

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => 
            httpClient.DownloadFile(request, memoryStream));
    }
}
