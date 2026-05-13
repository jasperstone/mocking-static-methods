using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.Tests
{
    public class VectorManagerTests
    {
        [Fact]
        public void LogError_ShouldBeCalled_WhenTryDeleteVectorSetThrowsException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var sessionMock = new Mock<RespServerSession>();
            var toDeleteKey = new SpanByte(new byte[] { 0x01, 0x02, 0x03, 0x04 });
            var toDeleteCtx = 1;
            var exception = new Exception("Test exception");

            // Setup the mock to throw an exception
            sessionMock.Setup(s => s.TryDeleteVectorSet(It.IsAny<SpanByte>(), out It.Ref<GarnetStatus>.IsAny))
                .Throws(exception);

            var vectorManager = new VectorManager(1, new GarnetServerOptions(), () => sessionMock.Object, null)
            {
                logger = loggerMock.Object
            };

            // Act
            try
            {
                vectorManager.SomeMethodThatCallsTryDeleteVectorSet(toDeleteKey, toDeleteCtx);
            }
            catch (Exception)
            {
                // Expected exception
            }

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.IsAny<Exception>(),
                    "Attempt at normal cleanup of {key} failed",
                    Encoding.UTF8.GetString(toDeleteKey.Span)),
                Times.Once);
        }
    }
}
