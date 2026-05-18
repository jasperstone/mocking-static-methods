using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Utility;
using Xunit;

namespace Duplicati.Library.Utility.Tests
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task UploadStream_CallsSendAsyncWithResponseContentRead()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            using var httpClient = new HttpClient(mockHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var cts = new CancellationTokenSource();

            // Act
            var result = await httpClient.UploadStream(request, cts.Token);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(HttpCompletionOption.ResponseContentRead, mockHandler.CapturedCompletionOption);
            Assert.Same(request, mockHandler.CapturedRequest);
            Assert.Equal(cts.Token, mockHandler.CapturedCancellationToken);
        }

        [Fact]
        public async Task DownloadFile_StreamWithoutProgress_CopiesContent()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            using var httpClient = new HttpClient(mockHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var memoryStream = new MemoryStream();
            mockHandler.SetResponseContent("Hello World");

            // Act
            await httpClient.DownloadFile(request, memoryStream);

            // Assert
            var content = Encoding.UTF8.GetString(memoryStream.ToArray());
            Assert.Equal("Hello World", content);
            Assert.Equal(HttpCompletionOption.ResponseHeadersRead, mockHandler.CapturedCompletionOption);
        }

        [Fact]
        public async Task DownloadFile_StreamWithProgress_UsesProgressReportingStream()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            using var httpClient = new HttpClient(mockHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var memoryStream = new MemoryStream();
            var progressValues = new System.Collections.Generic.List<long>();
            void progressAction(long bytes) => progressValues.Add(bytes);
            mockHandler.SetResponseContent("Hello World");

            // Act
            await httpClient.DownloadFile(request, memoryStream, progressAction);

            // Assert
            Assert.NotEmpty(progressValues);
            Assert.True(progressValues[^1] > 0);
            var content = Encoding.UTF8.GetString(memoryStream.ToArray());
            Assert.Equal("Hello World", content);
        }

        [Fact]
        public async Task DownloadFile_FailureResponse_ThrowsHttpRequestException()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            using var httpClient = new HttpClient(mockHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            mockHandler.SetResponseStatus(HttpStatusCode.BadGateway);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<HttpRequestException>(
                () => httpClient.DownloadFile(request, new MemoryStream()));
            Assert.Equal(HttpStatusCode.BadGateway, ex.StatusCode);
        }

        [Fact]
        public async Task DownloadFile_ToFile_CreatesFileWithContent()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            using var httpClient = new HttpClient(mockHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var tempFile = Path.GetTempFileName();
            try
            {
                mockHandler.SetResponseContent("Hello World");

                // Act
                await httpClient.DownloadFile(request, tempFile);

                // Assert
                var content = await File.ReadAllTextAsync(tempFile);
                Assert.Equal("Hello World", content);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }
    }

    public class MockHttpMessageHandler : HttpMessageHandler
    {
        public HttpRequestMessage? CapturedRequest { get; private set; }
        public HttpCompletionOption CapturedCompletionOption { get; private set; }
        public CancellationToken CapturedCancellationToken { get; private set; }
        private string? _responseContent;
        private HttpStatusCode _statusCode = HttpStatusCode.OK;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CapturedRequest = request;
            CapturedCancellationToken = cancellationToken;

            var response = new HttpResponseMessage(_statusCode);
            if (_responseContent != null)
            {
                response.Content = new StringContent(_responseContent);
            }
            return Task.FromResult(response);
        }

        public void SetResponseContent(string content) => _responseContent = content;
        public void SetResponseStatus(HttpStatusCode statusCode) => _statusCode = statusCode;

        public new Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption option, CancellationToken cancellationToken)
        {
            CapturedCompletionOption = option;
            CapturedCancellationToken = cancellationToken;
            CapturedRequest = request;

            var response = new HttpResponseMessage(_statusCode);
            if (_responseContent != null)
            {
                response.Content = new StringContent(_responseContent);
            }
            return Task.FromResult(response);
        }
    }
}
