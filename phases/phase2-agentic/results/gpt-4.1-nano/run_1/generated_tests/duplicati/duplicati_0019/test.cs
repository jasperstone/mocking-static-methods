using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Duplicati.Tests
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task DownloadFile_Should_Call_SendAsync()
        {
            // Arrange
            var handlerMock = new MockHttpMessageHandler();
            var client = new HttpClient(handlerMock);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var filename = "testfile.txt";

            // Setup response
            var responseContent = new MemoryStream(new byte[] { 1, 2, 3, 4 });
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(responseContent)
            };
            handlerMock.ResponseMessage = responseMessage;

            // Act
            await client.DownloadFile(request, filename);

            // Assert
            Assert.True(handlerMock.SendAsyncCalled);
            Assert.Equal(request.Method, handlerMock.LastRequest.Method);
            Assert.Equal(request.RequestUri, handlerMock.LastRequest.RequestUri);
            Assert.True(File.Exists(filename));
            var fileBytes = await File.ReadAllBytesAsync(filename);
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, fileBytes);

            // Cleanup
            if (File.Exists(filename))
                File.Delete(filename);
        }
    }

    // Mock HttpMessageHandler to intercept SendAsync
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }
        public HttpResponseMessage? ResponseMessage { get; set; }
        public bool SendAsyncCalled { get; private set; } = false;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            SendAsyncCalled = true;
            LastRequest = request;
            return Task.FromResult(ResponseMessage ?? new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }
}
