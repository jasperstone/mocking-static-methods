using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Utility;
using NSubstitute;
using Xunit;

public class HttpClientExtensionsTests
{
    [Fact]
    public async Task DownloadFile_ShouldDownloadFileToStream()
    {
        // Arrange
        var httpClientMock = Substitute.For<HttpClient>();
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
        var fileStream = new MemoryStream();
        var progressReportingAction = new Action<long>(bytes => { });

        var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("Test content")
        };

        httpClientMock.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<HttpCompletionOption>(), Arg.Any<CancellationToken>())
                      .Returns(responseMessage);

        // Act
        await HttpClientExtensions.DownloadFile(httpClientMock, request, fileStream, progressReportingAction);

        // Assert
        fileStream.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(fileStream);
        var content = await reader.ReadToEndAsync();
        Assert.Equal("Test content", content);
    }

    [Fact]
    public async Task DownloadFile_ShouldDownloadFileToFile()
    {
        // Arrange
        var httpClientMock = Substitute.For<HttpClient>();
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
        var filename = Path.GetTempFileName();
        var progressReportingAction = new Action<long>(bytes => { });

        var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("Test content")
        };

        httpClientMock.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<HttpCompletionOption>(), Arg.Any<CancellationToken>())
                      .Returns(responseMessage);

        // Act
        await HttpClientExtensions.DownloadFile(httpClientMock, request, filename, progressReportingAction);

        // Assert
        var content = await File.ReadAllTextAsync(filename);
        Assert.Equal("Test content", content);
        File.Delete(filename);
    }

    [Fact]
    public async Task UploadStream_ShouldUploadStream()
    {
        // Arrange
        var httpClientMock = Substitute.For<HttpClient>();
        var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com")
        {
            Content = new StringContent("Test content")
        };

        var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

        httpClientMock.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<HttpCompletionOption>(), Arg.Any<CancellationToken>())
                      .Returns(responseMessage);

        // Act
        var response = await HttpClientExtensions.UploadStream(httpClientMock, request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
}
