using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Utility;
using Xunit;

namespace Duplicati.Library.Utility.Tests
{
    public class HttpClientExtensionsTests
    {
        private class TestHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handlerFunc;

            public TestHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc)
            {
                _handlerFunc = handlerFunc;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _handlerFunc(request, cancellationToken);
            }
        }

        [Fact]
        public async Task DownloadFile_CopiesContentToStream_WithoutProgress()
        {
            var contentBytes = new byte[] { 1, 2, 3, 4, 5 };
            var contentStream = new MemoryStream(contentBytes);

            var handler = new TestHttpMessageHandler((req, ct) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(contentStream)
                };
                return Task.FromResult(response);
            });

            var client = new HttpClient(handler);

            using var outputStream = new MemoryStream();

            await client.DownloadFile(new HttpRequestMessage(HttpMethod.Get, "http://test"), outputStream);

            var result = outputStream.ToArray();
            Assert.Equal(contentBytes, result);
        }

        [Fact]
        public async Task DownloadFile_CopiesContentToStream_WithProgress()
        {
            var contentBytes = new byte[100];
            for (int i = 0; i < contentBytes.Length; i++) contentBytes[i] = (byte)i;
            var contentStream = new MemoryStream(contentBytes);

            var handler = new TestHttpMessageHandler((req, ct) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(contentStream)
                };
                return Task.FromResult(response);
            });

            var client = new HttpClient(handler);

            using var outputStream = new MemoryStream();

            long lastProgress = -1;
            void ProgressAction(long progress) => lastProgress = progress;

            await client.DownloadFile(new HttpRequestMessage(HttpMethod.Get, "http://test"), outputStream, ProgressAction);

            var result = outputStream.ToArray();
            Assert.Equal(contentBytes, result);
            Assert.True(lastProgress > 0);
            Assert.Equal(contentBytes.Length, lastProgress);
        }

        [Fact]
        public async Task UploadStream_InvokesSendAsync_ReturnsResponse()
        {
            var handler = new TestHttpMessageHandler((req, ct) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.Accepted);
                return Task.FromResult(response);
            });

            var client = new HttpClient(handler);

            var request = new HttpRequestMessage(HttpMethod.Post, "http://test")
            {
                Content = new StringContent("test content")
            };

            var response = await client.UploadStream(request);

            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        }
    }
}
