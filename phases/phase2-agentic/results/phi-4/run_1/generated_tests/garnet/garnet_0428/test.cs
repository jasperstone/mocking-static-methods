using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.Tests
{
    public class LuaRunnerFunctionsTests
    {
        [Fact]
        public void TryDecodeLargeArray_LogsError_WhenArrayLengthIsTooLong()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var luaRunner = new LuaRunner(
                memMode: LuaMemoryManagementMode.Default,
                memLimitBytes: null,
                logMode: LuaLoggingMode.Default,
                allowedFunctions: new HashSet<string>(),
                source: new byte[0],
                logger: loggerMock.Object
            );

            var data = new byte[4 + sizeof(uint)];
            BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan(4), uint.MaxValue + 1);

            // Act
            bool result = LuaRunnerFunctions.TryDecodeLargeArray(luaRunner, ref data.AsReadOnlySpan(), out int constStrErrId);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                l => l.LogError(It.IsAny<string>(), It.Is<object[]>(objects => objects[0] == uint.MaxValue + 1)),
                Times.Once
            );
        }
    }
}
