using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Tsavorite.core;
using System.Reflection;

namespace Tsavorite.Tests
{
    public class IndexRecoveryTests
    {
        [Fact]
        public void AsyncPageReadCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TsavoriteBase>>();
            loggerMock.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => {
                    var message = v?.ToString();
                    return message != null && 
                           message.Contains("AsyncPageReadCallback error: {errorCode}") && 
                           message.Contains("123");
                }),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            var tsavorite = new FakeTsavoriteWithLogger(loggerMock.Object);

            // Act
            unsafe
            {
                tsavorite.AsyncPageReadCallback(123, 0, null);
            }

            // Assert
            loggerMock.VerifyAll();
        }

        [Fact]
        public void AsyncPageReadCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TsavoriteBase>>();

            var tsavorite = new FakeTsavoriteWithLogger(loggerMock.Object);

            // Act
            unsafe
            {
                tsavorite.AsyncPageReadCallback(0, 1024, null);
            }

            // Assert
            loggerMock.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }

    internal class FakeTsavoriteWithLogger : TsavoriteBase
    {
        private readonly ILogger<TsavoriteBase> _logger;

        public FakeTsavoriteWithLogger(ILogger<TsavoriteBase> logger)
        {
            _logger = logger;
            // Use reflection to set the private logger field
            var loggerField = typeof(TsavoriteBase).GetField("logger", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            loggerField?.SetValue(this, logger);
        }

        public new unsafe void AsyncPageReadCallback(uint errorCode, uint numBytes, object overlap)
        {
            // Call the original private method via reflection
            var method = typeof(TsavoriteBase).GetMethod("AsyncPageReadCallback", 
                BindingFlags.NonPublic | BindingFlags.Instance,
                null, 
                new[] { typeof(uint), typeof(uint), typeof(object) }, 
                null);
            method?.Invoke(this, new object[] { errorCode, numBytes, overlap });
        }
    }
}
