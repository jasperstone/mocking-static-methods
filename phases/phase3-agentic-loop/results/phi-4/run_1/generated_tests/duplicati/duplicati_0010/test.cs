using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;

namespace Duplicati.Library.Modules.Builtin.Tests
{
    public class SendTelegramMessageTests
    {
        private const string BotId = "testBotId";
        private const string ApiKey = "testApiKey";
        private const string ChannelId = "testChannelId";
        private const string TopicId = "testTopicId";
        private const string Message = "testMessage";
        private const int PartNumber = 1;
        private const int TotalParts = 1;

        [Fact]
        public async Task SendMessageChunk_Success()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"ok\":true}")
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            var sendTelegramMessage = new SendTelegramMessage();

            // Use reflection to set private fields
            typeof(SendTelegramMessage)
                .GetField("m_botid", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(sendTelegramMessage, BotId);

            typeof(SendTelegramMessage)
                .GetField("m_apikey", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(sendTelegramMessage, ApiKey);

            typeof(SendTelegramMessage)
                .GetField("m_channelId", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(sendTelegramMessage, ChannelId);

            typeof(SendTelegramMessage)
                .GetField("m_topicId", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(sendTelegramMessage, TopicId);

            // Act
            await sendTelegramMessage.SendMessageChunk(Message, PartNumber, TotalParts);

            // Assert
            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().Contains($"bot{BotId}:{ApiKey}/sendMessage")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task SendMessageChunk_BotIdNotSet_ThrowsException()
        {
            // Arrange
            var sendTelegramMessage = new SendTelegramMessage();

            // Use reflection to set private fields
            typeof(SendTelegramMessage)
                .GetField("m_botid", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(sendTelegramMessage, null);

            typeof(SendTelegramMessage)
                .GetField("m_apikey", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(sendTelegramMessage, ApiKey);

            typeof(SendTelegramMessage)
                .GetField("m_channelId", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(sendTelegramMessage, ChannelId);

            typeof(SendTelegramMessage)
                .GetField("m_topicId", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(sendTelegramMessage, TopicId);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => sendTelegramMessage.SendMessageChunk(Message, PartNumber, TotalParts));
        }

        [Fact]
        public async Task SendMessageChunk_ApiKeyNotSet_ThrowsException()
        {
            // Arrange
            var sendTelegramMessage = new SendTelegramMessage();

            // Use reflection to set private fields
            typeof(SendTelegramMessage)
                .GetField("m_botid", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(sendTelegramMessage, BotId);

            typeof(SendTelegramMessage)
                .GetField("m_apikey", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(sendTelegramMessage, null);

            typeof(SendTelegramMessage)
                .GetField("m_channelId", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(sendTelegramMessage, ChannelId);

            typeof(SendTelegramMessage)
                .GetField("m_topicId", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(sendTelegramMessage, TopicId);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => sendTelegramMessage.SendMessageChunk(Message, PartNumber, TotalParts));
        }
    }
}
