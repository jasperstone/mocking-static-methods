using System;
using System.Collections.Generic;
using System.Reflection;
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
        public async Task ProcessFunctionsAsync_LogsDebugMessage_WhenDebugEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

            var httpClient = new System.Net.Http.HttpClient();

            // Use reflection to create instance of internal sealed class
            var clientType = typeof(GeminiChatCompletionClient);
            var ctor = clientType.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new Type[] { typeof(System.Net.Http.HttpClient), typeof(string), typeof(string), typeof(GoogleAIVersion), typeof(ILogger) },
                null);
            Assert.NotNull(ctor);

            var client = (GeminiChatCompletionClient)ctor.Invoke(new object[] { httpClient, "modelId", "apiKey", GoogleAIVersion.V1, loggerMock.Object });

            var state = new ChatCompletionState
            {
                LastMessage = new GeminiChatMessage
                {
                    ToolCalls = new List<GeminiToolCall>
                    {
                        new GeminiToolCall(),
                        new GeminiToolCall()
                    }
                }
            };

            // Use reflection to invoke private method ProcessFunctionsAsync
            var method = clientType.GetMethod("ProcessFunctionsAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method);

            // Act
            var task = (Task)method.Invoke(client, new object[] { state, CancellationToken.None })!;
            await task;

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Tool requests: 2")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }

    // Minimal stubs for required types to compile the test
    internal class ChatCompletionState
    {
        public GeminiChatMessage? LastMessage { get; set; }
        public bool AutoInvoke { get; set; }
        public bool FilterTerminationRequested { get; set; }
    }

    internal class GeminiChatMessage
    {
        public List<GeminiToolCall>? ToolCalls { get; set; }
    }

    internal class GeminiToolCall
    {
        public override string ToString() => "ToolCall";
    }

    internal enum GoogleAIVersion
    {
        V1
    }
}
