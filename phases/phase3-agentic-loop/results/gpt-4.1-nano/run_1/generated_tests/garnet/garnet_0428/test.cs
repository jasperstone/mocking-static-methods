using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;

namespace Garnet.Tests
{
    public class LuaRunnerTests
    {
        [Fact]
        public void TryDecodeLargeArray_ShouldLogError_WhenArrayLengthIsTooLong()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var self = new LuaRunner
            {
                logger = loggerMock.Object,
                constStrs = new ConstStrings
                {
                    MsgPackArrayTooLong = 42,
                },
                // Setup other dependencies as needed
            };

            var data = new ReadOnlySpan<byte>(BitConverter.GetBytes(uint.MaxValue + 1));
            // Act
            var result = LuaRunner.TryDecodeLargeArray(self, ref data, out var errorId);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.LogError("Array length is too long: {len}", It.IsAny<uint>()),
                Times.Once);
        }
    }
}
