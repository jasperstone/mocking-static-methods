using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Tsavorite.core;
using Xunit;

namespace Garnet.Tests.Storage.Tsavorite.Index.Recovery
{
    public class IndexRecoveryLoggerExtensionsTests
    {
        private sealed class TsavoriteBaseAccessor : TsavoriteBase
        {
            public TsavoriteBaseAccessor()
            {
                // Ensure recoveryCountdown is initialized to avoid NullReferenceException in AsyncPageReadCallback.
                var countdownWrapperType = typeof(TsavoriteBase)
                    .GetNestedType("CountdownWrapper", BindingFlags.NonPublic);
                var countdown = Activator.CreateInstance(countdownWrapperType!, new object?[] { 1, false });
                typeof(TsavoriteBase)
                    .GetField("recoveryCountdown", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .SetValue(this, countdown);
            }

            public void SetLogger(ILogger logger)
            {
                typeof(TsavoriteBase)
                    .GetField("logger", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .SetValue(this, logger);
            }

            public void InvokeAsyncPageReadCallback(uint errorCode)
            {
                var method = typeof(TsavoriteBase)
                    .GetMethod("AsyncPageReadCallback", BindingFlags.Instance | BindingFlags.NonPublic)!;
                method.Invoke(this, new object?[] { errorCode, 0u, null! });
            }
        }

        [Fact]
        public void AsyncPageReadCallback_WhenErrorCodeNotZero_LogsError()
        {
            var mockLogger = new Mock<ILogger>();
            mockLogger
                .Setup(logger => logger.IsEnabled(LogLevel.Error))
                .Returns(true);

            var accessor = new TsavoriteBaseAccessor();
            accessor.SetLogger(mockLogger.Object);

            accessor.InvokeAsyncPageReadCallback(123u);

            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, _) =>
                        state.ToString() == "AsyncPageReadCallback error: {errorCode}"),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void AsyncPageReadCallback_WhenErrorCodeZero_DoesNotLogError()
        {
            var mockLogger = new Mock<ILogger>();
            mockLogger
                .Setup(logger => logger.IsEnabled(LogLevel.Error))
                .Returns(true);

            var accessor = new TsavoriteBaseAccessor();
            accessor.SetLogger(mockLogger.Object);

            accessor.InvokeAsyncPageReadCallback(0u);

            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}
