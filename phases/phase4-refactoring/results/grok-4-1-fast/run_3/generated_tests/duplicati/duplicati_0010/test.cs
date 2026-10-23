using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Modules.Builtin;
using Moq;
using Moq.Protected;
using Xunit;
using System.Reflection;

namespace Duplicati.Library.Modules.Builtin.Tests;

public class SendTelegramMessageTests
{
    private const string VALID_BOT_ID = "testbot";
    private const string VALID_API_KEY = "testkey";
    private const string VALID_CHANNEL_ID = "123456789";
    private const string VALID_TOPIC_ID = "123";

    [Fact]
    public async Task SendMessageChunk_ValidParameters_SuccessResponse_NoException()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"ok\":true}")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        httpClient.Timeout = Timeout.InfiniteTimeSpan;

        var target = new TestableSendTelegramMessage();
        target.SetPrivateFields(VALID_BOT_ID, VALID_API_KEY, VALID_CHANNEL_ID, null);
        target.SetHttpClient(httpClient);

        // Act
        await target.CallSendMessageChunkAsync("test message", 1, 1);

        // Assert
        mockHandler.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString().Contains("sendMessage")));
    }

    [Fact]
    public async Task SendMessageChunk_WithTopicId_IncludesMessageThreadId()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"ok\":true}")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        httpClient.Timeout = Timeout.InfiniteTimeSpan;

        var target = new TestableSendTelegramMessage();
        target.SetPrivateFields(VALID_BOT_ID, VALID_API_KEY, VALID_CHANNEL_ID, VALID_TOPIC_ID);
        target.SetHttpClient(httpClient);

        // Act
        await target.CallSendMessageChunkAsync("test message", 1, 1);

        // Assert
        mockHandler.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => 
                req.RequestUri.ToString().Contains("message_thread_id=") && 
                req.RequestUri.ToString().Contains("123")));
    }

    [Fact]
    public async Task SendMessageChunk_MultipleParts_FormatsMessageCorrectly()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"ok\":true}")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        httpClient.Timeout = Timeout.InfiniteTimeSpan;

        var target = new TestableSendTelegramMessage();
        target.SetPrivateFields(VALID_BOT_ID, VALID_API_KEY, VALID_CHANNEL_ID, null);
        target.SetHttpClient(httpClient);

        // Act
        await target.CallSendMessageChunkAsync("test message", 1, 2);

        // Assert
        mockHandler.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => 
                req.RequestUri.ToString().Contains("Part%201%2F2")));
    }

    [Fact]
    public void SendMessageChunk_NullBotId_ThrowsException()
    {
        // Arrange
        var target = new TestableSendTelegramMessage();
        target.SetPrivateFields(null, VALID_API_KEY, VALID_CHANNEL_ID, null);
        target.SetHttpClient(new HttpClient());

        // Act & Assert
        Assert.ThrowsAnyAsync<Exception>(() => target.CallSendMessageChunkAsync("test", 1, 1));
    }

    [Fact]
    public void SendMessageChunk_EmptyApiKey_ThrowsException()
    {
        // Arrange
        var target = new TestableSendTelegramMessage();
        target.SetPrivateFields(VALID_BOT_ID, "", VALID_CHANNEL_ID, null);
        target.SetHttpClient(new HttpClient());

        // Act & Assert
        Assert.ThrowsAnyAsync<Exception>(() => target.CallSendMessageChunkAsync("test", 1, 1));
    }

    private class TestableSendTelegramMessage : SendTelegramMessage
    {
        private HttpClient _testHttpClient;

        public async Task CallSendMessageChunkAsync(string message, int partNumber, int totalParts)
        {
            await SendMessageChunk(message, partNumber, totalParts);
        }

        public void SetPrivateFields(string botid, string apikey, string channelId, string topicId)
        {
            var botIdField = typeof(SendTelegramMessage).GetField("m_botid", BindingFlags.NonPublic | BindingFlags.Instance);
            var apiKeyField = typeof(SendTelegramMessage).GetField("m_apikey", BindingFlags.NonPublic | BindingFlags.Instance);
            var channelIdField = typeof(SendTelegramMessage).GetField("m_channelId", BindingFlags.NonPublic | BindingFlags.Instance);
            var topicIdField = typeof(SendTelegramMessage).GetField("m_topicId", BindingFlags.NonPublic | BindingFlags.Instance);

            botIdField?.SetValue(this, botid);
            apiKeyField?.SetValue(this, apikey);
            channelIdField?.SetValue(this, channelId);
            topicIdField?.SetValue(this, topicId);
        }

        public void SetHttpClient(HttpClient client)
        {
            var clientField = typeof(SendTelegramMessage).GetField("client", BindingFlags.NonPublic | BindingFlags.Instance);
            clientField?.SetValue(this, client);
        }
    }
}
