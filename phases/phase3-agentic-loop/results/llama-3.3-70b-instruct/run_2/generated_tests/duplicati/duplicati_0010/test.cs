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
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"ok\":true}")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(x => x.CreateClient()).Returns(httpClient);

            HttpClientHelper.Configure(httpClientFactoryMock.Object);

            var sendTelegramMessage = new SendTelegramMessage();
            // You need to set the properties or fields of sendTelegramMessage using the methods or properties provided by the class

            // Act
            await sendTelegramMessage.SendMessageChunk("message", 1, 1);

            // Assert
            handlerMock
                .Verify(
                    h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
                    Times.Once()
                );
        }

        [Fact]
        public async Task SendMessageChunk_InvalidMessage_DoesNotSendMessage()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"ok\":true}")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(x => x.CreateClient()).Returns(httpClient);

            HttpClientHelper.Configure(httpClientFactoryMock.Object);

            var sendTelegramMessage = new SendTelegramMessage();
            // You need to set the properties or fields of sendTelegramMessage using the methods or properties provided by the class

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => sendTelegramMessage.SendMessageChunk("message", 1, 1));
            handlerMock
                .Verify(
                    h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
                    Times.Never()
                );
        }
    }
}
