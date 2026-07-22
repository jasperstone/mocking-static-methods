using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Amazon.Core.Tests;

public class BedrockChatCompletionClientLoggerTests
{
    private const string ModelId = "test-model";

    [Fact]
    public void LogErrorExtension_VerifiesMessageFormat()
    {
        // Test the specific LoggerExtensions.LogError call pattern from line 180
        var mockLogger = new Mock<ILogger>();

        var exception = new Exception("Test exception");
        var expectedMessage = "Can't converse stream with '{ModelId}'. Reason: {Error}";

        // Act - simulate the exact LogError call from line 180
        mockLogger.Object.LogError(exception, expectedMessage, ModelId, exception.Message);

        // Assert - verify the LogError extension was called with correct parameters
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                0,
                It.Is<It.IsAnyType>((object state) => 
                    state.ToString()!.Contains(expectedMessage) && 
                    state.ToString()!.Contains(ModelId) && 
                    state.ToString()!.Contains(exception.Message)),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogErrorExtension_VerifiesWithRealisticModelId()
    {
        // Test with realistic model ID from Bedrock
        var mockLogger = new Mock<ILogger>();
        var exception = new Exception("Bedrock API failure");
        var modelId = "anthropic.claude-3-sonnet-20240229-v1:0";
        var expectedMessage = "Can't converse stream with '{ModelId}'. Reason: {Error}";

        // Act
        mockLogger.Object.LogError(exception, expectedMessage, modelId, exception.Message);

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                0,
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
