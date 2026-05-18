using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.Tests
{
    // Public derived class to access internal LuaRunner
    public class TestableLuaRunner : LuaRunner
    {
        public TestableLuaRunner(
            LuaMemoryManagementMode memMode,
            int? memLimitBytes,
            LuaLoggingMode logMode,
            HashSet<string> allowedFunctions,
            ReadOnlyMemory<byte> source,
            bool txnMode = false,
            RespServerSession respServerSession = null,
            ScratchBufferNetworkSender scratchBufferNetworkSender = null,
            string redisVersion = "0.0.0.0",
            ILogger logger = null
        ) : base(memMode, memLimitBytes, logMode, allowedFunctions, source, txnMode, respServerSession, scratchBufferNetworkSender, redisVersion, logger)
        {
        }
    }

    // Public derived class to access internal RespServerSession
    public class TestableRespServerSession : RespServerSession
    {
        public TestableRespServerSession()
        {
        }
    }

    // Public derived class to access internal ScratchBufferNetworkSender
    public class TestableScratchBufferNetworkSender : ScratchBufferNetworkSender
    {
        public TestableScratchBufferNetworkSender()
        {
        }
    }

    public class LuaRunnerTests
    {
        [Fact]
        public void TryDecodeLargeArray_LogsError_WhenArrayLengthIsTooLong()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var luaRunner = new TestableLuaRunner(
                LuaMemoryManagementMode.Default, // Adjust if necessary
                null,
                LuaLoggingMode.Default, // Adjust if necessary
                new HashSet<string>(),
                new ReadOnlyMemory<byte>(),
                logger: loggerMock.Object
            );

            // Prepare data with a negative length
            var data = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }; // Represents a negative length in big-endian

            // Act
            bool result = LuaRunner.TryDecodeLargeArray(luaRunner, ref data, out int constStrErrId);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                l => l.LogError(It.IsAny<string>(), It.Is<object[]>(args => args[0].ToString() == "-1")),
                Times.Once
            );
        }
    }
}
