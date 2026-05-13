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
        var logger = new Mock<ILogger<LuaRunner>>();
        var luaState = new Mock<LuaStateWrapper>();
        luaState.Setup(s => s.TryEnsureMinimumStackCapacity(It.IsAny<int>())).Returns(true);

        var constStrs = new Mock<ConstantStringRegistryIndexes>();
        constStrs.SetupGet(cs => cs.MsgPackArrayTooLong).Returns(123);

        var luaRunner = new LuaRunnerFixture(logger.Object, luaState.Object, constStrs.Object);

        // Create data with negative uint32 (big endian): 0xFF FF FF FF
        Span<byte> data = stackalloc byte[4];
        new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }.AsSpan().CopyTo(data);
        ref var dataRef = ref data[0];

        // Act
        var result = LuaRunner.Functions.TryDecodeLargeArray(luaRunner.Instance, ref dataRef, out var constStrErrId);

        // Assert
        Assert.False(result);
        Assert.Equal(123, constStrErrId);
        logger.Verify(
            l => l.Log(
                LogLevel.Error,
                0,
                It.Is<It.IsAnyType>((v, t) => ContainsMessage(v, "Array length is too long: 4294967295")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void TryDecodeLargeArray_DoesNotLog_WhenLoggerIsNull()
    {
        // Arrange
        var luaState = new Mock<LuaStateWrapper>();
        luaState.Setup(s => s.TryEnsureMinimumStackCapacity(It.IsAny<int>())).Returns(true);

        var constStrs = new Mock<ConstantStringRegistryIndexes>();
        constStrs.SetupGet(cs => cs.MsgPackArrayTooLong).Returns(123);

        var luaRunner = new LuaRunnerFixture(null, luaState.Object, constStrs.Object);

        Span<byte> data = stackalloc byte[4] { 0xFF, 0xFF, 0xFF, 0xFF };
        ref var dataRef = ref data[0];

        // Act
        var result = LuaRunner.Functions.TryDecodeLargeArray(luaRunner.Instance, ref dataRef, out var constStrErrId);

        // Assert
        Assert.False(result);
        Assert.Equal(123, constStrErrId);
    }

    private static bool ContainsMessage<T>(T state, string expectedMessage)
    {
        return state?.ToString().Contains(expectedMessage) == true;
    }

    // Fixture to create minimal LuaRunner instance for testing the static method
    private sealed class LuaRunnerFixture
    {
        internal LuaRunner Instance { get; }

        internal LuaRunnerFixture(ILogger logger, LuaStateWrapper state, ConstantStringRegistryIndexes constStrs)
        {
            // Create minimal LuaRunner with required fields populated
            var luaRunner = new LuaRunner(
                LuaMemoryManagementMode.Default,
                null,
                LuaLoggingMode.Default,
                new HashSet<string>(),
                ReadOnlyMemory<byte>.Empty,
                txnMode: false,
                respServerSession: null,
                scratchBufferNetworkSender: null,
                "0.0.0.0",
                logger);

            // Use reflection or unsafe code to set private fields for testing
            // Since LuaRunner is sealed partial, we set up the required fields
            UnsafeSetField(luaRunner, nameof(LuaRunner.state), state);
            UnsafeSetField(luaRunner, nameof(LuaRunner.constStrs), constStrs);
            UnsafeSetField(luaRunner, nameof(LuaRunner.logger), logger);

            Instance = luaRunner;
        }

        private static void UnsafeSetField<T>(T obj, string fieldName, object value)
        {
            var field = typeof(T).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }
    }
}
