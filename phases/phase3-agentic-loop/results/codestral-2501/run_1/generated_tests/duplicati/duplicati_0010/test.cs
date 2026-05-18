using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Utility;
using Moq;
using Moq.Protected;
using Xunit;

namespace Duplicati.Library.Modules.Builtin.Tests
{
    public class SendTelegramMessageTests
    {
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

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            HttpClientHelper.Configure(new Mock<IHttpClientFactory>().Object);

            var sendTelegramMessage = new SendTelegramMessage();
            SetPrivateField(sendTelegramMessage, "m_botid", "test_bot_id");
            SetPrivateField(sendTelegramMessage, "m_apikey", "test_api_key");
            SetPrivateField(sendTelegramMessage, "m_channelId", "test_channel_id");
            SetPrivateField(sendTelegramMessage, "m_topicId", "test_topic_id");

            // Act
            await InvokePrivateMethod(sendTelegramMessage, "SendMessageChunk", "Test message", 1, 1);

            // Assert
            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task SendMessageChunk_Failure()
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
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"ok\":false}")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            HttpClientHelper.Configure(new Mock<IHttpClientFactory>().Object);

            var sendTelegramMessage = new SendTelegramMessage();
            SetPrivateField(sendTelegramMessage, "m_botid", "test_bot_id");
            SetPrivateField(sendTelegramMessage, "m_apikey", "test_api_key");
            SetPrivateField(sendTelegramMessage, "m_channelId", "test_channel_id");
            SetPrivateField(sendTelegramMessage, "m_topicId", "test_topic_id");

            // Act
            await InvokePrivateMethod(sendTelegramMessage, "SendMessageChunk", "Test message", 1, 1);

            // Assert
            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(obj, value);
            }
        }

        private async Task InvokePrivateMethod(object obj, string methodName, params object[] parameters)
        {
            var method = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (method != null)
            {
                await (Task)method.Invoke(obj, parameters);
            }
        }
    }
}
