using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Duplicati.Library.Utility;

namespace Duplicati.Tests
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
        public async Task DownloadFile_Should_Call_SendAsync_With_Correct_Params()
        {
            // Arrange
            var responseContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4, 5 });
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = responseContent
            };

            var handler = new FakeHttpMessageHandler(responseMessage);
            var client = new HttpClient(handler);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var tempFile = Path.GetTempFileName();

            try
            {
                // Act
                await client.DownloadFile(request, tempFile);

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
        public async Task DownloadFile_With_ProgressReporting_Should_Call_ProgressAction()
        {
            // Arrange
            var responseContent = new ByteArrayContent(new byte[] { 10, 20, 30 });
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = responseContent
            };

            var handler = new FakeHttpMessageHandler(responseMessage);
            var client = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var tempFile = Path.GetTempFileName();

            long totalBytesReported = 0;
            Action<long> progressAction = (bytes) => totalBytesReported = bytes;

            try
            {
                // Act
                await client.DownloadFile(request, tempFile, progressAction);

                // Assert
                Assert.True(File.Exists(tempFile));
                Assert.Equal(3, totalBytesReported);
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
            var responseContent = new StringContent("OK");
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = responseContent
            };

            var handler = new FakeHttpMessageHandler(responseMessage);
            var client = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Post, "http://test");
            var streamContent = new MemoryStream(new byte[] { 1, 2, 3 });
            request.Content = new StreamContent(streamContent);

            // Act
            var response = await client.UploadStream(request);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }
    }
}
