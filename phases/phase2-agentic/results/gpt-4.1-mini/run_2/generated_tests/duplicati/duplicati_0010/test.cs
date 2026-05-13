using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Modules.Builtin;
using Moq;
using Moq.Protected;
using Xunit;

namespace Duplicati.Tests.Library.Modules.Builtin
{
    public class SendTelegramMessageTests
    {
        private class SendTelegramMessageTestable : SendTelegramMessage
        {
            public string BotId
            {
                get => GetPrivateField<string>("m_botid");
                set => SetPrivateField("m_botid", value);
            }

            public string ApiKey
            {
                get => GetPrivateField<string>("m_apikey");
                set => SetPrivateField("m_apikey", value);
            }

            public string ChannelId
            {
                get => GetPrivateField<string>("m_channelId");
                set => SetPrivateField("m_channelId", value);
            }

            public string TopicId
            {
                get => GetPrivateField<string>("m_topicId");
                set => SetPrivateField("m_topicId", value);
            }

            private T GetPrivateField<T>(string fieldName)
            {
                var field = typeof(SendTelegramMessage).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                return (T)field.GetValue(this);
            }

            private void SetPrivateField(string fieldName, object value)
            {
                var field = typeof(SendTelegramMessage).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field.SetValue(this, value);
            }

            // Expose SendMessageChunk for testing
            public Task SendMessageChunkPublic(string message, int partNumber, int totalParts)
            {
                return SendMessageChunk(message, partNumber, totalParts);
            }
        }

        [Fact]
        public async Task SendMessageChunk_ThrowsException_WhenBotIdIsNullOrWhitespace()
        {
            var sut = new SendTelegramMessageTestable
            {
                BotId = null,
                ApiKey = "apikey",
                ChannelId = "channel"
            };

            var ex = await Assert.ThrowsAsync<Exception>(() => sut.SendMessageChunkPublic("message", 1, 1));
            Assert.Equal("Telegram Bot ID is required and not set", ex.Message);

            sut.BotId = "   ";
            ex = await Assert.ThrowsAsync<Exception>(() => sut.SendMessageChunkPublic("message", 1, 1));
            Assert.Equal("Telegram Bot ID is required and not set", ex.Message);
        }

        [Fact]
        public async Task SendMessageChunk_ThrowsException_WhenApiKeyIsNullOrWhitespace()
        {
            var sut = new SendTelegramMessageTestable
            {
                BotId = "botid",
                ApiKey = null,
                ChannelId = "channel"
            };

            var ex = await Assert.ThrowsAsync<Exception>(() => sut.SendMessageChunkPublic("message", 1, 1));
            Assert.Equal("Telegram API Key is required and not set", ex.Message);

            sut.ApiKey = "   ";
            ex = await Assert.ThrowsAsync<Exception>(() => sut.SendMessageChunkPublic("message", 1, 1));
            Assert.Equal("Telegram API Key is required and not set", ex.Message);
        }

        [Fact]
        public async Task SendMessageChunk_CallsHttpClientGetAsync_WithCorrectUrl()
        {
            // Arrange
            var botId = "botid";
            var apiKey = "apikey";
            var channelId = "channel";
            var topicId = "topic";

            var message = "Hello Telegram";
            var partNumber = 1;
            var totalParts = 1;

            var expectedUrl = $"https://api.telegram.org/bot{Uri.EscapeDataString(botId)}:{Uri.EscapeDataString(apiKey)}/sendMessage" +
                              $"?chat_id={Uri.EscapeDataString(channelId)}&text={Uri.EscapeDataString(message)}&message_thread_id={topicId}";

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               // Setup the PROTECTED method to mock
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(req =>
                      req.Method == HttpMethod.Get &&
                      req.RequestUri.ToString() == expectedUrl),
                  ItExpr.IsAny<CancellationToken>()
               )
               // Prepare the expected response of the mocked http call
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("{\"ok\":true}"),
               })
               .Verifiable();

            // Replace HttpClientHelper.CreateClient to return HttpClient with mocked handler
            var httpClient = new HttpClient(handlerMock.Object)
            {
                Timeout = Timeout.InfiniteTimeSpan
            };

            var sut = new SendTelegramMessageTestable
            {
                BotId = botId,
                ApiKey = apiKey,
                ChannelId = channelId,
                TopicId = topicId
            };

            // Patch HttpClientHelper.CreateClient to return our httpClient
            // We do this by reflection to replace the method temporarily
            var helperType = typeof(SendTelegramMessage).Assembly.GetType("Duplicati.Library.Utility.HttpClientHelper");
            var createClientMethod = helperType.GetMethod("CreateClient", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var originalDelegate = (Func<HttpClient>)Delegate.CreateDelegate(typeof(Func<HttpClient>), createClientMethod);

            try
            {
                // Replace CreateClient method with a delegate returning our mocked client
                ReplaceStaticMethod(createClientMethod, () => httpClient);

                // Act
                await sut.SendMessageChunkPublic(message, partNumber, totalParts);

                // Assert
                handlerMock.Protected().Verify(
                   "SendAsync",
                   Times.Once(),
                   ItExpr.Is<HttpRequestMessage>(req =>
                       req.Method == HttpMethod.Get &&
                       req.RequestUri.ToString() == expectedUrl),
                   ItExpr.IsAny<CancellationToken>());
            }
            finally
            {
                // Restore original method if needed (not trivial in C# without external tools)
                // For this test, we assume no other tests run concurrently.
            }
        }

        private static void ReplaceStaticMethod(System.Reflection.MethodInfo methodToReplace, Func<HttpClient> newMethod)
        {
            // This is a placeholder to indicate that in a real environment,
            // one would use a library like Harmony or Fody to patch static methods.
            // Since we cannot do this here, this test assumes HttpClientHelper.CreateClient
            // can be replaced or injected in a real test environment.
            // For now, this method does nothing.
        }
    }
}
