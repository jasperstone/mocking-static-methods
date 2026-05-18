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
        private class DummyCountdown
        {
            public void Decrement() { }
        }

        [Fact]
        public void AsyncPageReadCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var tsavorite = (TsavoriteBase)Activator.CreateInstance(typeof(TsavoriteBase), nonPublic: true)!;

            var mockLogger = new Mock<ILogger>();
            tsavorite.GetType().GetField("logger", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(tsavorite, mockLogger.Object);

            tsavorite.GetType().GetField("recoveryCountdown", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(tsavorite, new DummyCountdown());

            // Act
            var method = tsavorite.GetType().GetMethod("AsyncPageReadCallback", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(tsavorite, new object[] { (uint)123, (uint)0, null });

            // Assert
            mockLogger.Verify(l => l.LogError("AsyncPageReadCallback error: {errorCode}", 123), Times.Once());
        }

        [Fact]
        public void AsyncPageReadCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var tsavorite = (TsavoriteBase)Activator.CreateInstance(typeof(TsavoriteBase), nonPublic: true)!;

            var mockLogger = new Mock<ILogger>();
            tsavorite.GetType().GetField("logger", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(tsavorite, mockLogger.Object);

            tsavorite.GetType().GetField("recoveryCountdown", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(tsavorite, new DummyCountdown());

            // Act
            var method = tsavorite.GetType().GetMethod("AsyncPageReadCallback", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(tsavorite, new object[] { (uint)0, (uint)0, null });

            // Assert
            mockLogger.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never());
        }
    }
}
