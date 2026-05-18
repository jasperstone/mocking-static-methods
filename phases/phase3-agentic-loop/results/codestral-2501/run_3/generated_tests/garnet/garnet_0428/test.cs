using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;
using System;
using System.Collections.Generic;
using System.Reflection;

public class LuaRunnerFunctionsTests
{
    [Fact]
    public void TryDecodeLargeArray_ArrayLengthTooLong_LogsError()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LuaRunner>>();
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

        var data = new ReadOnlySpan<byte>(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }); // Array length that is too long

        // Act
        var method = typeof(LuaRunner).GetMethod("TryDecodeLargeArray", BindingFlags.NonPublic | BindingFlags.Static);
        var result = (bool)method.Invoke(null, new object[] { luaRunner, data, 0 });

        // Assert
        Assert.False(result);
        mockLogger.Verify(
            logger => logger.LogError("Array length is too long: {len}", It.IsAny<uint>()),
            Times.Once
        );
    }
}
