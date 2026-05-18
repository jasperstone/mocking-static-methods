using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Modules.Builtin;
using Moq;
using Moq.Protected;
using Xunit;
using System.Collections.Generic;

namespace Duplicati.Library.Modules.Builtin.Tests;

public class SendTelegramMessageTests
{
    [Fact]
    public async Task SendMessageChunk_MissingBotId_ThrowsException()
    {
        // Arrange
        var sendTelegram = new SendTelegramMessage();
        SetPrivateField(sendTelegram, "m_botid", "");
        SetPrivateField(sendTelegram, "m_apikey", "testkey");
        SetPrivateField(sendTelegram, "m_channelId", "testchannel");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => InvokePrivateMethod<Task>(sendTelegram, "SendMessageChunk", "test", 1, 1));
        Assert.Equal("Telegram Bot ID is required and not set", ex.Message);
    }

    [Fact]
    public async Task SendMessageChunk_MissingApiKey_ThrowsException()
    {
        // Arrange
        var sendTelegram = new SendTelegramMessage();
        SetPrivateField(sendTelegram, "m_botid", "testbot");
        SetPrivateField(sendTelegram, "m_apikey", "");
        SetPrivateField(sendTelegram, "m_channelId", "testchannel");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => InvokePrivateMethod<Task>(sendTelegram, "SendMessageChunk", "test", 1, 1));
        Assert.Equal("Telegram API Key is required and not set", ex.Message);
    }

    [Fact]
    public async Task SendMessageChunk_SinglePart_CallsHttpClientGetAsync()
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

        var sendTelegram = new SendTelegramMessage();
        SetPrivateField(sendTelegram, "m_botid", "testbot");
        SetPrivateField(sendTelegram, "m_apikey", "testkey");
        SetPrivateField(sendTelegram, "m_channelId", "testchannel");
        SetPrivateField(sendTelegram, "m_topicId", "");

        // Replace HttpClientHelper._factory with mock that returns our client
        var mockFactory = new Mock<IHttpClientFactory>();
        mockFactory.Setup(f => f.CreateClient()).Returns(httpClient);
        SetStaticField(typeof(Duplicati.Library.Utility.HttpClientHelper), "_factory", mockFactory.Object);

        // Act
        await InvokePrivateMethod<Task>(sendTelegram, "SendMessageChunk", "test message", 1, 1);

        // Assert - verifies GetAsync call on line 283 was executed with correct parameters
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().Contains("sendMessage") &&
                    req.RequestUri.ToString().Contains("testbot:testkey") &&
                    req.RequestUri.ToString().Contains("testchannel")));
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

        var sendTelegram = new SendTelegramMessage();
        SetPrivateField(sendTelegram, "m_botid", "testbot");
        SetPrivateField(sendTelegram, "m_apikey", "testkey");
        SetPrivateField(sendTelegram, "m_channelId", "testchannel");

        var mockFactory = new Mock<IHttpClientFactory>();
        mockFactory.Setup(f => f.CreateClient()).Returns(httpClient);
        SetStaticField(typeof(Duplicati.Library.Utility.HttpClientHelper), "_factory", mockFactory.Object);

        // Act
        await InvokePrivateMethod<Task>(sendTelegram, "SendMessageChunk", "test message", 1, 2);

        // Assert - verifies part formatting reaches the GetAsync call (line 283)
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString().Contains("Part%201%2F2")));
    }

    private static T InvokePrivateMethod<T>(object obj, string methodName, params object[] args)
    {
        var method = obj.GetType().GetMethod(methodName, 
            BindingFlags.NonPublic | BindingFlags.Instance);
        return (T)method.Invoke(obj, args)!;
    }

    private static void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, 
            BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(obj, value);
    }

    private static void SetStaticField(Type type, string fieldName, object value)
    {
        var field = type.GetField(fieldName, 
            BindingFlags.NonPublic | BindingFlags.Static);
        field?.SetValue(null, value);
    }
}

public interface IHttpClientFactory
{
    System.Net.Http.HttpClient CreateClient();
}
