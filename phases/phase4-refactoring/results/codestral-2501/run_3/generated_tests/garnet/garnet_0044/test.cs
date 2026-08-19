using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Tsavorite.core;
using Garnet.cluster;

public class UtilityWrapper
{
    private readonly Func<uint, uint, object, string> _getCallbackErrorMessage;

    public UtilityWrapper(Func<uint, uint, object, string> getCallbackErrorMessage)
    {
        _getCallbackErrorMessage = getCallbackErrorMessage;
    }

    public string GetCallbackErrorMessage(uint errorCode, uint numBytes, object context)
    {
        return _getCallbackErrorMessage(errorCode, numBytes, context);
    }
}

public static class LoggerExtensions
{
    public static void IOCallback(this ILogger logger, uint errorCode, uint numBytes, object context, UtilityWrapper utilityWrapper)
    {
        if (errorCode != 0)
        {
            var errorMessage = utilityWrapper.GetCallbackErrorMessage(errorCode, numBytes, context);
            logger?.LogError("[ClusterUtils] OverlappedStream GetQueuedCompletionStatus error: {errorCode} msg: {errorMessage}", errorCode, errorMessage);
        }

        ((SemaphoreSlim)context).Release();
    }
}

public class LoggerExtensionsTests
{
    [Fact]
    public void IOCallback_LogsError_WhenErrorCodeIsNonZero()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        uint errorCode = 1;
        uint numBytes = 10;
        var context = new SemaphoreSlim(0);

        var utilityWrapper = new UtilityWrapper((code, bytes, ctx) => "Test error message");

        // Act
        LoggerExtensions.IOCallback(loggerMock.Object, errorCode, numBytes, context, utilityWrapper);

        // Assert
        loggerMock.Verify(
            logger => logger.LogError(
                "[ClusterUtils] OverlappedStream GetQueuedCompletionStatus error: {errorCode} msg: {errorMessage}",
                errorCode,
                "Test error message"),
            Times.Once);
    }

    [Fact]
    public void IOCallback_DoesNotLogError_WhenErrorCodeIsZero()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        uint errorCode = 0;
        uint numBytes = 10;
        var context = new SemaphoreSlim(0);

        var utilityWrapper = new UtilityWrapper((code, bytes, ctx) => "Test error message");

        // Act
        LoggerExtensions.IOCallback(loggerMock.Object, errorCode, numBytes, context, utilityWrapper);

        // Assert
        loggerMock.Verify(
            logger => logger.LogError(
                It.IsAny<string>(),
                It.IsAny<object[]>()),
            Times.Never);
    }
}
