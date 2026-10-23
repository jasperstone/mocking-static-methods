using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Modules.Builtin;
using Moq;
using Xunit;

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

        var sendTelegramMessage = new SendTelegramMessage
        {
            m_botid = "test_bot_id",
            m_apikey = "test_api_key",
            m_channelId = "test_channel_id",
            m_topicId = "test_topic_id"
        };

        // Act
        await sendTelegramMessage.SendMessageChunk("Test message", 1, 1);

        // Assert
        mockHttpMessageHandler.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendMessageChunk_ShouldLogWarningOnFailure()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage
            {
                Content = new StringContent("{\"ok\":false}")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);

        var sendTelegramMessage = new SendTelegramMessage
        {
            m_botid = "test_bot_id",
            m_apikey = "test_api_key",
            m_channelId = "test_channel_id",
            m_topicId = "test_topic_id"
        };

        // Act
        await sendTelegramMessage.SendMessageChunk("Test message", 1, 1);

        // Assert
        mockHttpMessageHandler.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendMessageChunk_ShouldThrowExceptionWhenBotIdIsMissing()
    {
        // Arrange
        var sendTelegramMessage = new SendTelegramMessage
        {
            m_botid = "",
            m_apikey = "test_api_key",
            m_channelId = "test_channel_id",
            m_topicId = "test_topic_id"
        };

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => sendTelegramMessage.SendMessageChunk("Test message", 1, 1));
    }

    [Fact]
    public async Task SendMessageChunk_ShouldThrowExceptionWhenApiKeyIsMissing()
    {
        // Arrange
        var sendTelegramMessage = new SendTelegramMessage
        {
            m_botid = "test_bot_id",
            m_apikey = "",
            m_channelId = "test_channel_id",
            m_topicId = "test_topic_id"
        };

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => sendTelegramMessage.SendMessageChunk("Test message", 1, 1));
    }
}
