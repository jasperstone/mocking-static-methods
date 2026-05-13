using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.Tests
{
    public class GeminiChatCompletionClientTests
    {
        [Fact]
        public async Task ProcessFunctionsAsync_LogsToolRequests()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var stateMock = new Mock<ChatCompletionState>();
            stateMock.Setup(s => s.LastMessage).Returns(new ChatMessage { ToolCalls = new List<ToolCall> { new ToolCall(), new ToolCall() } });
            var client = new GeminiChatCompletionClient(new HttpClient(), "modelId", "apiKey", GoogleAIVersion.V1, loggerMock.Object);

            // Act
            await client.ProcessFunctionsAsync(stateMock.Object, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Tool requests: {Requests}", 2), Times.Once);
        }

        [Fact]
        public async Task ProcessFunctionsAsync_LogsFunctionCallRequests()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
            var stateMock = new Mock<ChatCompletionState>();
            stateMock.Setup(s => s.LastMessage).Returns(new ChatMessage { ToolCalls = new List<ToolCall> { new ToolCall(), new ToolCall() } });
            var client = new GeminiChatCompletionClient(new HttpClient(), "modelId", "apiKey", GoogleAIVersion.V1, loggerMock.Object);

            // Act
            await client.ProcessFunctionsAsync(stateMock.Object, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogTrace("Function call requests: {FunctionCall}", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcessFunctionsAsync_LogsFilterTerminationRequested()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
            var stateMock = new Mock<ChatCompletionState>();
            stateMock.Setup(s => s.LastMessage).Returns(new ChatMessage { ToolCalls = new List<ToolCall> { new ToolCall() } });
            var client = new GeminiChatCompletionClient(new HttpClient(), "modelId", "apiKey", GoogleAIVersion.V1, loggerMock.Object);

            // Act
            await client.ProcessFunctionsAsync(stateMock.Object, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Filter requested termination of automatic function invocation."), Times.Once);
        }
    }
}
