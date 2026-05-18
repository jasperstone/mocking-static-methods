using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Duplicati.Library.Utility;

namespace Duplicati.Tests.Utility
{
    public class HttpClientExtensionsTests
    {
        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response;

            public FakeHttpMessageHandler(HttpResponseMessage response)
            {
                _response = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_response);
            }
        }

        [Fact]
        public async Task DownloadFile_Should_Call_SendAsync_And_WriteStream()
        {
            // Arrange
            var contentStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(contentStream)
            };
            var handler = new FakeHttpMessageHandler(response);
            var client = new HttpClient(handler);

            var tempFile = Path.GetTempFileName();

            try
            {
                // Act
                await client.DownloadFile(new HttpRequestMessage(HttpMethod.Get, "http://test"), tempFile);

                // Assert
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
        public async Task DownloadFile_WithProgressReporting_Should_Call_ProgressAction()
        {
            // Arrange
            var contentStream = new MemoryStream(new byte[] { 10, 20, 30 });
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(contentStream)
            };
            var handler = new FakeHttpMessageHandler(response);
            var client = new HttpClient(handler);
            long totalBytes = 0;
            void ProgressAction(long bytesTransferred) => totalBytes = bytesTransferred;

            var tempFile = Path.GetTempFileName();

            try
            {
                // Act
                await client.DownloadFile(new HttpRequestMessage(HttpMethod.Get, "http://test"), tempFile, ProgressAction);

                // Assert
                Assert.True(File.Exists(tempFile));
                Assert.Equal(3, totalBytes);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task UploadStream_Should_Call_SendAsync()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = new FakeHttpMessageHandler(response);
            var client = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Post, "http://test");
            var streamContent = new MemoryStream(new byte[] { 1, 2, 3 });
            request.Content = new StreamContent(streamContent);

            // Act
            var result = await client.UploadStream(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }
    }
}
