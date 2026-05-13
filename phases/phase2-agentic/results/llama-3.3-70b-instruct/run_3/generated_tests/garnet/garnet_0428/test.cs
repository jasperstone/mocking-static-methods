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
            ReadOnlyMemory<byte>.Empty,
            false,
            null,
            null,
            "0.0.0.0",
            loggerMock.Object);

        // Act
        luaRunner.TryDecodeLargeArray(luaRunner, ref ReadOnlySpan<byte>.Empty, out _);

        // Assert
        loggerMock.Verify(l => l.LogError("Array length is too long: {len}", It.IsAny<int>()), Times.Once);
    }
}
