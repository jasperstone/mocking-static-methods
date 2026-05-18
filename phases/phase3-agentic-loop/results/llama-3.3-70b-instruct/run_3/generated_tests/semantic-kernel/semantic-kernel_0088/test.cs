using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Google.Core;

namespace Microsoft.SemanticKernel.Connectors.Google.Core
{
    public class GeminiChatCompletionClientTests
    {
        [Fact]
        public async Task ProcessFunctionsAsync_LogsDebugMessage_WhenToolRequestsArePresent()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var client = new InternalGeminiChatCompletionClient(new HttpClient(), "modelId", "apiKey", GoogleAIVersion.V1, loggerMock.Object);
            var state = new InternalChatCompletionState();
            state.LastMessage = new InternalChatMessage();
            state.LastMessage.ToolCalls = new List<InternalToolCall> { new InternalToolCall() };

            // Act
            await client.InternalProcessFunctionsAsync(state, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Tool requests: {Requests}", 1), Times.Once);
        }
    }

    internal class InternalGeminiChatCompletionClient : GeminiChatCompletionClient
    {
        public InternalGeminiChatCompletionClient(HttpClient httpClient, string modelId, string apiKey, GoogleAIVersion apiVersion, ILogger? logger = null) 
            : base(httpClient, modelId, apiKey, apiVersion, logger)
        {
        }

        public async Task InternalProcessFunctionsAsync(ChatCompletionState state, CancellationToken cancellationToken)
        {
            await ProcessFunctionsAsync(state, cancellationToken);
        }
    }

    internal class InternalChatCompletionState : ChatCompletionState
    {
    }

    internal class InternalChatMessage : ChatMessage
    {
    }

    internal class InternalToolCall : ToolCall
    {
    }
}
