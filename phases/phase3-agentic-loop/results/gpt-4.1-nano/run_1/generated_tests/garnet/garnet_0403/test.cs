using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.server;

namespace Garnet.Tests
{
    public class MultiDatabaseManagerTests
    {
        [Fact]
        public void RecoverCheckpoint_LogsInformationOnError()
        {
            // Arrange
            var storeWrapper = new DummyStoreWrapper();
            var manager = new MultiDatabaseManager(storeWrapper.CreateDatabaseDelegate, storeWrapper, createDefaultDatabase: true);
            var loggerMock = new Mock<ILogger>();
            manager.Logger = loggerMock.Object;

            // Use reflection to invoke the protected method
            var methodInfo = typeof(MultiDatabaseManager).GetMethod("RecoverCheckpoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(methodInfo);

            // Act
            // Call the method with parameters to simulate error
            methodInfo.Invoke(manager, new object[] { false, false, false, null });

            // Assert
            loggerMock.Verify(x => x.LogInformation(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Error during recovery of database ids"))), Times.AtLeastOnce);
        }
    }
}
