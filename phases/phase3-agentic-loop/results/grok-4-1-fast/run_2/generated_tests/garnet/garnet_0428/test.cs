using System;
using System.Buffers.Binary;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server;

public class LuaRunnerFunctionsTests
{
    private sealed class LuaRunnerMock
    {
        public ILogger? Logger { get; set; }
        public LuaStateWrapperMock State { get; } = new();
        public ConstStrsMock ConstStrs { get; } = new();

        public LuaRunnerMock(ILogger? logger = null)
        {
            Logger = logger;
        }
    }

    private sealed class LuaStateWrapperMock
    {
        public bool TryEnsureMinimumStackCapacityResult = true;
        public bool TryCreateTableResult = true;
        public int StackTop = 1;

        public bool TryEnsureMinimumStackCapacity(int needed) => TryEnsureMinimumStackCapacityResult;
        public bool TryCreateTable(int len, int numFields) => TryCreateTableResult;
        public int StackTop => StackTop;
        public void RawSetInteger(int len, int arrayIndex, int i) { }
    }

    private sealed class ConstStrsMock
    {
        public int InsufficientLuaStackSpace = 1;
        public int MsgPackArrayTooLong = 2;
        public int OutOfMemory = 3;
    }

    [Fact]
    public void TryDecodeLargeArray_LogsError_WhenArrayLengthIsNegative()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var mock = new LuaRunnerMock(loggerMock.Object);

        Span<byte> data = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(data, uint.MaxValue);

        // Act
        var result = LuaRunner.Functions.TryDecodeLargeArray(mock, ref data, out var constStrErrId);

        // Assert
        Assert.False(result);
        Assert.Equal(mock.ConstStrs.MsgPackArrayTooLong, constStrErrId);
        loggerMock.Verify(x => x.LogError("Array length is too long: {len}", uint.MaxValue), Times.Once);
    }

    [Fact]
    public void TryDecodeLargeArray_DoesNotLog_WhenLoggerIsNull()
    {
        // Arrange
        var mock = new LuaRunnerMock(null);

        Span<byte> data = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(data, uint.MaxValue);

        // Act
        var result = LuaRunner.Functions.TryDecodeLargeArray(mock, ref data, out var constStrErrId);

        // Assert
        Assert.False(result);
        Assert.Equal(mock.ConstStrs.MsgPackArrayTooLong, constStrErrId);
    }

    [Fact]
    public void TryDecodeLargeArray_ReturnsFalse_WhenStackCapacityInsufficient()
    {
        // Arrange
        var mock = new LuaRunnerMock();
        mock.State.TryEnsureMinimumStackCapacityResult = false;

        Span<byte> data = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(data, uint.MaxValue);

        // Act
        var result = LuaRunner.Functions.TryDecodeLargeArray(mock, ref data, out var constStrErrId);

        // Assert
        Assert.False(result);
        Assert.Equal(mock.ConstStrs.InsufficientLuaStackSpace, constStrErrId);
    }
}
