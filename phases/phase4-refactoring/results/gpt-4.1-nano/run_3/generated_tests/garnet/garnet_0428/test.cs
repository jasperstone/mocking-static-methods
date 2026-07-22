using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
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
            var stateMock = new Mock<ILuaStateWrapper>();
            var runner = new LuaRunner
            {
                logger = loggerMock.Object,
                state = stateMock.Object,
                // Setup other dependencies if needed
            };

            var data = new ReadOnlySpan<byte>(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }); // large length
            var self = new LuaRunner
            {
                logger = loggerMock.Object,
                state = stateMock.Object,
            };

            // Act
            var result = LuaRunner.TryDecodeLargeArray(self, ref data, out int errorId);

            // Assert
            loggerMock.Verify(
                x => x.LogError("Array length is too long: {len}", It.IsAny<uint>()),
                Times.Once);
            Assert.False(result);
            Assert.Equal(self.constStrs.MsgPackArrayTooLong, errorId);
        }
    }
}
