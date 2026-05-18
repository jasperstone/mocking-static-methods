using Xunit;
using Moq;
using System.Threading;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Tsavorite.core
{
    public class IndexRecoveryTests
    {
        [Fact]
        public void AsyncPageReadCallback_LogsError_WhenErrorCodeIsNotZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var indexRecovery = new TsavoriteBase();
            var privateType = indexRecovery.GetType();
            var loggerField = privateType.GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(indexRecovery, loggerMock.Object);

            // Act
            var asyncPageReadCallbackMethod = privateType.GetMethod("AsyncPageReadCallback", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            asyncPageReadCallbackMethod.Invoke(indexRecovery, new object[] { 1u, 0u, null });

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), 1), Times.Once);
        }

        [Fact]
        public void AsyncPageReadCallback_DecrementsRecoveryCountdown_WhenErrorCodeIsNotZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var indexRecovery = new TsavoriteBase();
            var privateType = indexRecovery.GetType();
            var loggerField = privateType.GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(indexRecovery, loggerMock.Object);
            var recoveryCountdownField = privateType.GetField("recoveryCountdown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            recoveryCountdownField.SetValue(indexRecovery, new CountdownWrapper(1, false));

            // Act
            var asyncPageReadCallbackMethod = privateType.GetMethod("AsyncPageReadCallback", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            asyncPageReadCallbackMethod.Invoke(indexRecovery, new object[] { 1u, 0u, null });

            // Assert
            var isCompletedProperty = recoveryCountdownField.FieldType.GetProperty("IsCompleted");
            Assert.True((bool)isCompletedProperty.GetValue(recoveryCountdownField.GetValue(indexRecovery)));
        }

        [Fact]
        public void AsyncPageReadCallback_DecrementsRecoveryCountdown_WhenErrorCodeIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var indexRecovery = new TsavoriteBase();
            var privateType = indexRecovery.GetType();
            var loggerField = privateType.GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(indexRecovery, loggerMock.Object);
            var recoveryCountdownField = privateType.GetField("recoveryCountdown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            recoveryCountdownField.SetValue(indexRecovery, new CountdownWrapper(1, false));

            // Act
            var asyncPageReadCallbackMethod = privateType.GetMethod("AsyncPageReadCallback", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            asyncPageReadCallbackMethod.Invoke(indexRecovery, new object[] { 0u, 0u, null });

            // Assert
            var isCompletedProperty = recoveryCountdownField.FieldType.GetProperty("IsCompleted");
            Assert.True((bool)isCompletedProperty.GetValue(recoveryCountdownField.GetValue(indexRecovery)));
        }
    }
}
