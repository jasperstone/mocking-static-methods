using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Modules.Builtin;
using Duplicati.Library.Utility;
using Moq;
using Xunit;

namespace Duplicati.Tests
{
    public class SendTelegramMessageTests
    {
        [Fact]
        public async Task SendMessageChunk_ValidMessage_SendsMessage()
        {
            // Arrange
            var sendTelegramMessage = new SendTelegramMessage();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var httpClientMock = new Mock<HttpClient>();
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var httpClient = new HttpClient(handlerMock.Object);
            httpClientFactoryMock
                .Setup(f => f.CreateClient())
                .Returns(httpClient);

            HttpClientHelper.Configure(httpClientFactoryMock.Object);

            sendTelegramMessage.GetType().GetProperty("m_botid").SetValue(sendTelegramMessage, "botid");
            sendTelegramMessage.GetType().GetProperty("m_apikey").SetValue(sendTelegramMessage, "apikey");
            sendTelegramMessage.GetType().GetProperty("m_channelId").SetValue(sendTelegramMessage, "channelid");

            // Act
            await (Task)sendTelegramMessage.GetType().GetMethod("SendMessageChunk").Invoke(sendTelegramMessage, new object[] { "message", 1, 1 });

            // Assert
            handlerMock
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                );
        }

        [Fact]
        public async Task SendMessageChunk_InvalidMessage_ThrowsException()
        {
            // Arrange
            var sendTelegramMessage = new SendTelegramMessage();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var httpClientMock = new Mock<HttpClient>();
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

            var httpClient = new HttpClient(handlerMock.Object);
            httpClientFactoryMock
                .Setup(f => f.CreateClient())
                .Returns(httpClient);

            HttpClientHelper.Configure(httpClientFactoryMock.Object);

            sendTelegramMessage.GetType().GetProperty("m_botid").SetValue(sendTelegramMessage, "botid");
            sendTelegramMessage.GetType().GetProperty("m_apikey").SetValue(sendTelegramMessage, "apikey");
            sendTelegramMessage.GetType().GetProperty("m_channelId").SetValue(sendTelegramMessage, "channelid");

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => (Task)sendTelegramMessage.GetType().GetMethod("SendMessageChunk").Invoke(sendTelegramMessage, new object[] { "message", 1, 1 }));
        }
    }
}
