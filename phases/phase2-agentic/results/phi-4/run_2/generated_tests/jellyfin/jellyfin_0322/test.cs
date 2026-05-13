using System;
using System.IO;
using System.Net.Http;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Api.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

public class FileStreamResponseHelpersTests
{
    [Fact]
    public async Task GetStaticRemoteStreamResult_SendsRequestWithCorrectHeaders()
    {
        // Arrange
        var state = new Mock<StreamState>();
        state.Setup(s => s.MediaPath).Returns("http://example.com/media");
        state.Setup(s => s.RemoteHttpHeaders).Returns(new System.Collections.Generic.Dictionary<string, string>
        {
            { "User-Agent", "TestAgent" }
        });

        var httpClient = new Mock<HttpClient>();
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri("http://example.com/media"));
        requestMessage.Headers.UserAgent.Clear();
        requestMessage.Headers.TryAddWithoutValidation("User-Agent", "TestAgent");

        var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        responseMessage.Content = new StreamContent(new MemoryStream());
        responseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(MediaTypeNames.Application.Octet);

        httpClient.SetupSequence(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseMessage);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Add("Range", "bytes=0-");

        // Act
        var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state.Object, httpClient.Object, httpContext, CancellationToken.None);

        // Assert
        Assert.IsType<FileStreamResult>(result);
        httpClient.Verify(h => h.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal("TestAgent", requestMessage.Headers.UserAgent.ToString());
        Assert.Equal("bytes=0-", requestMessage.Headers.Range.ToString());
        Assert.Equal("bytes", httpContext.Response.Headers[HeaderNames.AcceptRanges]);
    }
}
