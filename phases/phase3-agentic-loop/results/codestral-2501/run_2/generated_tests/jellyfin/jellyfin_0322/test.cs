using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
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
    public async Task GetStaticRemoteStreamResult_ShouldForwardHeadersAndReturnFileStreamResult()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var httpContextMock = new Mock<HttpContext>();
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri("http://example.com/media"));
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("Test content")
        };

        httpClientMock.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(responseMessage);

        var httpContextResponseMock = new Mock<HttpResponse>();
        httpContextMock.Setup(context => context.Response).Returns(httpContextResponseMock.Object);

        var state = new MockStreamState
        {
            MediaPath = "http://example.com/media",
            RemoteHttpHeaders = new Dictionary<string, string>
            {
                { "User-Agent", "TestAgent" }
            }
        };

        // Act
        var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClientMock.Object, httpContextMock.Object);

        // Assert
        Assert.IsType<FileStreamResult>(result);
        var fileStreamResult = (FileStreamResult)result;
        Assert.Equal("application/octet-stream", fileStreamResult.ContentType);

        httpContextResponseMock.VerifySet(response => response.Headers["Accept-Ranges"] = "none");
        httpContextResponseMock.VerifySet(response => response.StatusCode = (int)HttpStatusCode.OK);
    }

    public class MockStreamState
    {
        public string MediaPath { get; set; }
        public Dictionary<string, string> RemoteHttpHeaders { get; set; }
    }
}
