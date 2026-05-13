using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;
using System.Buffers;
using System;

namespace Garnet.Tests
{
    public class LuaRunnerFunctionsTests
    {
        [Fact]
        public void TryDecodeLargeArray_ArrayLengthTooLong_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<LuaRunner>>();
            var luaRunner = new LuaRunner(
                LuaMemoryManagementMode.Default,
                null,
                LuaLoggingMode.Default,
                null,
                ReadOnlyMemory<byte>.Empty,
                false,
                null,
                null,
                "0.0.0.0",
                mockLogger.Object
            );

            var data = new byte[5] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }; // Array length that is too long
            var constStrErrId = -1;

            // Act
            var result = LuaRunnerFunctions.TryDecodeLargeArray(luaRunner, ref data, out constStrErrId);

            // Assert
            mockLogger.Verify(
                logger => logger.LogError("Array length is too long: {len}", It.IsAny<uint>()),
                Times.Once
            );
            Assert.False(result);
            Assert.Equal(luaRunner.constStrs.MsgPackArrayTooLong, constStrErrId);
        }
    }
}
