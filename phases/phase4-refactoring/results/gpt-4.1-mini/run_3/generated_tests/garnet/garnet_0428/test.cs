using System;
using System.Buffers.Binary;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server;

namespace Garnet.Tests
{
    public class LuaRunnerFunctionsTests
    {
        [Fact]
        public void TryDecodeLargeArray_LogsError_WhenArrayLengthTooLong()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var luaRunner = CreateLuaRunnerWithLogger(loggerMock.Object);

            // Prepare data with length > int.MaxValue (simulate overflow)
            // We use 0xFFFFFFFF (uint.MaxValue) which is -1 if cast to int
            var lengthBytes = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
            var data = new ReadOnlySpan<byte>(lengthBytes);

            // Act
            int constStrErrId;
            var result = InvokeTryDecodeLargeArray(luaRunner, ref data, out constStrErrId);

            // Assert
            Assert.False(result);
            Assert.Equal(luaRunner.constStrs.MsgPackArrayTooLong, constStrErrId);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Array length is too long")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private static LuaRunner CreateLuaRunnerWithLogger(ILogger logger)
        {
            // We create a LuaRunner with minimal required parameters and inject the logger.
            // Some parameters can be null or default as they are not used in this test.
            return (LuaRunner)Activator.CreateInstance(
                typeof(LuaRunner),
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                binder: null,
                new object[]
                {
                    LuaMemoryManagementMode.Default,
                    null,
                    LuaLoggingMode.None,
                    new System.Collections.Generic.HashSet<string>(),
                    ReadOnlyMemory<byte>.Empty,
                    false,
                    null,
                    null,
                    "0.0.0.0",
                    logger
                },
                culture: null);
        }

        private static bool InvokeTryDecodeLargeArray(LuaRunner runner, ref ReadOnlySpan<byte> data, out int constStrErrId)
        {
            // The TryDecodeLargeArray method is private static inside LuaRunner partial class.
            // We use reflection to invoke it for testing.
            var method = typeof(LuaRunner).GetMethod("TryDecodeLargeArray", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) throw new InvalidOperationException("TryDecodeLargeArray method not found");

            object[] parameters = { runner, data, null };
            var result = (bool)method.Invoke(null, parameters);
            constStrErrId = (int)parameters[2];
            return result;
        }
    }
}
