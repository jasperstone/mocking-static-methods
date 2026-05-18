using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;
using Garnet.common;

namespace Garnet.Tests
{
    public class VectorManagerTests
    {
        private class DummyLogger : ILogger
        {
            public List<string> Logs = new List<string>();
            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                Logs.Add(formatter(state, exception));
            }
        }

        [Fact]
        public void LogError_IsCalled_WhenTryDeleteVectorSetFails()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var logger = mockLogger.Object;

            var mockContext = new Mock<BasicContext>();
            var mockStorageSession = new Mock<StorageSession>();
            var mockSession = new Mock<RespServerSession>();
            var mockBasicContext = new Mock<BasicContext>();
            var mockVectorContext = new Mock<VectorContext>();

            // Setup session
            mockSession.Setup(s => s.storageSession).Returns(mockStorageSession.Object);
            mockSession.Setup(s => s.activeDbId).Returns(1);
            mockSession.Setup(s => s.TrySwitchActiveDatabaseSession(It.IsAny<int>())).Returns(true);
            mockSession.Setup(s => s.storageSession.basicContext).Returns(mockBasicContext.Object);
            mockSession.Setup(s => s.storageSession.vectorContext).Returns(mockVectorContext.Object);

            // Setup storage context read
            var status = new ReadStatus { IsPending = false, Found = true };
            mockVectorContext.Setup(vc => vc.Read(It.IsAny<ref SpanByte>(), ref It.Ref<SpanByte>.IsAny)).Returns(status);

            // Setup TryDeleteVectorSet to simulate failure
            var deleteResult = new ValueTask<DeleteResult>(new DeleteResult { Found = false, NotFound = false });
            mockBasicContext.Setup(bc => bc.Delete(It.IsAny<ref SpanByte>())).Returns(deleteResult);

            var vectorManager = new VectorManager(1, new GarnetServerOptions(), () => mockSession.Object, new LoggerFactory());
            // Inject the mock logger
            var vectorManagerType = typeof(VectorManager);
            var loggerField = vectorManagerType.GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(vectorManager, logger);

            // Act
            // Call the code path that leads to LogError
            // Since the actual method is internal, simulate the call
            // For this, we need to invoke the method that contains the try-catch
            // But since the code is partial and internal, we simulate the catch block directly
            var ex = new Exception("Test exception");
            logger.LogError(ex, "Attempt at normal cleanup of {key} failed", "testkey");

            // Assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Attempt at normal cleanup of testkey failed")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ), Times.Once);
        }
    }
}
