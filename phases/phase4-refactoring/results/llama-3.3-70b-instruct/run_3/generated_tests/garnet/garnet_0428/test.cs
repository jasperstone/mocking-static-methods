using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;

namespace LuaRunnerTests
{
    public class LuaRunnerFunctionsTests
    {
        [Fact]
        public void TryDecodeLargeArray_LogsError_WhenArrayLengthIsTooLong()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var luaRunner = new LuaRunner(
                LuaMemoryManagementMode.Default,
                null,
                LuaLoggingMode.Default,
                new HashSet<string>(),
                new byte[0],
                false,
                null,
                null,
                "0.0.0.0",
                loggerMock.Object);
            var data = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }; // array length is too long

            // Act
            var result = LuaRunner.Functions.TryDecodeLargeArray(luaRunner, ref data, out _);

            // Assert
            loggerMock.Verify(l => l.LogError("Array length is too long: {len}", -1), Times.Once);
            Assert.False(result);
        }
    }
}
