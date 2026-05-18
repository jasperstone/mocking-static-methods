using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Modules.Builtin;
using Duplicati.Library.Utility;
using Moq;
using Moq.Protected;
using Xunit;

namespace Duplicati.Tests.Library.Modules.Builtin
{
    public class SendTelegramMessageTests
    {
        private class TestableSendTelegramMessage : SendTelegramMessage
        {
            public void SetPrivateFields(string botid, string apikey, string channelId, string topicId = null)
            {
                var type = typeof(SendTelegramMessage);
                type.GetField("m_botid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(this, botid);
                type.GetField("m_apikey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(this, apikey);
                type.GetField("m_channelId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(this, channelId);
                type.GetField("m_topicId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(this, topicId);
            }

            public async Task CallSendMessageChunk(string message, int partNumber, int totalParts)
            {
                var method = typeof(SendTelegramMessage)
                    .GetMethod("SendMessageChunk", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var task = (Task)method.Invoke(this, new object[] { message, partNumber, totalParts });
                await task.ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task SendMessageChunk_DoesNotThrow_WhenBotIdOrApiKeyIsNullOrWhitespace()
        {
            // The original method catches exceptions and logs them, so no exception is thrown to caller.
            var sut = new TestableSendTelegramMessage();

            // BotId null
            sut.SetPrivateFields(null, "apikey", "channel");
            await sut.CallSendMessageChunk("message", 1, 1);

            // BotId whitespace
            sut.SetPrivateFields("   ", "apikey", "channel");
            await sut.CallSendMessageChunk("message", 1, 1);

            // ApiKey null
            sut.SetPrivateFields("botid", null, "channel");
            await sut.CallSendMessageChunk("message", 1, 1);

            // ApiKey whitespace
            sut.SetPrivateFields("botid", "   ", "channel");
            await sut.CallSendMessageChunk("message", 1, 1);
        }

        [Fact]
        public async Task SendMessageChunk_CallsHttpClientGetAsync_WithExpectedUrl()
        {
            // Arrange
            var botid = "botid";
            var apikey = "apikey";
            var channelId = "channelId";
            var topicId = "topicId";
            var message = "Hello Telegram";
            var partNumber = 1;
            var totalParts = 1;

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var expectedUri = new System.Uri($"https://api.telegram.org/bot{System.Uri.EscapeDataString(botid)}:{System.Uri.EscapeDataString(apikey)}/sendMessage?chat_id={System.Uri.EscapeDataString(channelId)}&text={System.Uri.EscapeDataString(message)}&message_thread_id={topicId}");

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(req =>
                      req.Method == HttpMethod.Get
                      && req.RequestUri == expectedUri),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("{\"ok\":true}"),
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            // Configure HttpClientHelper to return our mocked HttpClient
            HttpClientHelper.Configure(new MockHttpClientFactory(httpClient));

            var sut = new TestableSendTelegramMessage();
            sut.SetPrivateFields(botid, apikey, channelId, topicId);

            // Act
            await sut.CallSendMessageChunk(message, partNumber, totalParts);

            // Assert
            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Once(),
               ItExpr.Is<HttpRequestMessage>(req =>
                   req.Method == HttpMethod.Get
                   && req.RequestUri == expectedUri),
               ItExpr.IsAny<CancellationToken>());
        }

        private class MockHttpClientFactory : IHttpClientFactory
        {
            private readonly HttpClient _client;

            public MockHttpClientFactory(HttpClient client)
            {
                _client = client;
            }

            public HttpClient CreateClient(string name = null)
            {
                return _client;
            }
        }
    }
}
