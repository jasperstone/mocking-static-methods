using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Tsavorite.core;
using Garnet.cluster;

public class LoggerExtensionsWrapper
{
    private readonly ILogger _logger;

    public LoggerExtensionsWrapper(ILogger logger)
    {
        _logger = logger;
    }

    public void IOCallback(uint errorCode, uint numBytes, object context, Func<uint, uint, object, string> getErrorMessage)
    {
        if (errorCode != 0)
        {
            var errorMessage = getErrorMessage(errorCode, numBytes, context);
            _logger?.LogError("[ClusterUtils] OverlappedStream GetQueuedCompletionStatus error: {errorCode} msg: {errorMessage}", errorCode, errorMessage);
        }

        ((SemaphoreSlim)context).Release();
    }
}

public class LoggerExtensionsWrapperTests
{
    [Fact]
    public void IOCallback_LogsError_WhenErrorCodeIsNonZero()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var wrapper = new LoggerExtensionsWrapper(loggerMock.Object);
        uint errorCode = 1;
        uint numBytes = 10;
        var context = new SemaphoreSlim(0);

        var errorMessage = "Test error message";
        Func<uint, uint, object, string> getErrorMessage = (code, bytes, ctx) => errorMessage;

        // Act
        wrapper.IOCallback(errorCode, numBytes, context, getErrorMessage);

        // Assert
        loggerMock.Verify(
            logger => logger.LogError(
                "[ClusterUtils] OverlappedStream GetQueuedCompletionStatus error: {errorCode} msg: {errorMessage}",
                errorCode,
                errorMessage),
            Times.Once);
    }

    [Fact]
    public void IOCallback_DoesNotLogError_WhenErrorCodeIsZero()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var wrapper = new LoggerExtensionsWrapper(loggerMock.Object);
        uint errorCode = 0;
        uint numBytes = 10;
        var context = new SemaphoreSlim(0);

        Func<uint, uint, object, string> getErrorMessage = (code, bytes, ctx) => "Test error message";

        // Act
        wrapper.IOCallback(errorCode, numBytes, context, getErrorMessage);

        // Assert
        loggerMock.Verify(
            logger => logger.LogError(
                It.IsAny<string>(),
                It.IsAny<object[]>()),
            Times.Never);
    }
}
