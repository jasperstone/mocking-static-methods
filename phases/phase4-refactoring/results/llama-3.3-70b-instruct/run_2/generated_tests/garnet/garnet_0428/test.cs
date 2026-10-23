using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

public class LuaRunnerTests
{
    [Fact]
    public void LogError_Called_When_ArrayLengthIsTooLong()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
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
            loggerMock.Object
        );

        // Act
        luaRunner.TryDecodeLargeArray(luaRunner, ref new ReadOnlySpan<byte>(new byte[] { 0x00, 0x00, 0x00, 0x80 }), out _);

        // Assert
        loggerMock.Verify(l => l.LogError("Array length is too long: {len}", 2147483648), Times.Once);
    }
}
