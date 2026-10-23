using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Duplicati.Library.Utility;

namespace Duplicati.Tests
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task DownloadFile_Should_Call_SendAsync_And_Write_File()
        {
            // Arrange
            var responseContentStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(responseContentStream)
            };

            var handler = new TestHttpMessageHandler((request, cancellationToken) =>
            {
                return Task.FromResult(responseMessage);
            });

            var httpClient = new HttpClient(handler);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var filename = "testfile.bin";

            // Act
            await httpClient.DownloadFile(request, filename);

            // Assert
            Assert.True(File.Exists(filename));
            var fileBytes = await File.ReadAllBytesAsync(filename);
            Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, fileBytes);
            // Cleanup
            File.Delete(filename);
        }

        private class TestHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

            public TestHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
            {
                _sendAsync = sendAsync;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _sendAsync(request, cancellationToken);
            }
        }
    }
}
