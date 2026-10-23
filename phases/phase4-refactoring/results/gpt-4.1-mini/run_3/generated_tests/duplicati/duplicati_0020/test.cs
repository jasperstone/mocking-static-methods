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
        public async Task DownloadFile_WithFilename_WritesFileAndReportsProgress()
        {
            var contentBytes = new byte[100];
            for (int i = 0; i < contentBytes.Length; i++) contentBytes[i] = (byte)i;

            var handler = new TestHttpMessageHandler(async (req, ct) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(contentBytes)
                };
                return await Task.FromResult(response);
            });

            using var client = new HttpClient(handler);

            var tempFile = Path.GetTempFileName();
            try
            {
                long lastProgress = -1;
                void Progress(long bytes) => lastProgress = bytes;

                await client.DownloadFile(new HttpRequestMessage(HttpMethod.Get, "http://test"), tempFile, Progress);

                // Assert file content matches
                var fileBytes = await File.ReadAllBytesAsync(tempFile);
                Assert.Equal(contentBytes, fileBytes);

                // Assert progress was reported at least once and is equal to content length
                Assert.True(lastProgress >= 0);
                Assert.Equal(contentBytes.Length, lastProgress);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task DownloadFile_WithStream_WritesStreamAndReportsProgress()
        {
            var contentBytes = new byte[50];
            for (int i = 0; i < contentBytes.Length; i++) contentBytes[i] = (byte)(contentBytes.Length - i);

            var handler = new TestHttpMessageHandler(async (req, ct) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(contentBytes)
                };
                return await Task.FromResult(response);
            });

            using var client = new HttpClient(handler);

            using var outputStream = new MemoryStream();
            long lastProgress = -1;
            void Progress(long bytes) => lastProgress = bytes;

            await client.DownloadFile(new HttpRequestMessage(HttpMethod.Get, "http://test"), outputStream, Progress);

            // Assert stream content matches
            var resultBytes = outputStream.ToArray();
            Assert.Equal(contentBytes, resultBytes);

            // Assert progress was reported at least once and is equal to content length
            Assert.True(lastProgress >= 0);
            Assert.Equal(contentBytes.Length, lastProgress);
        }

        [Fact]
        public async Task DownloadFile_WithFilename_NoProgress_WritesFile()
        {
            var contentBytes = new byte[20];
            for (int i = 0; i < contentBytes.Length; i++) contentBytes[i] = (byte)(i * 2);

            var handler = new TestHttpMessageHandler((req, ct) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(contentBytes)
                };
                return Task.FromResult(response);
            });

            using var client = new HttpClient(handler);

            var tempFile = Path.GetTempFileName();
            try
            {
                await client.DownloadFile(new HttpRequestMessage(HttpMethod.Get, "http://test"), tempFile);

                var fileBytes = await File.ReadAllBytesAsync(tempFile);
                Assert.Equal(contentBytes, fileBytes);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task DownloadFile_WithStream_NoProgress_WritesStream()
        {
            var contentBytes = new byte[30];
            for (int i = 0; i < contentBytes.Length; i++) contentBytes[i] = (byte)(i + 1);

            var handler = new TestHttpMessageHandler((req, ct) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(contentBytes)
                };
                return Task.FromResult(response);
            });

            using var client = new HttpClient(handler);

            using var outputStream = new MemoryStream();

            await client.DownloadFile(new HttpRequestMessage(HttpMethod.Get, "http://test"), outputStream);

            var resultBytes = outputStream.ToArray();
            Assert.Equal(contentBytes, resultBytes);
        }
    }
}
