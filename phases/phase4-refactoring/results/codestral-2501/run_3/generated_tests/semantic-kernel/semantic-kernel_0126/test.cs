using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.SemanticKernel.Connectors.MistralAI.Client;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MistralClientTests
{
    public class MistralClientTests
    {
        [Fact]
        public async Task GetChatMessageContentsAsync_LogsDebug_WhenDebugEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MistralClient>>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
            var chatHistory = new ChatHistory();
            var cancellationToken = CancellationToken.None;
            var mistralClient = new MistralClient("modelId", new System.Net.Http.HttpClient(), "apiKey", logger: mockLogger.Object);

            // Act
            await mistralClient.GetChatMessageContentsAsync(chatHistory, cancellationToken);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Debug),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
