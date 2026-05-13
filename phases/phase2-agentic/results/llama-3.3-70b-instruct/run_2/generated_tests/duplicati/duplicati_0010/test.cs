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
            sendTelegramMessage.m_botid = "botid";
            sendTelegramMessage.m_apikey = "apikey";
            sendTelegramMessage.m_channelId = "channelid";
            var message = "Hello, World!";
            var partNumber = 1;
            var totalParts = 1;

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
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

            var httpClient = new HttpClient(handlerMock.Object);
            HttpClientHelper.Configure(new Mock<IHttpClientFactory>().Object);
            HttpClientHelper._factory = new Mock<IHttpClientFactory>().Object;

            // Act
            await sendTelegramMessage.SendMessageChunk(message, partNumber, totalParts);

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
            sendTelegramMessage.m_botid = "botid";
            sendTelegramMessage.m_apikey = "apikey";
            sendTelegramMessage.m_channelId = "channelid";
            var message = "Hello, World!";
            var partNumber = 1;
            var totalParts = 1;

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
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

            var httpClient = new HttpClient(handlerMock.Object);
            HttpClientHelper.Configure(new Mock<IHttpClientFactory>().Object);
            HttpClientHelper._factory = new Mock<IHttpClientFactory>().Object;

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => sendTelegramMessage.SendMessageChunk(message, partNumber, totalParts));
        }
    }
}
