using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;

public class LoggerExtensionsTests
{
    [Fact]
    public void LogDebugNoCallbackForResponse_LogsMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var message = new object();

        // Act
        LoggerExtensions.LogDebugNoCallbackForResponse(loggerMock.Object, message);

        // Assert
        loggerMock.Verify(l => l.LogDebug("No callback found for response. Message: {Message}", message), Times.Once);
    }

    [Fact]
    public void LogInformationReceivedStatusUpdate_LogsRequestAndDiagnostics()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var request = new object();
        var diagnostics = new List<string> { "Diagnostic 1", "Diagnostic 2" };

        // Act
        LoggerExtensions.LogInformationReceivedStatusUpdate(loggerMock.Object, request, diagnostics);

        // Assert
        loggerMock.Verify(l => l.LogInformation("Received status update for request. Request: {Request}. Diagnostics: {Diagnostics}", request, diagnostics), Times.Once);
    }
}
