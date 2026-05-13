using Xunit;
using Moq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using Duplicati.Library;

namespace Duplicati.Tests
{
    public class JsonWebHelperHttpClientTests
    {
        [Fact]
        public async Task GetResponseUncheckedAsync_ShouldCallSendAsync()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var cancellationToken = new CancellationToken();

            mockHttpClient
                .Setup(client => client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var jsonWebHelper = new JsonWebHelperHttpClient(mockHttpClient.Object);

            // Act
            var response = await jsonWebHelper.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            // Assert
            mockHttpClient.Verify(client => client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken), Times.Once);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
