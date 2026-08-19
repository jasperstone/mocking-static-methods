using Xunit;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using Moq;
using System.Net;
using System.Net.Http.Headers;
using Duplicati.Library.Utility;

namespace Duplicati.Tests
{
    public class HttpClientExtensionsTests
    {
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

        [Fact]
        public async Task DownloadFile_WithProgressReporting_CallsSendAsync()
        {
            // Arrange
            var responseContent = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(responseContent)
            };

            var handler = new TestHttpMessageHandler((request, token) => Task.FromResult(responseMessage));
            var httpClient = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var filename = Path.GetTempFileName();

            try
            {
                // Act
                await httpClient.DownloadFile(request, filename, progressReportingAction: (long progress) => { }, cancellationToken: CancellationToken.None);

                // Assert
                Assert.True(File.Exists(filename));
            }
            finally
            {
                if (File.Exists(filename))
                    File.Delete(filename);
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

            var handler = new TestHttpMessageHandler((request, token) => Task.FromResult(responseMessage));
            var httpClient = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var filename = Path.GetTempFileName();

            try
            {
                // Act
                await httpClient.DownloadFile(request, filename, null, CancellationToken.None);

                // Assert
                Assert.True(File.Exists(filename));
            }
            finally
            {
                if (File.Exists(filename))
                    File.Delete(filename);
            }
        }
    }
}
