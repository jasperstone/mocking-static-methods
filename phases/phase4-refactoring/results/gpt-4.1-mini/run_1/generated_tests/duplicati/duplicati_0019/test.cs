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
        private class MockHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handlerFunc;

            public MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc)
            {
                _handlerFunc = handlerFunc;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _handlerFunc(request, cancellationToken);
            }
        }

        [Fact]
        public async Task DownloadFile_SavesFile_WithProgressReporting()
        {
            // Arrange
            var contentBytes = new byte[100];
            for (int i = 0; i < contentBytes.Length; i++) contentBytes[i] = (byte)i;

            var httpContent = new ByteArrayContent(contentBytes);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = httpContent
            };

            var handler = new MockHttpMessageHandler((req, ct) => Task.FromResult(response));
            var client = new HttpClient(handler);

            var tempFile = Path.GetTempFileName();
            try
            {
                long lastProgress = -1;
                void ProgressAction(long progress)
                {
                    lastProgress = progress;
                }

                var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");

                // Act
                await client.DownloadFile(request, tempFile, ProgressAction, CancellationToken.None);

                // Assert
                Assert.True(File.Exists(tempFile));
                var fileBytes = await File.ReadAllBytesAsync(tempFile);
                Assert.Equal(contentBytes.Length, fileBytes.Length);
                Assert.Equal(contentBytes, fileBytes);
                Assert.True(lastProgress >= 0);
                Assert.Equal(contentBytes.Length, lastProgress);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task DownloadFile_SavesFile_WithoutProgressReporting()
        {
            // Arrange
            var contentBytes = new byte[50];
            for (int i = 0; i < contentBytes.Length; i++) contentBytes[i] = (byte)(255 - i);

            var httpContent = new ByteArrayContent(contentBytes);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = httpContent
            };

            var handler = new MockHttpMessageHandler((req, ct) => Task.FromResult(response));
            var client = new HttpClient(handler);

            var tempFile = Path.GetTempFileName();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");

                // Act
                await client.DownloadFile(request, tempFile, null, CancellationToken.None);

                // Assert
                Assert.True(File.Exists(tempFile));
                var fileBytes = await File.ReadAllBytesAsync(tempFile);
                Assert.Equal(contentBytes.Length, fileBytes.Length);
                Assert.Equal(contentBytes, fileBytes);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
