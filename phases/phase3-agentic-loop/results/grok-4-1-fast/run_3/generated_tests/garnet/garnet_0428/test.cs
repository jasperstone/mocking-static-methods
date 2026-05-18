using System;
using System.Buffers.Binary;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server
{
    public class LuaRunnerFunctionsTests
    {
        [Fact]
        public void TryDecodeLargeArray_LogsError_WhenArrayLengthIsNegative()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var luaRunner = new Mock<LuaRunner>();
            luaRunner.Setup(r => r.logger).Returns(mockLogger.Object);

            var data = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(data, uint.MaxValue);

            // Act
            var result = LuaRunner.Functions.TryDecodeLargeArray(luaRunner.Object, ref data, out var constStrErrId);

            // Assert
            Assert.False(result);
            Assert.Equal((int)LuaRunner.Functions.LuaConstStr.MsgPackArrayTooLong, constStrErrId);
            
            mockLogger.Verify(
                x => x.LogError("Array length is too long: {len}", uint.MaxValue),
                Times.Once
            );
        }

        [Fact]
        public void TryDecodeLargeArray_DoesNotLog_WhenLoggerIsNull()
        {
            // Arrange
            var luaRunner = new Mock<LuaRunner>();
            luaRunner.Setup(r => r.logger).Returns((ILogger)null);

            var data = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(data, uint.MaxValue);

            // Act
            var result = LuaRunner.Functions.TryDecodeLargeArray(luaRunner.Object, ref data, out var constStrErrId);

            // Assert
            Assert.False(result);
            Assert.Equal((int)LuaRunner.Functions.LuaConstStr.MsgPackArrayTooLong, constStrErrId);
        }
    }
}
