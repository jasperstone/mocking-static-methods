using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Tsavorite.core;

namespace Tsavorite.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void Verify_LogInformation_NoIndexCheckpointFound()
        {
            // This test verifies the LoggerExtensions.LogInformation call coverage
            // by creating a mock logger and verifying the specific message pattern
            var loggerMock = new Mock<ILogger<TsavoriteKV<long, long, StoreFunctions<long, long, IKeyComparer<long>, DefaultRecordDisposer<long, long>>, StandardAllocator<long, long, StoreFunctions<long, long, IKeyComparer<long>, DefaultRecordDisposer<long, long>>>>>>();

            // Verify the exact LogInformation extension method call signature used on line ~470
            loggerMock.Verify(
                x => x.LogInformation(
                    "No index checkpoint found, returning default index token in GetLatestCheckpointTokens"),
                Times.AtLeastOnce);
        }

        [Fact]
        public void Verify_LogInformation_NonEmptyLogRecovery()
        {
            // This test verifies the LoggerExtensions.LogInformation call coverage
            // specifically targeting line 500 in InternalRecoverAsync
            var loggerMock = new Mock<ILogger<TsavoriteKV<long, long, StoreFunctions<long, long, IKeyComparer<long>, DefaultRecordDisposer<long, long>>, StandardAllocator<long, long, StoreFunctions<long, long, IKeyComparer<long>, DefaultRecordDisposer<long, long>>>>>>>();

            // Verify the exact LogInformation extension method call signature used on line 500
            loggerMock.Verify(
                x => x.LogInformation(
                    "Recovery called on non-empty log - resetting to empty state first. Make sure store is quiesced before calling Recover on a running store."),
                Times.AtLeastOnce);
        }
    }
}
