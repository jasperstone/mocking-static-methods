using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.server;
using System;
using System.Text;
using System.Buffers;
using System.Runtime.InteropServices;

namespace Garnet.Tests
{
    public class LuaRunnerTests
    {
        [Fact]
        public void TryDecodeLargeArray_Should_LogError_When_ArrayLengthIsTooLong()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var stateMock = new Mock<ILuaState>();
            var runnerMock = new Mock<LuaRunner>();
            runnerMock.Setup(r => r.state).Returns(stateMock.Object);
            runnerMock.Setup(r => r.logger).Returns(loggerMock.Object);
            var self = runnerMock.Object;

            // Setup data span with length > int.MaxValue (simulate large array length)
            var dataBytes = new byte[8];
            var dataSpan = new ReadOnlySpan<byte>(dataBytes);
            BinaryPrimitives.WriteUInt32BigEndian(dataBytes.AsSpan(0,4), uint.MaxValue); // length = 4294967295

            // Act
            var result = LuaRunner.TryDecodeLargeArray(self, ref dataSpan, out int errorId);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                log => log.LogError("Array length is too long: {len}", uint.MaxValue),
                Times.Once);
        }
    }
}
