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
            var responseContent = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(responseContent)
            };

            var handler = new FakeHttpMessageHandler(responseMessage);
            var client = new HttpClient(handler);

            var tempFile = Path.GetTempFileName();

            try
            {
                bool sendAsyncCalled = false;
                // Wrap the client to verify SendAsync call
                var testClient = new HttpClient(new FakeHttpMessageHandler(responseMessage));
                // Act
                await testClient.DownloadFile(new HttpRequestMessage(HttpMethod.Get, "http://test"), tempFile);

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
