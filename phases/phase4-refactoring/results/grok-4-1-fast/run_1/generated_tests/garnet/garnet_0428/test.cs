using System;
using System.Buffers.Binary;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.server;

public class LuaRunnerFunctionsTests
{
    private class FakeLuaRunner
    {
        public ILogger logger;
        public LuaStateWrapper state = new FakeLuaStateWrapper();
        public ConstantStringRegistryIndexes constStrs = new FakeConstStrs();

        private class FakeLuaStateWrapper { }
        private class FakeConstStrs { public int MsgPackArrayTooLong => 123; }
    }

    [Fact]
    public void TryDecodeLargeArray_LogsError_WhenArrayLengthIsNegative()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);
        
        var fakeRunner = new FakeLuaRunner { logger = mockLogger.Object };
        var data = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }.AsSpan();

        // Act
        var result = LuaRunner.Functions.TryDecodeLargeArray(fakeRunner, ref data, out var constStrErrId);

        // Assert
        Assert.False(result);
        Assert.Equal(123, constStrErrId);
        
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                0,
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Array length is too long") && ((string)v).Contains("4294967295")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void TryDecodeLargeArray_SkipsLogging_WhenLoggerIsNull()
    {
        // Arrange
        var fakeRunner = new FakeLuaRunner { logger = null };
        var data = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }.AsSpan();

        // Act
        var result = LuaRunner.Functions.TryDecodeLargeArray(fakeRunner, ref data, out var constStrErrId);

        // Assert
        Assert.False(result);
        Assert.Equal(123, constStrErrId);
    }
}
