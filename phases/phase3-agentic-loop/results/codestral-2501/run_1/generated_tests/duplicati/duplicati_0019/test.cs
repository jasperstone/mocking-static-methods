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

public class HttpClientExtensionsTests
{
    [Fact]
    public async Task DownloadFile_ShouldDownloadFileSuccessfully()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file");
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }))
        };
        httpClientMock
            .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var filePath = Path.GetTempFileName();
        var progressReportingAction = new Action<long>(bytes => { });

        // Act
        await HttpClientExtensions.DownloadFile(httpClientMock.Object, request, filePath, progressReportingAction, CancellationToken.None);

        // Assert
        Assert.True(File.Exists(filePath));
        var fileBytes = await File.ReadAllBytesAsync(filePath);
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, fileBytes);
    }

    [Fact]
    public async Task DownloadFile_ShouldDownloadFileSuccessfully_WithProgressReporting()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file");
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }))
        };
        httpClientMock
            .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var filePath = Path.GetTempFileName();
        var progressReportingAction = new Action<long>(bytes => { });

        // Act
        await HttpClientExtensions.DownloadFile(httpClientMock.Object, request, filePath, progressReportingAction, CancellationToken.None);

        // Assert
        Assert.True(File.Exists(filePath));
        var fileBytes = await File.ReadAllBytesAsync(filePath);
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, fileBytes);
    }

    [Fact]
    public async Task DownloadFile_ShouldDownloadFileSuccessfully_WithStream()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file");
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }))
        };
        httpClientMock
            .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var fileStream = new MemoryStream();
        var progressReportingAction = new Action<long>(bytes => { });

        // Act
        await HttpClientExtensions.DownloadFile(httpClientMock.Object, request, fileStream, progressReportingAction, CancellationToken.None);

        // Assert
        fileStream.Seek(0, SeekOrigin.Begin);
        var fileBytes = fileStream.ToArray();
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, fileBytes);
    }
}
