using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.server;

namespace Garnet.Tests
{
    public class SingleDatabaseManagerTests
    {
        private class DummyDatabase : GarnetDatabase
        {
            public override void Initialize() { }
            public override long Recover() => 42;
            public override long Recover(object token1, object token2) => 42;
        }

        private class DummyStoreWrapper
        {
            public class DummyLoggerFactory
            {
                public ILogger CreateLogger(string name) => new Mock<ILogger>().Object;
            }

            public DummyLoggerFactory loggerFactory = new DummyLoggerFactory();
            public DummyServerOptions serverOptions = new DummyServerOptions();
        }

        private class DummyServerOptions
        {
            public bool FailOnRecoveryError { get; set; } = false;
        }

        [Fact]
        public void LogInformation_IsCalled_OnLine226()
        {
            // Arrange
            var mockStoreWrapper = new DummyStoreWrapper();
            var createDb = new Func<int, GarnetDatabase>(id => new DummyDatabase());
            var manager = new SingleDatabaseManager(createDb, mockStoreWrapper, createDefaultDatabase: true);

            var loggerMock = new Mock<ILogger>();
            // Inject the mock logger into the manager
            var managerType = typeof(SingleDatabaseManager);
            var loggerField = managerType.GetField("Logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(manager, loggerMock.Object);

            // Force the condition to log information
            var aofSizeLimit = 0L;
            var aofSize = 10L;
            var logger = loggerMock.Object;

            // Act
            var task = manager.TaskCheckpointBasedOnAofSizeLimitAsync(aofSizeLimit, logger: logger);
            task.GetAwaiter().GetResult();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Enforcing AOF size limit")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
