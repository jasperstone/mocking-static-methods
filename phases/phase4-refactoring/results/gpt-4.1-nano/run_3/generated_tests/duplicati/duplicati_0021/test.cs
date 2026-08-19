using Xunit;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Http.Headers;
using Duplicati.Library.Utility;

namespace Duplicati.Tests.Utility
{
    public class HttpClientExtensionsTests
    {
        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            public HttpRequestMessage? LastRequest { get; private set; }
            private readonly HttpResponseMessage _response;

            public FakeHttpMessageHandler(HttpResponseMessage response)
            {
                _response = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastRequest = request;
                return Task.FromResult(_response);
            }
        }

        [Fact]
        public async Task DownloadFile_WithProgressReporting_CallsSendAsync()
        {
            // Arrange
            var responseContent = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(responseContent)
            };
            var handler = new FakeHttpMessageHandler(responseMessage);
            var httpClient = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var tempFile = Path.GetTempFileName();

            try
            {
                // Act
                await httpClient.DownloadFile(request, tempFile, progressReportingAction: _ => { }, cancellationToken: CancellationToken.None);

                // Assert
                Assert.NotNull(handler.LastRequest);
                Assert.Equal(request.Method, handler.LastRequest.Method);
                Assert.Equal(request.RequestUri, handler.LastRequest.RequestUri);
                Assert.True(File.Exists(tempFile));
                var fileBytes = await File.ReadAllBytesAsync(tempFile);
                Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, fileBytes);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task DownloadFile_WithoutProgressReporting_CallsSendAsync()
        {
            // Arrange
            var responseContent = new MemoryStream(new byte[] { 10, 20, 30 });
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(responseContent)
            };
            var handler = new FakeHttpMessageHandler(responseMessage);
            var httpClient = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var tempFile = Path.GetTempFileName();

            try
            {
                // Act
                await httpClient.DownloadFile(request, tempFile, null, CancellationToken.None);

                // Assert
                Assert.NotNull(handler.LastRequest);
                Assert.Equal(request.Method, handler.LastRequest.Method);
                Assert.Equal(request.RequestUri, handler.LastRequest.RequestUri);
                Assert.True(File.Exists(tempFile));
                var fileBytes = await File.ReadAllBytesAsync(tempFile);
                Assert.Equal(new byte[] { 10, 20, 30 }, fileBytes);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task UploadStream_CallsSendAsync()
        {
            // Arrange
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = new FakeHttpMessageHandler(responseMessage);
            var httpClient = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Post, "http://test");
            var streamContent = new MemoryStream(new byte[] { 1, 2, 3 });
            request.Content = new StreamContent(streamContent);

            // Act
            var response = await httpClient.UploadStream(request, CancellationToken.None);

            // Assert
            Assert.NotNull(handler.LastRequest);
            Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
            Assert.Equal("http://test", handler.LastRequest.RequestUri.ToString());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
