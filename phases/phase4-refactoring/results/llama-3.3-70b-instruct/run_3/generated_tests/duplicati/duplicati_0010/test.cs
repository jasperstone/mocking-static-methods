using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Modules.Builtin;
using Moq;
using Xunit;

namespace Duplicati.Tests
{
    public class SendTelegramMessageTests
    {
        [Fact]
        public async Task SendMessageChunk_SendsMessage()
        {
            // Arrange
            var sendTelegramMessage = new SendTelegramMessage();
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            handlerMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>()))
                .ReturnsAsync(response);

            var httpClient = new HttpClient(handlerMock.Object);

            // Act
            await sendTelegramMessage.SendMessageChunk("message", 1, 1);

            // Assert
            handlerMock.Verify(
                h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>()),
                Times.Once
            );
        }
    }
}
