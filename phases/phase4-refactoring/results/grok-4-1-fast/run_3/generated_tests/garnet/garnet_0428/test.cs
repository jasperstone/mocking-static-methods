using System;
using System.Buffers.Binary;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Garnet.server;

namespace Garnet.server.Tests
{
    public class LuaRunnerFunctionsTests
    {
        [Fact]
        public void TryDecodeLargeArray_LogsError_WhenArrayLengthIsNegative()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<LuaRunner>>();
            mockLogger.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString().Contains("Array length is too long: 4294967295") == true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            byte[] dataArray = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
            ref ReadOnlySpan<byte> dataRef = ref dataArray.AsSpan();

            // Create LuaRunner instance (using parameterless constructor or minimal params)
            // Since LuaRunner is internal, we test the static method directly with mocked logger via reflection if needed
            // But for static method testing, we focus on the observable behavior

            // Act
            bool result = LuaRunner.Functions.TryDecodeLargeArray(
                CreateTestLuaRunner(mockLogger.Object), 
                ref dataRef, 
                out int constStrErrId);

            // Assert
            Assert.False(result);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v?.ToString().Contains("Array length is too long: 4294967295") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private static LuaRunner CreateTestLuaRunner(ILogger logger)
        {
            // Minimal constructor call - use actual enum values from source
            return new LuaRunner(
                0, // LuaMemoryManagementMode - use 0 as placeholder
                null,
                0, // LuaLoggingMode - use 0 as placeholder  
                new System.Collections.Generic.HashSet<string>(),
                ReadOnlyMemory<byte>.Empty,
                false,
                null,
                null,
                "0.0.0.0",
                logger);
        }
    }
}
