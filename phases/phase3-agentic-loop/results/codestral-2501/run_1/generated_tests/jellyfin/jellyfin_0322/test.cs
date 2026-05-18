using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Api.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using Xunit;

public class FileStreamResponseHelpersTests
{
    [Fact]
    public async Task GetStaticRemoteStreamResult_SendAsync_Called()
    {
        // Arrange
        var handlerMock = new Mock<DelegatingHandler>();
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("Test content")
        };

        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response)
            .Verifiable();

        var httpClient = new HttpClient(handlerMock.Object);
        var httpContextMock = new Mock<HttpContext>();
        var streamState = new StreamState { MediaPath = "http://example.com/media" };

        // Act
        var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(
            streamState,
            httpClient,
            httpContextMock.Object,
            CancellationToken.None);

        // Assert
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());

        Assert.IsType<FileStreamResult>(result);
    }
}

public class StreamState
{
    public string MediaPath { get; set; }
}
