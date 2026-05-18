using Xunit;
using System.Net.Http;
using System.Threading.Tasks;
using Duplicati.Library.Utility;
using Moq;
using System.Threading;

namespace Duplicati.Tests
{
    public class SendTelegramMessageTests
    {
        [Fact]
        public async Task SendMessageChunk_CallsGetAsync_WithExpectedUrl()
        {
            // Arrange
            var mockFactory = new Mock<IHttpClientFactory>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var client = new HttpClient(mockHttpMessageHandler.Object);
            mockFactory.Setup(_ => _.CreateClient()).Returns(client);
            HttpClientHelper.Configure(mockFactory.Object);

            var messageSender = new SendTelegramMessage
            {
                // Set required fields
                m_botid = "botid",
                m_apikey = "apikey",
                m_channelId = "channel",
                m_topicId = "topic"
            };

            // Setup mock response
            var responseContent = new StringContent("{\"ok\":true}");
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = responseContent
            };

            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(async (req) =>
                {
                    // Capture the request URL
                    var url = req.RequestUri.ToString();
                    Assert.Contains("https://api.telegram.org/bot", url);
                    return responseMessage;
                });

            // Act
            await messageSender.SendMessageChunk("Test message", 1, 1);

            // Assert
            mockHttpMessageHandler.VerifyAll();
        }
    }
}
