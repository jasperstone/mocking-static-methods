using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Tsavorite.core;

namespace Tsavorite.Tests
{
    public class IndexRecoveryTests
    {
        private readonly Mock<ILogger<TsavoriteBase>> _loggerMock;
        private readonly TsavoriteBase _tsavorite;
        private FieldInfo _recoveryCountdownField;
        private MethodInfo _asyncPageReadCallbackMethod;

        public IndexRecoveryTests()
        {
            _loggerMock = new Mock<ILogger<TsavoriteBase>>();
            _loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

            _tsavorite = new Mock<TsavoriteBase> { CallBase = true }.Object;
            
            _recoveryCountdownField = typeof(TsavoriteBase).GetField("recoveryCountdown", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            _asyncPageReadCallbackMethod = typeof(TsavoriteBase).GetMethod("AsyncPageReadCallback", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;

            // Set up logger field using reflection since it's private/internal
            var loggerField = typeof(TsavoriteBase).GetField("logger", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            loggerField.SetValue(_tsavorite, _loggerMock.Object);
        }

        [Fact]
        public void AsyncPageReadCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var mockCountdown = new Mock<object>().Object; 
            _recoveryCountdownField.SetValue(_tsavorite, mockCountdown);

            uint errorCode = 123u;

            // Act
            _asyncPageReadCallbackMethod.Invoke(_tsavorite, new object[] { errorCode, 1024u, null });

            // Assert - verify Log was called with Error level and correct message
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        ((string)v!).Contains("AsyncPageReadCallback error:") && 
                        ((string)v!).Contains("123")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
                Times.Once
            );
        }

        [Fact]
        public void AsyncPageReadCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var mockCountdown = new Mock<object>().Object;
            _recoveryCountdownField.SetValue(_tsavorite, mockCountdown);

            // Act
            _asyncPageReadCallbackMethod.Invoke(_tsavorite, new object[] { 0u, 1024u, null });

            // Assert
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
                Times.Never
            );
        }
    }
}
