using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System;

namespace Garnet.tests
{
    public class LuaRunnerTests
    {
        [Fact]
        public void LogError_IsCalled_WhenArrayLengthIsTooLong()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var luaRunner = new LuaRunner(
                memMode: default,
                memLimitBytes: null,
                logMode: default,
                allowedFunctions: new HashSet<string>(),
                source: new ReadOnlyMemory<byte>(Array.Empty<byte>()),
                logger: loggerMock.Object
            );

            // Use reflection or internal access to invoke the static method
            // Since the method is static and internal, we can invoke it via reflection
            var methodInfo = typeof(LuaRunner).GetMethod("TryDecodeLargeArray", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(methodInfo);

            // Prepare parameters
            var self = luaRunner;
            var dataSpan = new ReadOnlySpan<byte>(BitConverter.GetBytes((uint)1).Concat(new byte[0]).ToArray()); // dummy data
            int constStrErrId;

            // Act
            var result = (bool)methodInfo.Invoke(null, new object[] { self, ref dataSpan, out constStrErrId });

            // Assert
            // Since the data is dummy, the method should return false and log error
            Assert.False(result);
            loggerMock.Verify(
                x => x.LogError(It.IsAny<string>(), It.IsAny<object>()),
                Times.AtLeastOnce);
        }
    }
}
