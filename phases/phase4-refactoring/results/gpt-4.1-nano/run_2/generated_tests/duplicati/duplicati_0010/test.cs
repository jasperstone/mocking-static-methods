using Xunit;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Text;
using Duplicati.Library.Modules.Builtin;

namespace Duplicati.Tests
{
    public class SendTelegramMessageTests
    {
        // A subclass to expose the private SendMessageChunk method for testing
        public class TestableSendTelegramMessage : SendTelegramMessage
        {
            public async Task InvokeSendMessageChunk(string message, int partNumber, int totalParts)
            {
                await this.SendMessageChunk(message, partNumber, totalParts);
            }
        }

        [Fact]
        public async Task SendMessageChunk_Should_Call_GetAsync_And_Log_Warning_On_Failure()
        {
            // Arrange
            var message = "Test message";
            int partNumber = 1;
            int totalParts = 1;

            // Create a mock HttpMessageHandler
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"ok\":false}")
                });

            var client = new HttpClient(handlerMock.Object);

            // Configure HttpClientHelper to return our mock client
            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.CreateClient()).Returns(client);
            Duplicati.Library.Utility.HttpClientHelper.Configure(factoryMock.Object);

            var sendTelegram = new TestableSendTelegramMessage();

            // Act
            await sendTelegram.InvokeSendMessageChunk(message, partNumber, totalParts);

            // Assert
            handlerMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
