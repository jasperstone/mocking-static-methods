using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Tsavorite.core;
using Xunit;
using Moq;

namespace Tsavorite.Tests
{
    public class TsavoriteBaseTests
    {
        [Fact]
        public void AsyncPageReadCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var tsavorite = new TsavoriteBase();

            // Setup a mock logger and assign it to the private field 'logger' via reflection
            var mockLogger = new Mock<ILogger>();
            var loggerField = typeof(TsavoriteBase).GetField("logger", BindingFlags.NonPublic | BindingFlags.Instance);
            loggerField.SetValue(tsavorite, mockLogger.Object);

            // Setup a dummy recoveryCountdown with a Decrement method to avoid null ref
            var recoveryCountdownField = typeof(TsavoriteBase).GetField("recoveryCountdown", BindingFlags.NonPublic | BindingFlags.Instance);
            var countdownType = recoveryCountdownField.FieldType;
            var countdownCtor = countdownType.GetConstructor(new Type[] { typeof(int), typeof(bool) });
            var countdownInstance = countdownCtor.Invoke(new object[] { 1, false });
            recoveryCountdownField.SetValue(tsavorite, countdownInstance);

            // Act
            var method = typeof(TsavoriteBase).GetMethod("AsyncPageReadCallback", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(tsavorite, new object[] { (uint)123, (uint)0, null });

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
            var tsavorite = new TsavoriteBase();

            var mockLogger = new Mock<ILogger>();
            var loggerField = typeof(TsavoriteBase).GetField("logger", BindingFlags.NonPublic | BindingFlags.Instance);
            loggerField.SetValue(tsavorite, mockLogger.Object);

            var recoveryCountdownField = typeof(TsavoriteBase).GetField("recoveryCountdown", BindingFlags.NonPublic | BindingFlags.Instance);
            var countdownType = recoveryCountdownField.FieldType;
            var countdownCtor = countdownType.GetConstructor(new Type[] { typeof(int), typeof(bool) });
            var countdownInstance = countdownCtor.Invoke(new object[] { 1, false });
            recoveryCountdownField.SetValue(tsavorite, countdownInstance);

            var method = typeof(TsavoriteBase).GetMethod("AsyncPageReadCallback", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            method.Invoke(tsavorite, new object[] { (uint)0, (uint)0, null });

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }
    }
}
