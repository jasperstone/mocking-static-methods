using System;
using System.Buffers.Binary;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.Tests
{
    public class LuaRunnerFunctionsTests
    {
        // We want to test the internal static method TryDecodeLargeArray on LuaRunner
        // specifically the branch where it logs an error when array length is too long.
        // The method signature is:
        // static bool TryDecodeLargeArray(LuaRunner self, ref ReadOnlySpan<byte> data, out int constStrErrId)
        //
        // We will create a minimal subclass of LuaRunner to set logger and constStrs.
        // We will pass a data span with a length > int.MaxValue to trigger the error.

        private class TestLuaRunner : LuaRunner
        {
            public new ILogger logger;
            public new ConstStrs constStrs;
            public new LuaStateWrapper state;

            public TestLuaRunner(ILogger logger, ConstStrs constStrs, LuaStateWrapper state)
                : base(LuaMemoryManagementMode.Default, null, LuaLoggingMode.None, null, ReadOnlyMemory<byte>.Empty)
            {
                this.logger = logger;
                this.constStrs = constStrs;
                this.state = state;
            }
        }

        private class ConstStrs
        {
            public int MsgPackArrayTooLong = 1234;
        }

        private class LuaStateWrapper
        {
            public bool TryEnsureMinimumStackCapacity(int needed) => true;
            public bool TryCreateTable(int arrayLen, int mapLen) => true;
            public int StackTop => 1;
            public void RawSetInteger(int len, int arrayIndex, int i) { }
        }

        [Fact]
        public void TryDecodeLargeArray_LogsError_WhenArrayLengthTooLong()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var constStrs = new ConstStrs();
            var state = new LuaStateWrapper();

            var runner = new TestLuaRunner(loggerMock.Object, constStrs, state);

            // Create a data span with a 4-byte length prefix > int.MaxValue
            // For example, length = 0x80000000 (2147483648)
            byte[] dataBytes = new byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(dataBytes, 0x80000000);
            ReadOnlySpan<byte> dataSpan = dataBytes;

            int constStrErrId;

            // Act
            var result = InvokeTryDecodeLargeArray(runner, ref dataSpan, out constStrErrId);

            // Assert
            Assert.False(result);
            Assert.Equal(constStrs.MsgPackArrayTooLong, constStrErrId);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Array length is too long")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Helper to invoke the private static method TryDecodeLargeArray via reflection
        private static bool InvokeTryDecodeLargeArray(LuaRunner runner, ref ReadOnlySpan<byte> data, out int constStrErrId)
        {
            var method = typeof(LuaRunner).GetMethod("TryDecodeLargeArray", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            object[] parameters = new object[] { runner, data, null };
            var result = (bool)method.Invoke(null, parameters);
            data = (ReadOnlySpan<byte>)parameters[1];
            constStrErrId = (int)parameters[2];
            return result;
        }
    }
}
