using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Garnet.common;
using KeraLua;

namespace Garnet.Tests
{
    public class LuaRunnerFunctionsTests
    {
        [Fact]
        public void TryDecodeLargeArray_ArrayLengthTooLong_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var luaRunner = new LuaRunner(
                LuaMemoryManagementMode.Default,
                null,
                LuaLoggingMode.Default,
                new HashSet<string>(),
                new ReadOnlyMemory<byte>(),
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
                x => x.Log(
                    It.IsAny<LogLevel>(),
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
