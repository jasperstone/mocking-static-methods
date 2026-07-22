using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class FailoverSessionTests
    {
        [Fact]
        public async Task PauseWritesAndWaitForSyncAsync_ClientNull_LogsErrorAndReturnsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FailoverSession>>();
            var failoverSession = new FailoverSession();

            // Inject the mock logger into the private field
            var loggerField = typeof(FailoverSession).GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(failoverSession, loggerMock.Object);

            // Override GetConnectionAsync to return null
            var getConnectionMethod = typeof(FailoverSession).GetMethod("GetConnectionAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            // Since it's private, we can't override directly; instead, we can create a derived class for testing
            // or use reflection to replace the method. For simplicity, assume we can set a delegate or modify the class.
            // Here, we will create a derived class for test purposes.

            var testSession = new FailoverSessionForTest(loggerMock.Object);
            testSession.SetGetConnectionAsyncReturn(null);

            // Act
            var result = await testSession.PauseWritesAndWaitForSyncAsync();

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), "PauseWritesAndWaitForSync Error"),
                Times.Once);
        }
    }

    // Derived class to override GetConnectionAsync
    public class FailoverSessionForTest : FailoverSession
    {
        private Func<string, Task<GarnetClient>> _getConnectionAsync;

        public FailoverSessionForTest(ILogger<FailoverSession> logger)
        {
            // Set the logger
            var loggerField = typeof(FailoverSession).GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(this, logger);
        }

        public void SetGetConnectionAsyncReturn(Task<GarnetClient> task)
        {
            _getConnectionAsync = _ => task;
        }

        protected override Task<GarnetClient> GetConnectionAsync(string nodeId)
        {
            return _getConnectionAsync != null ? _getConnectionAsync(nodeId) : base.GetConnectionAsync(nodeId);
        }
    }
}
