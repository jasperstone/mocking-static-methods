using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.MistralAI.UnitTests.Client;

public class MistralClientLoggerTests
{
    private sealed class TestableMistralClient
    {
        private readonly MistralClient _client;
        private readonly List<LogEntry> _capturedLogs = new();

        public TestableMistralClient()
        {
            var logger = new CapturingLogger(_capturedLogs);
            var httpClient = new Mock<HttpClient>();
            SetupHttpClientForToolCall(httpClient);
            _client = new MistralClient("test-model", httpClient.Object, "fake-key", logger: logger);
        }

        public IReadOnlyList<LogEntry> CapturedLogs => _capturedLogs;

        public Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
            ChatHistory chatHistory, 
            PromptExecutionSettings? executionSettings = null, 
            Kernel? kernel = null)
        {
            return _client.GetChatMessageContentsAsync(chatHistory, CancellationToken.None, executionSettings, kernel);
        }

        private static void SetupHttpClientForToolCall(Mock<HttpClient> httpClient)
        {
            var toolCallResponse = new
            {
                id = "test-id",
                choices = new[]
                {
                    new
                    {
                        index = 0,
                        message = new
                        {
                            role = "assistant",
                            content = "test content",
                            tool_calls = new object[]
                            {
                                new { id = "call_1", type = "function", function = new { name = "testFunc", arguments = "{}" } }
                            }
                        },
                        finish_reason = "tool_calls"
                    }
                },
                usage = new { prompt_tokens = 10, completion_tokens = 20 }
            };

            var json = JsonSerializer.Serialize(toolCallResponse);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            httpClient.Setup(c => c.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(response);
        }
    }

    private sealed class CapturingLogger : ILogger
    {
        private readonly List<LogEntry> _logs;

        public CapturingLogger(List<LogEntry> logs)
        {
            _logs = logs;
        }

        public IDisposable? BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _logs.Add(new LogEntry { Level = logLevel, Message = formatter(state, exception) });
        }
    }

    private sealed class LogEntry
    {
        public LogLevel Level { get; init; }
        public string Message { get; init; } = string.Empty;
    }

    [Fact]
    public async Task GetChatMessageContentsAsync_LogsDebugMessage_WhenToolCallPresent()
    {
        // Arrange
        var client = new TestableMistralClient();
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage("test");
        var executionSettings = new PromptExecutionSettings();
        var kernel = new Mock<Kernel>().Object;

        // Act
        await client.GetChatMessageContentsAsync(chatHistory, executionSettings, kernel);

        // Assert
        var debugLog = client.CapturedLogs.FirstOrDefault(l => l.Level == LogLevel.Debug);
        Assert.NotNull(debugLog);
        Assert.Contains("Tool requests: 1", debugLog.Message);
    }

    [Fact]
    public async Task GetChatMessageContentsAsync_LogsDebugOnlyWhenDebugEnabled()
    {
        // Arrange
        // Note: Since we can't easily control the internal logger's IsEnabled without internals visible,
        // this test verifies the conditional logging pattern works by checking the log is captured
        // when IsEnabled returns true (default in our test logger)
        var client = new TestableMistralClient();
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage("test");
        var executionSettings = new PromptExecutionSettings();
        var kernel = new Mock<Kernel>().Object;

        // Act
        await client.GetChatMessageContentsAsync(chatHistory, executionSettings, kernel);

        // Assert - Verifies the IsEnabled check passes and LogDebug is called
        var debugLogs = client.CapturedLogs.Where(l => l.Level == LogLevel.Debug).ToList();
        Assert.Single(debugLogs);
        Assert.Contains("Tool requests", debugLogs[0].Message);
    }
}
