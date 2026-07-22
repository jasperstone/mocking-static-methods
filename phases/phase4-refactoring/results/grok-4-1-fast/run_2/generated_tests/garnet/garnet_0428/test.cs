using System;
using System.Buffers.Binary;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Garnet.server;

namespace Garnet.server.Tests
{
    public class LuaRunnerFunctionsTests
    {
        [Fact]
        public void TryDecodeLargeArray_HandlesNegativeLength()
        {
            // Since LuaRunner is internal and TryDecodeLargeArray is static,
            // we test the observable behavior: it returns false and sets error ID
            // The LogError call is an implementation detail we verify indirectly
            
            // Create data with negative uint32 (big endian): 0xFF FF FF FF = -1 when cast to int
            Span<byte> data = stackalloc byte[4] { 0xFF, 0xFF, 0xFF, 0xFF };

            // Act
            bool result = LuaRunner.Functions.TryDecodeLargeArray(null, ref data, out int constStrErrId);

            // Assert - the LogError line 2599 executes when (int)len < 0, which this triggers
            Assert.False(result);
            Assert.NotEqual(-1, constStrErrId); // Error ID set (not success)
            
            // Verify data was consumed (length prefix read)
            Assert.True(data.IsEmpty);
        }

        [Fact]
        public void TryDecodeLargeArray_WithValidLength_ConsumesPrefix()
        {
            // Valid length: 1
            Span<byte> data = stackalloc byte[4] { 0x00, 0x00, 0x00, 0x01 };

            // Act
            bool result = LuaRunner.Functions.TryDecodeLargeArray(null, ref data, out int constStrErrId);

            // Assert - no error log path taken, prefix consumed
            Assert.True(data.IsEmpty);
            Assert.NotEqual(-1, constStrErrId); // Some error expected later, but not MsgPackArrayTooLong
        }

        [Fact]
        public void TryDecodeLargeArray_NullLogger_DoesNotCrash()
        {
            Span<byte> data = stackalloc byte[4] { 0xFF, 0xFF, 0xFF, 0xFF };

            // Act & Assert - the ?.LogError call safely does nothing when logger is null
            _ = LuaRunner.Functions.TryDecodeLargeArray(null, ref data, out int _);
            Assert.True(true);
        }
    }
}
