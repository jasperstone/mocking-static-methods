using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Garnet.server
{
    public class LuaRunnerTests
    {
        [Fact]
        public void TryDecodeLargeArray_LogsError_WhenArrayLengthIsTooLong()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var luaRunner = new LuaRunner(
                LuaMemoryManagementMode.Default,
                null,
                LuaLoggingMode.Default,
                new HashSet<string>(),
                new byte[0],
                false,
                null,
                null,
                "0.0.0.0",
                loggerMock.Object
            );

            var data = new byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(data, uint.MaxValue + 1);

            // Act
            var result = LuaRunner.TryDecodeLargeArray(luaRunner, ref data, out _);

            // Assert
            loggerMock.Verify(l => l.LogError("Array length is too long: {len}", uint.MaxValue + 1), Times.Once);
            Assert.False(result);
        }
    }
}
