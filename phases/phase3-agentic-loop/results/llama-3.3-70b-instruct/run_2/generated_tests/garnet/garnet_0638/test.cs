using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Tsavorite.core
{
    public class IndexRecoveryTests
    {
        [Fact]
        public void TestAsyncPageReadCallback_LogsError_WhenErrorCodeIsNotZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var indexRecovery = new TsavoriteBase();
            var privateField = indexRecovery.GetType().GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            privateField.SetValue(indexRecovery, loggerMock.Object);

            // Act
            var methodInfo = indexRecovery.GetType().GetMethod("AsyncPageReadCallback", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            methodInfo.Invoke(indexRecovery, new object[] { 1, 0, null });

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void TestAsyncPageReadCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var indexRecovery = new TsavoriteBase();
            var privateField = indexRecovery.GetType().GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            privateField.SetValue(indexRecovery, loggerMock.Object);

            // Act
            var methodInfo = indexRecovery.GetType().GetMethod("AsyncPageReadCallback", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            methodInfo.Invoke(indexRecovery, new object[] { 0, 0, null });

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }
    }
}
