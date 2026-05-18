using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.UnitTests.Gemini.Clients;

public sealed class GeminiChatCompletionClientTests
{
    [Fact]
    public async Task ProcessFunctionsAsync_LogsDebugMessage_WhenDebugEnabledAndToolCallsPresent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        
        var mockHttpClient = new Mock<HttpClient>();
        var clientType = Type.GetType("Microsoft.SemanticKernel.Connectors.Google.Core.Gemini.Clients.GeminiChatCompletionClient", true)!;
        
        // Create client instance using reflection (assuming GoogleAIVersion has enum values starting from 0)
        var client = (dynamic)Activator.CreateInstance(clientType, mockHttpClient.Object, "test-model", "test-api-key", 0, loggerMock.Object)!;

        // Create state with LastMessage and ToolCalls using reflection or minimal types
        var stateType = Type.GetType("Microsoft.SemanticKernel.Connectors.Google.Core.Gemini.Clients.ChatCompletionState", throwOnError: false) 
                       ?? throw new InvalidOperationException("ChatCompletionState type not found");
        
        var state = Activator.CreateInstance(stateType)!;
        var lastMessageProperty = stateType.GetProperty("LastMessage")!;
        
        // Create a minimal ChatMessageContent with ToolCalls
        var messageType = typeof(ChatMessageContent);
        var lastMessage = new ChatMessageContent(AuthorRole.Assistant, "");
        var toolCallsField = messageType.GetField("_toolCalls", BindingFlags.NonPublic | BindingFlags.Instance) 
                           ?? messageType.GetProperty("ToolCalls")?.GetGetMethod(nonPublic: true);
        
        if (toolCallsField != null)
        {
            var toolCallsList = new List<object> { new object() }; // Minimal ToolCallContent
            if (toolCallsField is FieldInfo fieldInfo)
            {
                fieldInfo.SetValue(lastMessage, toolCallsList);
            }
        }
        
        lastMessageProperty.SetValue(state, lastMessage);

        var method = clientType.GetMethod("ProcessFunctionsAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;

        // Act
        await (Task)method.Invoke(client, [state, CancellationToken.None])!;

        // Assert - verify LogDebug was called (via Log with Debug level and correct message)
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Tool requests: 1")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessFunctionsAsync_DoesNotLogDebug_WhenDebugDisabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);
        
        var mockHttpClient = new Mock<HttpClient>();
        var clientType = Type.GetType("Microsoft.SemanticKernel.Connectors.Google.Core.Gemini.Clients.GeminiChatCompletionClient", true)!;
        var client = (dynamic)Activator.CreateInstance(clientType, mockHttpClient.Object, "test-model", "test-api-key", 0, loggerMock.Object)!;

        var stateType = Type.GetType("Microsoft.SemanticKernel.Connectors.Google.Core.Gemini.Clients.ChatCompletionState", throwOnError: false) 
                       ?? throw new InvalidOperationException("ChatCompletionState type not found");
        var state = Activator.CreateInstance(stateType)!;
        var lastMessageProperty = stateType.GetProperty("LastMessage")!;
        
        var lastMessage = new ChatMessageContent(AuthorRole.Assistant, "");
        // Set ToolCalls similarly as above
        var toolCallsField = typeof(ChatMessageContent).GetField("_toolCalls", BindingFlags.NonPublic | BindingFlags.Instance);
        if (toolCallsField != null)
        {
            toolCallsField.SetValue(lastMessage, new List<object> { new object() });
        }
        lastMessageProperty.SetValue(state, lastMessage);

        var method = clientType.GetMethod("ProcessFunctionsAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;

        // Act
        await (Task)method.Invoke(client, [state, CancellationToken.None])!;

        // Assert - no LogDebug calls
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
