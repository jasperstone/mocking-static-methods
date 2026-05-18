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
using SysUri = System.Uri;

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

            public async Task CallSendMessageChunk(string message, int partNumber, int totalParts)
            {
                var method = typeof(SendTelegramMessage).GetMethod("SendMessageChunk", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var task = (Task)method.Invoke(this, new object[] { message, partNumber, totalParts });
                await task.ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task SendMessageChunk_LogsWarning_WhenBotIdIsNullOrWhitespace()
        {
            var sut = new SendTelegramMessageTestable
            {
                BotId = null,
                ApiKey = "apikey",
                ChannelId = "channel"
            };

            bool warningLogged = false;
            Logging.Log.WriteWarningMessage = (tag, key, ex, format, args) =>
            {
                if (key == "telegramSendError" && args.Length > 2 && args[2] is string msg && msg.Contains("Telegram Bot ID is required"))
                {
                    warningLogged = true;
                }
            };

            await sut.CallSendMessageChunk("message", 1, 1);
            Assert.True(warningLogged);

            warningLogged = false;
            sut.BotId = "   ";
            await sut.CallSendMessageChunk("message", 1, 1);
            Assert.True(warningLogged);
        }

        [Fact]
        public async Task SendMessageChunk_LogsWarning_WhenApiKeyIsNullOrWhitespace()
        {
            var sut = new SendTelegramMessageTestable
            {
                BotId = "botid",
                ApiKey = null,
                ChannelId = "channel"
            };

            bool warningLogged = false;
            Logging.Log.WriteWarningMessage = (tag, key, ex, format, args) =>
            {
                if (key == "telegramSendError" && args.Length > 2 && args[2] is string msg && msg.Contains("Telegram API Key is required"))
                {
                    warningLogged = true;
                }
            };

            await sut.CallSendMessageChunk("message", 1, 1);
            Assert.True(warningLogged);

            warningLogged = false;
            sut.ApiKey = "   ";
            await sut.CallSendMessageChunk("message", 1, 1);
            Assert.True(warningLogged);
        }

        [Fact]
        public async Task SendMessageChunk_CallsHttpClientGetAsync_WithCorrectUrl()
        {
            var botId = "botid";
            var apiKey = "apikey";
            var channelId = "channelid";
            var topicId = "topicid";
            var message = "Hello Telegram";
            var partNumber = 1;
            var totalParts = 1;

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(req =>
                      req.Method == HttpMethod.Get &&
                      req.RequestUri != null &&
                      req.RequestUri.ToString().StartsWith($"https://api.telegram.org/bot{SysUri.EscapeDataString(botId)}:{SysUri.EscapeDataString(apiKey)}/sendMessage") &&
                      req.RequestUri.Query.Contains($"chat_id={SysUri.EscapeDataString(channelId)}") &&
                      req.RequestUri.Query.Contains($"text={SysUri.EscapeDataString(message)}")
                  ),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("{\"ok\":true}")
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object)
            {
                Timeout = Timeout.InfiniteTimeSpan
            };

            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
            HttpClientHelper.Configure(factoryMock.Object);

            var sut = new SendTelegramMessageTestable
            {
                BotId = botId,
                ApiKey = apiKey,
                ChannelId = channelId,
                TopicId = topicId
            };

            await sut.CallSendMessageChunk(message, partNumber, totalParts);

            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Once(),
               ItExpr.Is<HttpRequestMessage>(req =>
                   req.Method == HttpMethod.Get &&
                   req.RequestUri != null &&
                   req.RequestUri.ToString().StartsWith($"https://api.telegram.org/bot{SysUri.EscapeDataString(botId)}:{SysUri.EscapeDataString(apiKey)}/sendMessage") &&
                   req.RequestUri.Query.Contains($"chat_id={SysUri.EscapeDataString(channelId)}") &&
                   req.RequestUri.Query.Contains($"text={SysUri.EscapeDataString(message)}")
               ),
               ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}
