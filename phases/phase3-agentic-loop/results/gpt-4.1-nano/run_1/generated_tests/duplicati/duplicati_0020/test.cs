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
        public async Task DownloadFile_Should_Call_SendAsync_And_Write_File()
        {
            // Arrange
            var contentStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(contentStream)
            };
            var handler = new FakeHttpMessageHandler(response);
            var client = new HttpClient(handler);

            var filename = Path.GetTempFileName();

            try
            {
                // Act
                await client.DownloadFile(new HttpRequestMessage(HttpMethod.Get, "http://test"), filename);

                // Assert
                Assert.True(File.Exists(filename));
                var fileBytes = await File.ReadAllBytesAsync(filename);
                Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, fileBytes);
            }
            finally
            {
                if (File.Exists(filename))
                    File.Delete(filename);
            }
        }

        [Fact]
        public async Task DownloadFile_WithProgress_Should_Call_ProgressReportingAction()
        {
            // Arrange
            var contentStream = new MemoryStream(new byte[] { 10, 20, 30 });
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(contentStream)
            };
            var handler = new FakeHttpMessageHandler(response);
            var client = new HttpClient(handler);

            var filename = Path.GetTempFileName();
            long reportedProgress = 0;
            Action<long> progressAction = progress => reportedProgress = progress;

            try
            {
                // Act
                await client.DownloadFile(new HttpRequestMessage(HttpMethod.Get, "http://test"), filename, progressAction);

                // Assert
                Assert.True(File.Exists(filename));
                Assert.Equal(3, reportedProgress);
            }
            finally
            {
                if (File.Exists(filename))
                    File.Delete(filename);
            }
        }

        [Fact]
        public async Task DownloadFile_Stream_Should_Call_SendAsync_And_Write_To_Stream()
        {
            // Arrange
            var contentStream = new MemoryStream(new byte[] { 7, 8, 9 });
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(contentStream)
            };
            var handler = new FakeHttpMessageHandler(response);
            var client = new HttpClient(handler);

            using var outputStream = new MemoryStream();

            // Act
            await client.DownloadFile(new HttpRequestMessage(HttpMethod.Get, "http://test"), outputStream);

            // Assert
            var resultBytes = outputStream.ToArray();
            Assert.Equal(new byte[] { 7, 8, 9 }, resultBytes);
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
