using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Google.Core;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.Tests
{
    public class GeminiChatCompletionClientTests
    {
        [Fact]
        public async Task GenerateChatMessageAsync_LogsToolRequestsDebugMessage_WhenDebugEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
            loggerMock.Setup(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Tool requests")),
                null,
                It.IsAny<Func<It.IsAnyType, System.Exception?, string>>()));

            var httpClient = new HttpClient();
            var client = new GeminiChatCompletionClient(
                httpClient,
                "test-model",
                "test-api-key",
                GoogleAIVersion.V1,
                loggerMock.Object);

            // Act
            // Call with null or minimal parameters to trigger the code path
            await client.GenerateChatMessageAsync(null);

            // Assert
            loggerMock.Verify(l => l.IsEnabled(LogLevel.Debug), Times.AtLeastOnce);
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Tool requests")),
                null,
                It.IsAny<Func<It.IsAnyType, System.Exception?, string>>()), Times.AtLeastOnce);
        }
    }

    internal enum GoogleAIVersion
    {
        V1
    }
}
