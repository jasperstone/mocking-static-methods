using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Garnet.common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server;

public class LuaRunnerTests
{
    [Fact]
    public void TryDecodeLargeArray_LogsError_WhenArrayLengthIsNegative()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LuaRunner>>();
        var mockState = new Mock<LuaStateWrapper>();
        var mockConstStrs = new Mock<IConstantStringRegistryIndexes>();

        mockState.Setup(s => s.TryEnsureMinimumStackCapacity(It.IsAny<int>())).Returns(true);
        mockConstStrs.Setup(cs => cs.MsgPackArrayTooLong).Returns(123);

        var data = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(data, uint.MaxValue); // Negative when cast to int
        var dataSpan = new ReadOnlySpan<byte>(data);

        // Create LuaRunner instance (minimal setup for test)
        var luaRunner = new LuaRunner(
            LuaMemoryManagementMode.Default,
            null,
            LuaLoggingMode.Default,
            new HashSet<string>(),
            ReadOnlyMemory<byte>.Empty,
            false,
            null,
            null,
            "0.0.0.0",
            mockLogger.Object
        );

        // Set up internal state via reflection (since it's private/internal)
        using var pin = new PinnedObject(luaRunner.state);
        luaRunner.state = mockState.Object;
        luaRunner.constStrs = mockConstStrs.Object;
        luaRunner.logger = mockLogger.Object;

        // Act
        var result = LuaRunner.Functions.TryDecodeLargeArray(luaRunner, ref dataSpan, out var constStrErrId);

        // Assert
        Assert.False(result);
        Assert.Equal(123, constStrErrId);
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat<string>>(msg => msg.ToString().Contains("Array length is too long")),
                It.IsAny<object[]>(),
                It.IsAny<Exception>()
            ),
            Times.Once
        );
        mockLogger.Verify(l => l.LogError("Array length is too long: {len}", uint.MaxValue), Times.Once);
    }

    [Fact]
    public void TryDecodeLargeArray_DoesNotLog_WhenLoggerIsNull()
    {
        // Arrange
        var mockState = new Mock<LuaStateWrapper>();
        var mockConstStrs = new Mock<IConstantStringRegistryIndexes>();

        mockState.Setup(s => s.TryEnsureMinimumStackCapacity(It.IsAny<int>())).Returns(true);
        mockConstStrs.Setup(cs => cs.MsgPackArrayTooLong).Returns(123);

        var data = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(data, uint.MaxValue);
        var dataSpan = new ReadOnlySpan<byte>(data);

        var luaRunner = new LuaRunner(
            LuaMemoryManagementMode.Default,
            null,
            LuaLoggingMode.Default,
            new HashSet<string>(),
            ReadOnlyMemory<byte>.Empty,
            false,
            null,
            null,
            "0.0.0.0",
            null // No logger
        );

        luaRunner.state = mockState.Object;
        luaRunner.constStrs = mockConstStrs.Object;

        // Act
        var result = LuaRunner.Functions.TryDecodeLargeArray(luaRunner, ref dataSpan, out var constStrErrId);

        // Assert
        Assert.False(result);
        Assert.Equal(123, constStrErrId);
    }
}
