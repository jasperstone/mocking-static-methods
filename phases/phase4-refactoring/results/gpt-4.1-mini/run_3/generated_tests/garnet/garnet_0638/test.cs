using System;
using Microsoft.Extensions.Logging;
using Tsavorite.core;
using Xunit;
using Moq;
using System.Reflection;

namespace Tsavorite.Tests
{
    public class TsavoriteBaseTests
    {
        private class TestTsavoriteBase : TsavoriteBase
        {
            public ILogger Logger
            {
                set
                {
                    var loggerField = typeof(TsavoriteBase).GetField("logger", BindingFlags.NonPublic | BindingFlags.Instance);
                    loggerField.SetValue(this, value);
                }
            }

            public void CallAsyncPageReadCallback(uint errorCode, uint numBytes, object overlap)
            {
                // Call the private method via reflection
                var method = typeof(TsavoriteBase).GetMethod("AsyncPageReadCallback", BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(this, new object[] { errorCode, numBytes, overlap });
            }
        }

        [Fact]
        public void AsyncPageReadCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var tsavorite = new TestTsavoriteBase();
            tsavorite.Logger = mockLogger.Object;

            uint errorCode = 123;
            uint numBytes = 0;
            object overlap = null;

            // Act
            tsavorite.CallAsyncPageReadCallback(errorCode, numBytes, overlap);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AsyncPageReadCallback error:")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void AsyncPageReadCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var tsavorite = new TestTsavoriteBase();
            tsavorite.Logger = mockLogger.Object;

            uint errorCode = 0;
            uint numBytes = 0;
            object overlap = null;

            // Act
            tsavorite.CallAsyncPageReadCallback(errorCode, numBytes, overlap);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }
    }
}
