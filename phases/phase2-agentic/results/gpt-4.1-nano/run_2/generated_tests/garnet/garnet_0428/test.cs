using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.server;

namespace Garnet.Tests
{
    public class LuaRunnerTests
    {
        [Fact]
        public void TryDecodeLargeArray_Should_LogError_When_ArrayLengthIsTooLong()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var stateMock = new Mock<ILuaState>();
            var runner = new LuaRunner
            {
                logger = loggerMock.Object,
                state = stateMock.Object,
                constStrs = new ConstStrings
                {
                    MsgPackArrayTooLong = 1001,
                    OutOfMemory = 1002,
                }
            };

            var data = new ReadOnlySpan<byte>(BitConverter.GetBytes((uint)int.MaxValue + 1));
            var dataRef = data;

            // Setup state mock to simulate TryEnsureMinimumStackCapacity returning true
            stateMock.Setup(s => s.TryEnsureMinimumStackCapacity(It.IsAny<int>())).Returns(true);
            // Setup TryCreateTable to return true
            stateMock.Setup(s => s.TryCreateTable(It.IsAny<int>(), It.IsAny<int>())).Returns(true);
            // Setup RawSetInteger to do nothing
            stateMock.Setup(s => s.RawSetInteger(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()));

            // Act
            var result = LuaRunner.TryDecodeLargeArray(runner, ref dataRef, out int errorId);

            // Assert
            Assert.False(result);
            Assert.Equal(runner.constStrs.MsgPackArrayTooLong, errorId);
            loggerMock.Verify(
                x => x.LogError("Array length is too long: {len}", It.IsAny<uint>()),
                Times.Once);
        }
    }
}
