using System;
using System.Buffers.Binary;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Garnet.server;
using System.Reflection;

namespace Garnet.Tests
{
    public class LuaRunnerFunctionsTests
    {
        [Fact]
        public void TryDecodeLargeArray_LogsError_WhenArrayLengthTooLong()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var runnerType = typeof(LuaRunner);

            // Create an instance of LuaRunner using non-public constructor
            var runner = (LuaRunner)Activator.CreateInstance(runnerType, nonPublic: true);

            // Set the private readonly logger field via reflection
            var loggerField = runnerType.GetField("logger", BindingFlags.Instance | BindingFlags.NonPublic);
            loggerField.SetValue(runner, loggerMock.Object);

            // Prepare data with length = 0xFFFFFFFF (max uint)
            var dataArray = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
            var data = new ReadOnlySpan<byte>(dataArray);

            // Prepare out parameter
            int constStrErrId;

            // Act
            var result = InvokeTryDecodeLargeArray(runner, ref data, out constStrErrId);

            // Assert
            Assert.False(result);
            Assert.NotEqual(0, constStrErrId); // Should be set to MsgPackArrayTooLong constant string id
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Array length is too long")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private static bool InvokeTryDecodeLargeArray(LuaRunner runner, ref ReadOnlySpan<byte> data, out int constStrErrId)
        {
            var method = typeof(LuaRunner).GetMethod("TryDecodeLargeArray", BindingFlags.NonPublic | BindingFlags.Static);
            object[] parameters = { runner, data, null };
            var result = (bool)method.Invoke(null, parameters);
            constStrErrId = (int)parameters[2];
            return result;
        }
    }
}
