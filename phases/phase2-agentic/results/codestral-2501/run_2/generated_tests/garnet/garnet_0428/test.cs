using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;
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

            var data = new ReadOnlySpan<byte>(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }); // Length that is too long

            // Act
            var result = LuaRunnerFunctions.TryDecodeLargeArray(luaRunner, ref data, out var constStrErrId);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Array length is too long")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            Assert.False(result);
            Assert.Equal(luaRunner.constStrs.MsgPackArrayTooLong, constStrErrId);
        }
    }
}
