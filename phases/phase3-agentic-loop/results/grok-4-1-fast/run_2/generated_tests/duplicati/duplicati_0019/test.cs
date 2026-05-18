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
    public async Task DownloadFileToFilename_SuccessfulResponseWithProgressReporting_WritesFile()
    {
        // Arrange
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        var responseContent = "Hello, World!";
        mockHttpMessageHandler.RespondWithSuccess(responseContent);
        
        using var httpClient = new HttpClient(mockHttpMessageHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/test");
        var tempFile = Path.GetTempFileName();
        
        try
        {
            long progressCallCount = 0;
            Action<long> progressAction = _ => progressCallCount++;
            
            // Act
            await httpClient.DownloadFile(request, tempFile, progressAction);
            
            // Assert
            Assert.True(File.Exists(tempFile));
            Assert.Equal(responseContent, await File.ReadAllTextAsync(tempFile));
            Assert.True(progressCallCount > 0); // Progress should be called at least once
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
    
    [Fact]
    public async Task DownloadFileToFilename_SuccessfulResponseWithoutProgressReporting_WritesFile()
    {
        // Arrange
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        var responseContent = "Hello, World!";
        mockHttpMessageHandler.RespondWithSuccess(responseContent);
        
        using var httpClient = new HttpClient(mockHttpMessageHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/test");
        var tempFile = Path.GetTempFileName();
        
        try
        {
            // Act
            await httpClient.DownloadFile(request, tempFile);
            
            // Assert
            Assert.True(File.Exists(tempFile));
            Assert.Equal(responseContent, await File.ReadAllTextAsync(tempFile));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
    
    [Fact]
    public async Task DownloadFileToStream_SuccessfulResponseWithProgressReporting_WritesToStream()
    {
        // Arrange
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        var responseContent = "Hello, World!";
        mockHttpMessageHandler.RespondWithSuccess(responseContent);
        
        using var httpClient = new HttpClient(mockHttpMessageHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/test");
        var memoryStream = new MemoryStream();
        
        long progressCallCount = 0;
        Action<long> progressAction = _ => progressCallCount++;
        
        // Act
        await httpClient.DownloadFile(request, memoryStream, progressAction);
        memoryStream.Position = 0;
        
        // Assert
        Assert.Equal(responseContent, System.Text.Encoding.UTF8.GetString(memoryStream.ToArray()));
        Assert.True(progressCallCount > 0);
    }
    
    [Fact]
    public async Task DownloadFileToStream_SuccessfulResponseWithoutProgressReporting_WritesToStream()
    {
        // Arrange
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        var responseContent = "Hello, World!";
        mockHttpMessageHandler.RespondWithSuccess(responseContent);
        
        using var httpClient = new HttpClient(mockHttpMessageHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/test");
        var memoryStream = new MemoryStream();
        
        // Act
        await httpClient.DownloadFile(request, memoryStream);
        memoryStream.Position = 0;
        
        // Assert
        Assert.Equal(responseContent, System.Text.Encoding.UTF8.GetString(memoryStream.ToArray()));
    }
    
    [Fact]
    public async Task DownloadFile_UnsuccessfulResponse_ThrowsHttpRequestException()
    {
        // Arrange
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        mockHttpMessageHandler.RespondWithStatus(HttpStatusCode.BadRequest);
        
        using var httpClient = new HttpClient(mockHttpMessageHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/test");
        var tempFile = Path.GetTempFileName();
        
        try
        {
            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => 
                httpClient.DownloadFile(request, tempFile));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}

public class MockHttpMessageHandler : HttpMessageHandler
{
    private HttpResponseMessage _response;
    
    public void RespondWithSuccess(string content)
    {
        _response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(content)
        };
    }
    
    public void RespondWithStatus(HttpStatusCode statusCode)
    {
        _response = new HttpResponseMessage(statusCode);
    }
    
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_response);
    }
}
