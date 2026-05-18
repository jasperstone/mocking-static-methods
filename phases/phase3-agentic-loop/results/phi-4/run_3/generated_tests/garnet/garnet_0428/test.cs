using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.Tests
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
                ReadOnlyMemory<byte>.Empty,
                logger: loggerMock.Object
            );

            var data = new byte[5];
            BinaryPrimitives.WriteUInt32BigEndian(data, unchecked((uint)-1)); // Set length to -1 to trigger error

            // Act
            int constStrErrId;
            var result = luaRunner.TryDecodeLargeArray(ref data.AsSpan(), out constStrErrId);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Array length is too long: -1")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }
    }
}
