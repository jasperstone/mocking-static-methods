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
        }

        private class TestState
        {
            public int StackTop { get; set; } = 1;
            public bool TryEnsureMinimumStackCapacity(int needed) => true;
            public bool TryCreateTable(int arrayLen, int mapLen) => true;
            public void RawSetInteger(int len, int arrayIndex, int i) { }
        }

        private class TestConstStrs
        {
            public int InsufficientLuaStackSpace { get; } = 1;
            public int MsgPackArrayTooLong { get; } = 2;
            public int OutOfMemory { get; } = 3;
        }

        // We replicate the static method TryDecodeLargeArray from LuaRunner.Functions.cs
        // but adapted for testing with our TestLuaRunner class.
        private static bool TryDecodeLargeArray(TestLuaRunner self, ref ReadOnlySpan<byte> data, out int constStrErrId)
        {
            const int NeededStackSpace = 1;

            if (!self.state.TryEnsureMinimumStackCapacity(NeededStackSpace))
            {
                constStrErrId = self.constStrs.InsufficientLuaStackSpace;
                return false;
            }

            if (data.Length < 4)
            {
                constStrErrId = -1;
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

            if (!self.state.TryCreateTable((int)len, 0))
            {
                constStrErrId = self.constStrs.OutOfMemory;
                return false;
            }
            var arrayIndex = self.state.StackTop;

            for (var i = 1; i <= len; i++)
            {
                // For testing, we simulate TryDecode always succeeds
                // and consume one byte from data if available
                if (data.IsEmpty)
                {
                    constStrErrId = -1;
                    return false;
                }
                data = data[1..];

                self.state.RawSetInteger((int)len, arrayIndex, i);
            }

            constStrErrId = -1;
            return true;
        }

        [Fact]
        public void TryDecodeLargeArray_LogsError_WhenLengthTooLong()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var runner = new TestLuaRunner
            {
                logger = mockLogger.Object
            };

            // Create data with length > int.MaxValue (simulate negative int cast)
            // Use 0xFF FF FF FF which is uint.MaxValue (4294967295)
            var dataBytes = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
            var data = new ReadOnlySpan<byte>(dataBytes);

            // Act
            var result = TryDecodeLargeArray(runner, ref data, out int errId);

            // Assert
            Assert.False(result);
            Assert.Equal(runner.constStrs.MsgPackArrayTooLong, errId);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Array length is too long")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void TryDecodeLargeArray_ReturnsFalse_WhenStackCapacityInsufficient()
        {
            // Arrange
            var runner = new TestLuaRunner();
            runner.state = new TestStateInsufficientStack();

            var dataBytes = new byte[] { 0, 0, 0, 1 };
            var data = new ReadOnlySpan<byte>(dataBytes);

            // Act
            var result = TryDecodeLargeArray(runner, ref data, out int errId);

            // Assert
            Assert.False(result);
            Assert.Equal(runner.constStrs.InsufficientLuaStackSpace, errId);
        }

        private class TestStateInsufficientStack : TestState
        {
            public override bool TryEnsureMinimumStackCapacity(int needed) => false;
        }

        [Fact]
        public void TryDecodeLargeArray_ReturnsFalse_WhenTryCreateTableFails()
        {
            // Arrange
            var runner = new TestLuaRunner();
            runner.state = new TestStateCreateTableFails();

            var dataBytes = new byte[] { 0, 0, 0, 1, 0x01 };
            var data = new ReadOnlySpan<byte>(dataBytes);

            // Act
            var result = TryDecodeLargeArray(runner, ref data, out int errId);

            // Assert
            Assert.False(result);
            Assert.Equal(runner.constStrs.OutOfMemory, errId);
        }

        private class TestStateCreateTableFails : TestState
        {
            public override bool TryCreateTable(int arrayLen, int mapLen) => false;
        }

        [Fact]
        public void TryDecodeLargeArray_ReturnsTrue_WhenDataIsValid()
        {
            // Arrange
            var runner = new TestLuaRunner();

            // Length = 2, then 2 bytes for elements
            var dataBytes = new byte[] { 0, 0, 0, 2, 0x01, 0x02 };
            var data = new ReadOnlySpan<byte>(dataBytes);

            // Act
            var result = TryDecodeLargeArray(runner, ref data, out int errId);

            // Assert
            Assert.True(result);
            Assert.Equal(-1, errId);
            Assert.Equal(0, data.Length); // All data consumed
        }
    }
}
