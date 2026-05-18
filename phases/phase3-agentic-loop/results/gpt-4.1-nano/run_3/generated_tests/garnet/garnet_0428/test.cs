using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.server;

namespace Garnet.Tests
{
    public class LuaRunnerTests
    {
        [Fact]
        public void TryDecodeLargeArray_Should_LogError_When_ArrayLengthTooLong()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var stateMock = new Mock<ILuaStateWrapper>();
            var runnerMock = new Mock<LuaRunner>();
            runnerMock.Setup(r => r.state).Returns(stateMock.Object);
            runnerMock.Setup(r => r.logger).Returns(loggerMock.Object);
            runnerMock.Setup(r => r.constStrs).Returns(new LuaRunner.ConstStrings());

            var self = runnerMock.Object;

            // Setup data with length > int.MaxValue to simulate "array length is too long"
            var dataSpan = new ReadOnlySpan<byte>(new byte[4] { 0xFF, 0xFF, 0xFF, 0xFF });
            // Act
            var result = LuaRunner.TryDecodeLargeArray(self, ref dataSpan, out int errorId);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                log => log.LogError("Array length is too long: {len}", It.IsAny<ulong>()),
                Times.Once);
        }
    }
}
