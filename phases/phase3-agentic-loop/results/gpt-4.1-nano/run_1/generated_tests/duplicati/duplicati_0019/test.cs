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
        public async Task DownloadFile_Should_Call_SendAsync_With_Correct_Params()
        {
            // Arrange
            var responseContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4, 5 });
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = responseContent
            };

            var handler = new FakeHttpMessageHandler(responseMessage);
            var client = new HttpClient(handler);

            var tempFile = Path.GetTempFileName();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
                bool sendAsyncCalled = false;

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
    }
}
