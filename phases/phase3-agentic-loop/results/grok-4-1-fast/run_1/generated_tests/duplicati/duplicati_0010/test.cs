using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Modules.Builtin;
using Duplicati.Library.Utility;
using Moq;
using Moq.Protected;
using Xunit;
using System.Linq.Expressions;

namespace Duplicati.Library.Modules.Builtin.Tests;

public class SendTelegramMessageTests
{
    private const string BOT_ID = "test-bot-id";
    private const string API_KEY = "test-api-key";
    private const string CHANNEL_ID = "test-channel-id";
    private const string TOPIC_ID = "test-topic-id";

    [Fact]
    public async Task SendMessageChunk_SuccessfulResponse_CallsHttpClient()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"ok\":true}")
            });

        var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);
        mockHttpClient.Timeout = Timeout.InfiniteTimeSpan;

        // Mock HttpClientHelper static method using reflection
        var createClientMethod = typeof(HttpClientHelper).GetMethod("CreateClient", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)!;
        var mockHttpClientField = new Mock<HttpClient>();
        mockHttpClientField.SetupProperty(c => c.Timeout);
        mockHttpClientField.Object.Timeout = Timeout.InfiniteTimeSpan;
        
        // Use delegate replacement approach
        var field = typeof(HttpClientHelper).GetField("_factory", BindingFlags.NonPublic | BindingFlags.Static);
        var originalFactory = field?.GetValue(null);
        var mockFactory = new Mock<global::Duplicati.Library.Utility.IHttpClientFactory>();
        mockFactory.Setup(f => f.CreateClient()).Returns(mockHttpClient);
        field?.SetValue(null, mockFactory.Object);

        var target = new PrivateSendTelegramMessage();
        SetPrivateFields(target, BOT_ID, API_KEY, CHANNEL_ID, TOPIC_ID);

        // Act
        await target.SendMessageChunk("test message", 1, 1);

        // Assert
        mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());

        // Cleanup
        field?.SetValue(null, originalFactory);
    }

    [Fact]
    public async Task SendMessageChunk_MissingBotId_ThrowsException()
    {
        // Arrange
        var target = new SendTelegramMessage();
        SetPrivateFields(target, null!, API_KEY, CHANNEL_ID, TOPIC_ID);

        // Act & Assert
        var ex = await Assert.ThrowsAnyAsync<Exception>(() => CallPrivateMethod(target, "test", 1, 1));
        Assert.Equal("Telegram Bot ID is required and not set", ex.Message);
    }

    [Fact]
    public async Task SendMessageChunk_MissingApiKey_ThrowsException()
    {
        // Arrange
        var target = new SendTelegramMessage();
        SetPrivateFields(target, BOT_ID, null!, CHANNEL_ID, TOPIC_ID);

        // Act & Assert
        var ex = await Assert.ThrowsAnyAsync<Exception>(() => CallPrivateMethod(target, "test", 1, 1));
        Assert.Equal("Telegram API Key is required and not set", ex.Message);
    }

    [Fact]
    public async Task SendMessageChunk_MultiPartMessage_FormatsCorrectly()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);
        mockHttpClient.Timeout = Timeout.InfiniteTimeSpan;

        var field = typeof(HttpClientHelper).GetField("_factory", BindingFlags.NonPublic | BindingFlags.Static);
        var originalFactory = field?.GetValue(null);
        var mockFactory = new Mock<global::Duplicati.Library.Utility.IHttpClientFactory>();
        mockFactory.Setup(f => f.CreateClient()).Returns(mockHttpClient);
        field?.SetValue(null, mockFactory.Object);

        var target = new SendTelegramMessage();
        SetPrivateFields(target, BOT_ID, API_KEY, CHANNEL_ID, null);

        // Act
        await CallPrivateMethod(target, "test message", 2, 3);

        // Assert - verify the call happened with correct formatting
        mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => 
                req.RequestUri.ToString().Contains("Part%202%2F3%3A")),
            ItExpr.IsAny<CancellationToken>());

        // Cleanup
        field?.SetValue(null, originalFactory);
    }

    private static async Task CallPrivateMethod(SendTelegramMessage target, string message, int partNumber, int totalParts)
    {
        var method = typeof(SendTelegramMessage).GetMethod("SendMessageChunk", 
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        await (Task)method.Invoke(target, new object[] { message, partNumber, totalParts })!;
    }

    private static void SetPrivateFields(SendTelegramMessage target, string botId, string apiKey, string channelId, string topicId)
    {
        typeof(SendTelegramMessage)
            .GetField("m_botid", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(target, botId);
        typeof(SendTelegramMessage)
            .GetField("m_apikey", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(target, apiKey);
        typeof(SendTelegramMessage)
            .GetField("m_channelId", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(target, channelId);
        typeof(SendTelegramMessage)
            .GetField("m_topicId", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(target, topicId);
    }
}
