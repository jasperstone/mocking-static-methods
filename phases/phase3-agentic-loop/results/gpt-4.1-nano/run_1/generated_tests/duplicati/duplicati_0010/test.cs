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
            var mockClient = new Mock<HttpClient>();
            var mockResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("{\"ok\":true}")
            };

            mockClient.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(mockResponse);

            mockFactory.Setup(f => f.CreateClient()).Returns(mockClient.Object);
            HttpClientHelper.Configure(mockFactory.Object);

            var sendTelegram = new SendTelegramMessage();

            // Set required fields via reflection
            var type = typeof(SendTelegramMessage);
            type.GetField("m_botid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(sendTelegram, "botid");
            type.GetField("m_apikey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(sendTelegram, "apikey");
            type.GetField("m_channelId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(sendTelegram, "channel");
            type.GetField("m_topicId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(sendTelegram, "topic");

            // Act
            await sendTelegram.SendMessageChunk("test message", 1, 1);

            // Assert
            mockClient.Verify(c => c.GetAsync(It.Is<string>(url => url.Contains("sendMessage")), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
