using Xunit;
using Moq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Utility;
using System;
using System.Reflection;

namespace Duplicati.Library.Modules.Builtin.Tests
{
    public class SendTelegramMessageTests
    {
        [Fact]
        public async Task SendMessageChunk_ShouldSendMessageSuccessfully()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StringContent("{\"ok\":true}")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            HttpClientHelper.Configure(new Mock<IHttpClientFactory>().Object);

            var sendTelegramMessage = new SendTelegramMessage();
            typeof(SendTelegramMessage)
                .GetField("m_botid", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(sendTelegramMessage, "test_bot_id");
            typeof(SendTelegramMessage)
                .GetField("m_apikey", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(sendTelegramMessage, "test_api_key");
            typeof(SendTelegramMessage)
                .GetField("m_channelId", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(sendTelegramMessage, "test_channel_id");
            typeof(SendTelegramMessage)
                .GetField("m_topicId", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(sendTelegramMessage, "test_topic_id");

            // Act
            await (Task)sendTelegramMessage.GetType()
                .GetMethod("SendMessageChunk", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(sendTelegramMessage, new object[] { "Test message", 1, 1 });

            // Assert
            mockHttpMessageHandler.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SendMessageChunk_ShouldThrowException_WhenBotIdIsNotSet()
        {
            // Arrange
            var sendTelegramMessage = new SendTelegramMessage();
            typeof(SendTelegramMessage)
                .GetField("m_botid", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(sendTelegramMessage, "");
            typeof(SendTelegramMessage)
                .GetField("m_apikey", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(sendTelegramMessage, "test_api_key");
            typeof(SendTelegramMessage)
                .GetField("m_channelId", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(sendTelegramMessage, "test_channel_id");
            typeof(SendTelegramMessage)
                .GetField("m_topicId", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(sendTelegramMessage, "test_topic_id");

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => (Task)sendTelegramMessage.GetType()
                .GetMethod("SendMessageChunk", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(sendTelegramMessage, new object[] { "Test message", 1, 1 }));
        }

        [Fact]
        public async Task SendMessageChunk_ShouldThrowException_WhenApiKeyIsNotSet()
        {
            // Arrange
            var sendTelegramMessage = new SendTelegramMessage();
            typeof(SendTelegramMessage)
                .GetField("m_botid", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(sendTelegramMessage, "test_bot_id");
            typeof(SendTelegramMessage)
                .GetField("m_apikey", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(sendTelegramMessage, "");
            typeof(SendTelegramMessage)
                .GetField("m_channelId", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(sendTelegramMessage, "test_channel_id");
            typeof(SendTelegramMessage)
                .GetField("m_topicId", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(sendTelegramMessage, "test_topic_id");

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => (Task)sendTelegramMessage.GetType()
                .GetMethod("SendMessageChunk", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(sendTelegramMessage, new object[] { "Test message", 1, 1 }));
        }
    }
}
