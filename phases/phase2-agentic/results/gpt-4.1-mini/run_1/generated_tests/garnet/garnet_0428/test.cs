using System;
using System.Buffers.Binary;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.Tests
{
    public class LuaRunnerFunctionsTests
    {
        private class TestLuaRunner
        {
            public ILogger logger;
            public TestState state = new TestState();
            public TestConstStrs constStrs = new TestConstStrs();

            public bool TryEnsureMinimumStackCapacity(int needed)
            {
                return state.TryEnsureMinimumStackCapacity(needed);
            }

            public bool TryCreateTable(int arrayLen, int mapLen)
            {
                return state.TryCreateTable(arrayLen, mapLen);
            }

            public int StackTop => state.StackTop;

            public void RawSetInteger(int len, int arrayIndex, int i)
            {
                state.RawSetInteger(len, arrayIndex, i);
            }
        }

        private class TestState
        {
            public bool EnsureStackSuccess = true;
            public bool CreateTableSuccess = true;
            public int StackTop = 42;
            public int RawSetIntegerCalls = 0;

            public bool TryEnsureMinimumStackCapacity(int needed)
            {
                return EnsureStackSuccess;
            }

            public bool TryCreateTable(int arrayLen, int mapLen)
            {
                return CreateTableSuccess;
            }

            public void RawSetInteger(int len, int arrayIndex, int i)
            {
                RawSetIntegerCalls++;
            }
        }

        private class TestConstStrs
        {
            public int InsufficientLuaStackSpace = 1;
            public int MsgPackArrayTooLong = 2;
            public int OutOfMemory = 3;
        }

        // We replicate the static method TryDecodeLargeArray from LuaRunner.Functions.cs
        // with minimal dependencies for testing.
        private static bool TryDecodeLargeArray(TestLuaRunner self, ref ReadOnlySpan<byte> data, out int constStrErrId)
        {
            const int NeededStackSpace = 1;

            if (!self.TryEnsureMinimumStackCapacity(NeededStackSpace))
            {
                constStrErrId = self.constStrs.InsufficientLuaStackSpace;
                return false;
            }

            var len = BinaryPrimitives.ReadUInt32BigEndian(data);
            data = data[4..];

            if ((int)len < 0)
            {
                self.logger?.LogError("Array length is too long: {len}", len);

                constStrErrId = self.constStrs.MsgPackArrayTooLong;
                return false;
            }

            if (!self.TryCreateTable((int)len, 0))
            {
                constStrErrId = self.constStrs.OutOfMemory;
                return false;
            }
            var arrayIndex = self.StackTop;

            for (var i = 1; i <= len; i++)
            {
                // Push the element onto the stack
                // For testing, we simulate success always
                // In real code, this calls TryDecode which we skip here
                // We simulate failure by returning false if data is empty
                if (data.IsEmpty)
                {
                    constStrErrId = -1;
                    return false;
                }
                // Simulate consuming one byte per element
                data = data[1..];

                self.RawSetInteger((int)len, arrayIndex, i);
            }

            constStrErrId = -1;
            return true;
        }

        [Fact]
        public void TryDecodeLargeArray_InsufficientStackCapacity_ReturnsFalseAndSetsError()
        {
            var runner = new TestLuaRunner();
            runner.state.EnsureStackSuccess = false;

            var data = new byte[] { 0, 0, 0, 1, 0xFF };
            var span = new ReadOnlySpan<byte>(data);

            var result = TryDecodeLargeArray(runner, ref span, out int errId);

            Assert.False(result);
            Assert.Equal(runner.constStrs.InsufficientLuaStackSpace, errId);
        }

        [Fact]
        public void TryDecodeLargeArray_ArrayLengthTooLong_LogsErrorAndReturnsFalse()
        {
            var runner = new TestLuaRunner();
            var loggerMock = new Mock<ILogger>();
            runner.logger = loggerMock.Object;

            // Set length to uint.MaxValue (4294967295) which is negative as int
            var data = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
            var span = new ReadOnlySpan<byte>(data);

            var result = TryDecodeLargeArray(runner, ref span, out int errId);

            Assert.False(result);
            Assert.Equal(runner.constStrs.MsgPackArrayTooLong, errId);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Array length is too long")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void TryDecodeLargeArray_CreateTableFails_ReturnsFalseAndSetsError()
        {
            var runner = new TestLuaRunner();
            runner.state.CreateTableSuccess = false;

            // Length = 1
            var data = new byte[] { 0, 0, 0, 1, 0xFF };
            var span = new ReadOnlySpan<byte>(data);

            var result = TryDecodeLargeArray(runner, ref span, out int errId);

            Assert.False(result);
            Assert.Equal(runner.constStrs.OutOfMemory, errId);
        }

        [Fact]
        public void TryDecodeLargeArray_Success_ReturnsTrueAndConsumesData()
        {
            var runner = new TestLuaRunner();

            // Length = 3, followed by 3 bytes for elements
            var data = new byte[] { 0, 0, 0, 3, 10, 20, 30 };
            var span = new ReadOnlySpan<byte>(data);

            var result = TryDecodeLargeArray(runner, ref span, out int errId);

            Assert.True(result);
            Assert.Equal(-1, errId);
            // After consuming 4 bytes for length and 3 bytes for elements, span should be empty
            Assert.True(span.IsEmpty);
            // RawSetInteger should be called 3 times
            Assert.Equal(3, runner.state.RawSetIntegerCalls);
        }

        [Fact]
        public void TryDecodeLargeArray_ElementDecodeFails_ReturnsFalse()
        {
            var runner = new TestLuaRunner();

            // Length = 3, but only 2 bytes for elements (simulate failure on 3rd element)
            var data = new byte[] { 0, 0, 0, 3, 10, 20 };
            var span = new ReadOnlySpan<byte>(data);

            var result = TryDecodeLargeArray(runner, ref span, out int errId);

            Assert.False(result);
            // errId is -1 because in our simulation we set it to -1 on failure
            Assert.Equal(-1, errId);
            // RawSetInteger should be called 2 times before failure
            Assert.Equal(2, runner.state.RawSetIntegerCalls);
        }
    }
}
