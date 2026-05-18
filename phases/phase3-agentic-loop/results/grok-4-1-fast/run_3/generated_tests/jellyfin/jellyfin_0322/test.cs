using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Api.Helpers;
using MediaBrowser.Controller.Streaming;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Moq;
using Moq.Protected;
using Xunit;

namespace Jellyfin.Api.Tests.Helpers
{
    public class FileStreamResponseHelpersTests : IDisposable
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;
        private readonly StreamState _streamState;

        public FileStreamResponseHelpersTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_handlerMock.Object);

            _streamState = new StreamState(null!, default, null!)
            {
                MediaPath = "http://test-server/media.mp4"
            };
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            _streamState?.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_CallsSendAsync_WithResponseHeadersRead()
        {
            // Arrange
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(Array.Empty<byte>())
            };
            SetupHttpHandler(responseMessage);

            var httpContextMock = SetupHttpContext();

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(
                _streamState, _httpClient, httpContextMock.Object, default);

            // Assert
            _handlerMock.Protected()
                .Verify("SendAsync", Times.Once(),
                    ItExpr.IsAny<HttpRequestMessage>(),
                    HttpCompletionOption.ResponseHeadersRead,
                    ItExpr.IsAny<CancellationToken>());
            Assert.IsType<Microsoft.AspNetCore.Mvc.FileStreamResult>(result);
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_ForwardsUserAgent_WhenPresentInRemoteHttpHeaders()
        {
            // Arrange
            _streamState.RemoteHttpHeaders = new Dictionary<string, string>
            {
                [HeaderNames.UserAgent] = "TestUserAgent/1.0"
            };
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(Array.Empty<byte>())
            };
            SetupHttpHandler(responseMessage);

            var httpContextMock = SetupHttpContext();

            // Act
            await FileStreamResponseHelpers.GetStaticRemoteStreamResult(
                _streamState, _httpClient, httpContextMock.Object, default);

            // Assert
            _handlerMock.Protected()
                .Verify("SendAsync", Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Headers.UserAgent.Count() == 1 &&
                        req.Headers.UserAgent.ToString() == "TestUserAgent/1.0"),
                    HttpCompletionOption.ResponseHeadersRead,
                    ItExpr.IsAny<CancellationToken>());
        }

        [Theory]
        [InlineData("bytes=0-999")]
        [InlineData("bytes=500-")]
        public async Task GetStaticRemoteStreamResult_ForwardsRangeHeader(string rangeValue)
        {
            // Arrange
            var httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(r => r.Headers[HeaderNames.Range]).Returns(new StringValues(rangeValue));

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.Request).Returns(httpRequestMock.Object);

            var responseMessage = new HttpResponseMessage(HttpStatusCode.PartialContent)
            {
                Content = new ByteArrayContent(Array.Empty<byte>())
            };
            SetupHttpHandler(responseMessage);

            // Act
            await FileStreamResponseHelpers.GetStaticRemoteStreamResult(
                _streamState, _httpClient, httpContextMock.Object, default);

            // Assert
            _handlerMock.Protected()
                .Verify("SendAsync", Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req => req.Headers.Range != null),
                    HttpCompletionOption.ResponseHeadersRead,
                    ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_SetsAcceptRangesNone_WhenNoAcceptRangesHeader()
        {
            // Arrange
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(Array.Empty<byte>())
            };
            SetupHttpHandler(responseMessage);

            var httpResponseMock = new Mock<HttpResponse>();
            httpResponseMock.SetupProperty(r => r.Headers);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.Response).Returns(httpResponseMock.Object);

            // Act
            await FileStreamResponseHelpers.GetStaticRemoteStreamResult(
                _streamState, _httpClient, httpContextMock.Object, default);

            // Assert
            httpResponseMock.VerifySet(r => r.Headers[HeaderNames.AcceptRanges] = "none");
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_SetsAcceptRangesBytes_When206WithoutAcceptRangesHeader()
        {
            // Arrange
            var responseMessage = new HttpResponseMessage(HttpStatusCode.PartialContent)
            {
                Content = new ByteArrayContent(Array.Empty<byte>())
            };
            SetupHttpHandler(responseMessage);

            var httpResponseMock = new Mock<HttpResponse>();
            httpResponseMock.SetupProperty(r => r.Headers);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.Response).Returns(httpResponseMock.Object);

            // Act
            await FileStreamResponseHelpers.GetStaticRemoteStreamResult(
                _streamState, _httpClient, httpContextMock.Object, default);

            // Assert
            httpResponseMock.VerifySet(r => r.Headers[HeaderNames.AcceptRanges] = "bytes");
        }

        private Mock<HttpContext> SetupHttpContext()
        {
            var httpRequestMock = new Mock<HttpRequest>();
            var httpResponseMock = new Mock<HttpResponse>();
            httpResponseMock.SetupProperty(r => r.Headers);
            httpResponseMock.SetupProperty(r => r.ContentLength);
            httpResponseMock.SetupProperty(r => r.StatusCode);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.Request).Returns(httpRequestMock.Object);
            httpContextMock.Setup(c => c.Response).Returns(httpResponseMock.Object);

            return httpContextMock;
        }

        private void SetupHttpHandler(HttpResponseMessage responseMessage)
        {
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);
        }
    }
}
