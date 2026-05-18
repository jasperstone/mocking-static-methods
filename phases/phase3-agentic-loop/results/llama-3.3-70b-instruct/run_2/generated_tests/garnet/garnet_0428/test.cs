using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;

namespace Garnet.server
{
    public class LuaRunnerTests
    {
        [Fact]
        public void TryDecodeLargeArray_LogsError_WhenArrayLengthIsTooLong()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            internal class LuaRunnerInternal : LuaRunner
            {
                public LuaRunnerInternal(
                    LuaMemoryManagementMode memMode,
                    int? memLimitBytes,
                    LuaLoggingMode logMode,
                    System.Collections.Generic.HashSet<string> allowedFunctions,
                    ReadOnlyMemory<byte> source,
                    bool txnMode = false,
                    RespServerSession respServerSession = null,
                    ScratchBufferNetworkSender scratchBufferNetworkSender = null,
                    string redisVersion = "0.0.0.0",
                    ILogger logger = null
                ) : base(
                    memMode,
                    memLimitBytes,
                    logMode,
                    allowedFunctions,
                    source,
                    txnMode,
                    respServerSession,
                    scratchBufferNetworkSender,
                    redisVersion,
                    logger
                )
                {
                }
            }
            var luaRunner = new LuaRunnerInternal(
                LuaMemoryManagementMode.Managed,
                null,
                LuaLoggingMode.Managed,
                new System.Collections.Generic.HashSet<string>(),
                new byte[0],
                false,
                null,
                null,
                "0.0.0.0",
                loggerMock.Object
            );

            var data = new byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(data, uint.MaxValue);

            // Act
            var result = LuaRunner.TryDecodeLargeArray(luaRunner, ref data, out _);

            // Assert
            loggerMock.Verify(l => l.LogError("Array length is too long: {len}", uint.MaxValue), Times.Once);
            Assert.False(result);
        }
    }
}
