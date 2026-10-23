using Xunit;
using Moq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Garnet.server;

namespace Garnet.Tests
{
    public class LuaRunnerLoggingTests
    {
        [Fact]
        public void TryDecodeLargeArray_ShouldLogError_WhenArrayLengthIsTooLong()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var data = new ReadOnlySpan<byte>(BitConverter.GetBytes(uint.MaxValue + 1));
            var luaRunnerType = typeof(LuaRunner);
            var luaRunnerInstance = Activator.CreateInstance(luaRunnerType, nonPublic: true);

            // Set the logger field via reflection
            var loggerField = luaRunnerType.GetField("logger", BindingFlags.NonPublic | BindingFlags.Instance);
            loggerField.SetValue(luaRunnerInstance, loggerMock.Object);

            // Get the method info for TryDecodeLargeArray
            var methodInfo = luaRunnerType.GetMethod("TryDecodeLargeArray", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(methodInfo);

            // Act
            var result = (bool)methodInfo.Invoke(null, new object[] { luaRunnerInstance, data, null });
            var errorId = (int)methodInfo.Invoke(null, new object[] { luaRunnerInstance, data, null });

            // Assert
            loggerMock.Verify(
                x => x.LogError("Array length is too long: {len}", It.IsAny<uint>()),
                Times.Once);
            Assert.False(result);
            // Assuming MsgPackArrayTooLong string constant is "MsgPackArrayTooLong"
            Assert.Equal("MsgPackArrayTooLong", errorId);
        }
    }
}
