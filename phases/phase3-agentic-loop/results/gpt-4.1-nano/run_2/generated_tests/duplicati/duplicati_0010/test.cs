using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using System.Net;
using System.Text;
using Duplicati.Library.Modules.Builtin;

namespace Duplicati.Tests
{
    public class SendTelegramMessageTests
    {
        private class TestSendTelegramMessage : SendTelegramMessage
        {
            public HttpClient HttpClientToUse { get; set; }

            protected override HttpClient CreateHttpClient()
            {
                return HttpClientToUse ?? base.CreateHttpClient();
            }
        }

        [Fact]
        public async Task SendMessageChunk_CallsGetAsync_WithCorrectUrl()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var responseContent = new StringContent("{\"ok\":true}");
            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(async (req) =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = responseContent
                    };
                    return response;
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var sendTelegram = new TestSendTelegramMessage
            {
                HttpClientToUse = httpClient
            };

            // Set required fields via reflection
            var type = typeof(SendTelegramMessage);
            type.GetField("m_botid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(sendTelegram, "botid");
            type.GetField("m_apikey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(sendTelegram, "apikey");
            type.GetField("m_channelId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(sendTelegram, "channel");
            type.GetField("m_topicId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(sendTelegram, "topic");

            // Act
            await sendTelegram.SendMessageChunk("Test message", 1, 1);

            // Assert
            mockHttpMessageHandler.Verify(m => m.Send(It.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get &&
                req.RequestUri.ToString().Contains("sendMessage")
            )), Times.Once);
        }
    }
}
