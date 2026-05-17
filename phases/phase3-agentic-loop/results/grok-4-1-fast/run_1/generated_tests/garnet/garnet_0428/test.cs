using System;
using System.Buffers.Binary;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Garnet.server.Tests
{
    public class LuaRunnerFunctionsTests
    {
        [Fact]
        public void TryDecodeLargeArray_LogsError_WhenArrayLengthNegative()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString().Contains("Array length is too long") && 
                    v.ToString().Contains("4294967295")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ));

            var luaRunner = new MockLuaRunner { logger = mockLogger.Object };

            // Act
            bool result = LuaRunner.Functions.TryDecodeLargeArray(luaRunner.Object, ref data, out _);

            // Assert
            Assert.False(result);
            Assert.True(data.IsEmpty);
            mockLogger.Verify();
        }

        [Fact]
        public void TryDecodeLargeArray_DoesNotLog_WhenLoggerNull()
        {
            // Arrange
            var luaRunner = new MockLuaRunner { logger = null };

            // Act
            bool result = LuaRunner.Functions.TryDecodeLargeArray(luaRunner.Object, ref data, out _);

            // Assert
            Assert.False(result);
            Assert.True(data.IsEmpty);
        }

        private static ReadOnlySpan<byte> data = stackalloc byte[] { 0xFF, 0xFF, 0xFF, 0xFF };

        // Minimal mock implementations
        private class MockLuaRunner : Mock<LuaRunner>
        {
            public new ILogger logger;
            public LuaStateWrapper state = new MockLuaStateWrapper();

            public MockLuaRunner()
            {
                this.CallBase = true;
                // Mock state property
                this.SetupProperty(x => x.state);
            }
        }

        private class MockLuaStateWrapper
        {
            public bool TryEnsureMinimumStackCapacity(int needed) => true;
            public bool TryCreateTable(int len, int zero) => true;
            public int StackTop => 1;
            public void RawSetInteger(int len, int arrayIndex, int i) { }
        }
    }
}
