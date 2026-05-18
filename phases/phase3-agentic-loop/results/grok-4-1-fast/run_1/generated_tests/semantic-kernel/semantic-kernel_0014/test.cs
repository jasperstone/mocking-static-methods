using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Amazon.UnitTests.Core.Clients;

public class LoggerExtensionsTests
{
    private const string TestModelId = "test-model-id";

    [Fact]
    public void LoggerExtensions_LogError_CalledWithCorrectParameters()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var exception = new Exception("Test exception");
        
        // Act
        mockLogger.Object.LogError(exception, "Can't converse stream with '{ModelId}'. Reason: {Error}", TestModelId, exception.Message);

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(TestModelId) && v.ToString()!.Contains("Can't converse stream")),
                It.Is<Exception>(ex => ex == exception),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LoggerExtensions_LogError_GenerateChat_CalledWithCorrectParameters()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var exception = new Exception("Test exception");
        
        // Act
        mockLogger.Object.LogError(exception, "Can't converse with '{ModelId}'. Reason: {Error}", TestModelId, exception.Message);

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(TestModelId) && v.ToString()!.Contains("Can't converse")),
                It.Is<Exception>(ex => ex == exception),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
