using System;
using System.IO;
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
        public async Task DownloadFile_Should_Call_SendAsync_With_Correct_Request()
        {
            // Arrange
            var responseContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4, 5 });
            var response = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = responseContent
            };
            var handler = new FakeHttpMessageHandler(response);
            var client = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var filename = Path.GetTempFileName();

            try
            {
                // Act
                await client.DownloadFile(request, filename);

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
        public async Task DownloadFile_With_ProgressReporting_Should_Call_ProgressAction()
        {
            // Arrange
            var data = new byte[] { 10, 20, 30, 40, 50 };
            var responseContent = new ByteArrayContent(data);
            var response = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = responseContent
            };
            var handler = new FakeHttpMessageHandler(response);
            var client = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var filename = Path.GetTempFileName();
            long reportedProgress = 0;
            Action<long> progressAction = progress => reportedProgress = progress;

            try
            {
                // Act
                await client.DownloadFile(request, filename, progressAction);

                // Assert
                Assert.True(File.Exists(filename));
                Assert.Equal(data.Length, reportedProgress);
            }
            finally
            {
                if (File.Exists(filename))
                    File.Delete(filename);
            }
        }

        [Fact]
        public async Task DownloadFile_With_Stream_Should_Write_To_Stream()
        {
            // Arrange
            var data = new byte[] { 7, 8, 9 };
            var responseContent = new ByteArrayContent(data);
            var response = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = responseContent
            };
            var handler = new FakeHttpMessageHandler(response);
            var client = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            using var memoryStream = new MemoryStream();

            // Act
            await client.DownloadFile(request, memoryStream);

            // Assert
            Assert.Equal(data, memoryStream.ToArray());
        }

        [Fact]
        public async Task UploadStream_Should_Call_SendAsync()
        {
            // Arrange
            var response = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK
            };
            var handler = new FakeHttpMessageHandler(response);
            var client = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Post, "http://test");
            var streamContent = new MemoryStream(new byte[] { 1, 2, 3 });
            request.Content = new StreamContent(streamContent);

            // Act
            var result = await client.UploadStream(request);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        }
    }
}
