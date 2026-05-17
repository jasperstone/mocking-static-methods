using System;
using System.Buffers.Binary;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server
{
    public class LuaRunnerFunctionsTests
    {
        private static readonly MethodInfo TryDecodeLargeArrayMethod = typeof(LuaRunner).GetMethod("TryDecodeLargeArray", BindingFlags.NonPublic | BindingFlags.Static);

        private class DummyLuaStateWrapper
        {
            public bool TryEnsureMinimumStackCapacity(int needed) => true;
            public bool TryCreateTable(int arraySize, int hashSize) => true;
            public int StackTop => 0;
            public void RawSetInteger(int len, int arrayIndex, int i) { }
        }

        private class DummyConstStrs
        {
            public int MsgPackArrayTooLong => 12345;
        }

        private LuaRunner CreateLuaRunnerWithLogger(Mock<ILogger> loggerMock)
        {
            var luaRunner = (LuaRunner)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(LuaRunner));

            var loggerField = typeof(LuaRunner).GetField("logger", BindingFlags.NonPublic | BindingFlags.Instance);
            loggerField.SetValue(luaRunner, loggerMock.Object);

            var stateField = typeof(LuaRunner).GetField("state", BindingFlags.NonPublic | BindingFlags.Instance);
            stateField.SetValue(luaRunner, new DummyLuaStateWrapper());

            var constStrsField = typeof(LuaRunner).GetField("constStrs", BindingFlags.NonPublic | BindingFlags.Instance);
            constStrsField.SetValue(luaRunner, new DummyConstStrs());

            return luaRunner;
        }

        [Fact]
        public void TryDecodeLargeArray_LogsError_WhenArrayLengthTooLong()
        {
            var loggerMock = new Mock<ILogger>();
            var luaRunner = CreateLuaRunnerWithLogger(loggerMock);

            // Prepare data with length = uint.MaxValue (0xFFFFFFFF)
            byte[] dataBytes = new byte[4] { 0xFF, 0xFF, 0xFF, 0xFF };
            ReadOnlySpan<byte> dataSpan = dataBytes;

            object[] parameters = new object[] { luaRunner, dataSpan, null };

            bool result = (bool)TryDecodeLargeArrayMethod.Invoke(null, parameters);
            int constStrErrId = (int)parameters[2];

            Assert.False(result);
            Assert.Equal(12345, constStrErrId);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Array length is too long")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
