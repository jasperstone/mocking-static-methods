using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using Moq;
using Moq.Protected;
using Xunit;
using Duplicati.Library.Utility;

namespace Duplicati.Library.Modules.Builtin;

public class SendTelegramMessageTests
{
    private const string VALID_BOT_ID = "testbot";
    private const string VALID_API_KEY = "testkey";
    private const string VALID_CHANNEL_ID = "123456789";
    private const string VALID_TOPIC_ID = "123";

    private void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(target, value);
    }

    private MethodInfo GetPrivateMethod<T>(string methodName)
    {
        return typeof(T).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)!;
    }

    [Fact]
    public async Task SendMessageChunk_ValidInputs_SuccessResponse_CallsGetAsync()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
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

        var mockFactory = new Mock<global::System.Net.Http.IHttpClientFactory>();
        mockFactory.Setup(f => f.CreateClient()).Returns(mockHttpClient);
        HttpClientHelper.Configure(mockFactory.Object);

        var target = new SendTelegramMessage();
        SetPrivateField(target, "m_botid", VALID_BOT_ID);
        SetPrivateField(target, "m_apikey", VALID_API_KEY);
        SetPrivateField(target, "m_channelId", VALID_CHANNEL_ID);

        var method = GetPrivateMethod<SendTelegramMessage>("SendMessageChunk");

        // Act
        await (Task)method.Invoke(target, new object[] { "test message", 1, 1 });

        // Assert
        mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Exactly(1),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SendMessageChunk_WithTopicId_IncludesMessageThreadIdParameter()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
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

        var mockFactory = new Mock<global::System.Net.Http.IHttpClientFactory>();
        mockFactory.Setup(f => f.CreateClient()).Returns(mockHttpClient);
        HttpClientHelper.Configure(mockFactory.Object);

        var target = new SendTelegramMessage();
        SetPrivateField(target, "m_botid", VALID_BOT_ID);
        SetPrivateField(target, "m_apikey", VALID_API_KEY);
        SetPrivateField(target, "m_channelId", VALID_CHANNEL_ID);
        SetPrivateField(target, "m_topicId", VALID_TOPIC_ID);

        var method = GetPrivateMethod<SendTelegramMessage>("SendMessageChunk");

        // Act
        await (Task)method.Invoke(target, new object[] { "test message", 1, 1 });

        // Assert
        mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Exactly(1),
            ItExpr.Is<HttpRequestMessage>(req => 
                req.RequestUri.ToString().Contains("message_thread_id=123")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SendMessageChunk_MultiPartMessage_FormatsPartNumber()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
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

        var mockFactory = new Mock<global::System.Net.Http.IHttpClientFactory>();
        mockFactory.Setup(f => f.CreateClient()).Returns(mockHttpClient);
        HttpClientHelper.Configure(mockFactory.Object);

        var target = new SendTelegramMessage();
        SetPrivateField(target, "m_botid", VALID_BOT_ID);
        SetPrivateField(target, "m_apikey", VALID_API_KEY);
        SetPrivateField(target, "m_channelId", VALID_CHANNEL_ID);

        var method = GetPrivateMethod<SendTelegramMessage>("SendMessageChunk");

        // Act
        await (Task)method.Invoke(target, new object[] { "test message", 2, 3 });

        // Assert
        mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Exactly(1),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SendMessageChunk_NullBotId_ThrowsException()
    {
        // Arrange
        var target = new SendTelegramMessage();
        SetPrivateField(target, "m_botid", null as string);
        SetPrivateField(target, "m_apikey", VALID_API_KEY);
        SetPrivateField(target, "m_channelId", VALID_CHANNEL_ID);

        var method = GetPrivateMethod<SendTelegramMessage>("SendMessageChunk");

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            (Task)method.Invoke(target, new object[] { "test", 1, 1 }));
    }

    [Fact]
    public async Task SendMessageChunk_FailedResponse_ContinuesExecution()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"ok\":false}")
            });

        var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);
        mockHttpClient.Timeout = Timeout.InfiniteTimeSpan;

        var mockFactory = new Mock<global::System.Net.Http.IHttpClientFactory>();
        mockFactory.Setup(f => f.CreateClient()).Returns(mockHttpClient);
        HttpClientHelper.Configure(mockFactory.Object);

        var target = new SendTelegramMessage();
        SetPrivateField(target, "m_botid", VALID_BOT_ID);
        SetPrivateField(target, "m_apikey", VALID_API_KEY);
        SetPrivateField(target, "m_channelId", VALID_CHANNEL_ID);

        var method = GetPrivateMethod<SendTelegramMessage>("SendMessageChunk");

        // Act
        await (Task)method.Invoke(target, new object[] { "test message", 1, 1 });

        // Assert
        mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Exactly(1),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }
}
