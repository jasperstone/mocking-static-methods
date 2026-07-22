using Xunit;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Moq;
using System.Net;
using Duplicati.Library.Utility;

namespace Duplicati.Tests
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task DownloadFile_Should_Call_SendAsync_And_Write_File()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
                {
                    var responseContent = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
                    var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StreamContent(responseContent)
                    };
                    return responseMessage;
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var filename = Path.GetTempFileName();

            try
            {
                // Act
                await httpClient.DownloadFile(request, filename);

                // Assert
                handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString() == "http://test"),
                    ItExpr.IsAny<CancellationToken>());

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
    }
}
