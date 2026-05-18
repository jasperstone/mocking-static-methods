using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Tsavorite.core;
using System.Reflection;

namespace TsavoriteTests
{
    public class IndexRecoveryTests
    {
        [Fact]
        public void AsyncPageReadCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var tsavoriteBase = new TsavoriteBase();

            // Use reflection to set the private logger field
            var loggerField = typeof(TsavoriteBase).GetField("logger", BindingFlags.NonPublic | BindingFlags.Instance);
            loggerField.SetValue(tsavoriteBase, mockLogger.Object);

            uint errorCode = 1; // Non-zero error code
            uint numBytes = 0;
            object overlap = null;

            // Use reflection to invoke the private AsyncPageReadCallback method
            var asyncPageReadCallbackMethod = typeof(TsavoriteBase).GetMethod("AsyncPageReadCallback", BindingFlags.NonPublic | BindingFlags.Instance);
            asyncPageReadCallbackMethod.Invoke(tsavoriteBase, new object[] { errorCode, numBytes, overlap });

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    It.Is<string>(s => s.Contains("AsyncPageReadCallback error:")),
                    errorCode),
                Times.Once);
        }
    }
}
