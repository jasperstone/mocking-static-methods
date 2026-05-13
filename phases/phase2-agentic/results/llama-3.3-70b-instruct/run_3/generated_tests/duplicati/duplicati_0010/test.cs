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
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(_ => _.CreateClient()).Returns(httpClient);

            HttpClientHelper.Configure(httpClientFactoryMock.Object);

            var sendTelegramMessage = new SendTelegramMessage();
            sendTelegramMessage.m_botid = "botid";
            sendTelegramMessage.m_apikey = "apikey";
            sendTelegramMessage.m_channelId = "channelid";
            sendTelegramMessage.m_topicId = "topicid";

            // Act
            await sendTelegramMessage.SendMessageChunk("message", 1, 1);

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
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(_ => _.CreateClient()).Returns(httpClient);

            HttpClientHelper.Configure(httpClientFactoryMock.Object);

            var sendTelegramMessage = new SendTelegramMessage();
            sendTelegramMessage.m_botid = "botid";
            sendTelegramMessage.m_apikey = "apikey";
            sendTelegramMessage.m_channelId = "channelid";
            sendTelegramMessage.m_topicId = "topicid";

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => sendTelegramMessage.SendMessageChunk("message", 1, 1));
        }
    }
}
