using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.common;
using Garnet.networking;
using Garnet.server;
using System.Text;
using Tsavorite.core;

namespace Garnet.server.tests
{
    public class VectorManagerTests
    {
        [Fact]
        public void ResumePostRecovery_LogsError_WhenTryDeleteVectorSetThrows()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<VectorManager>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);
            
            var cleanupSessionMock = new Mock<IMessageConsumer>();
            var getCleanupSession = new Mock<Func<IMessageConsumer>>();
            getCleanupSession.Setup(f => f()).Returns(cleanupSessionMock.Object);

            var serverOptions = new GarnetServerOptions { EnableVectorSetPreview = true };
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var vectorManager = new VectorManager(0, serverOptions, getCleanupSession.Object, loggerFactoryMock.Object);

            // Mock the complex internal state to reach the error path
            // GetDeletesInProgress returns non-empty, TryDeleteVectorSet throws
            var storageSessionMock = new Mock<object>();
            var sessionMock = Mock.Of<RespServerSession>(s => 
                s.storageSession == Mock.Of<StorageSession>(st => 
                    st.basicContext == Mock.Of<ICoreContext<SpanByte, SpanByte, EmptyDefaultInput, SpanByte, SpanByte, EmptyAllocator, Allocator<SpanByte>>>(ctx => true)
                ) == true
            );
            
            cleanupSessionMock.Setup(s => s as RespServerSession).Returns(sessionMock);

            // Act
            Assert.ThrowsAny<Exception>(() => vectorManager.ResumePostRecovery());

            // Assert - Verify the specific LogError extension call on line 221
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "Attempt at normal cleanup of {key} failed",
                    It.IsAny<string>()),
                Times.AtLeastOnce);
        }
    }
}
