using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;

namespace Tsavorite.core.Tests
{
    public class IndexRecoveryTests
    {
        private class DummyDevice : IDevice
        {
            public int SectorSize { get; set; } = 512;
            public List<(ulong offset, IntPtr buffer, uint size, Action<uint, uint, object> callback, object overlap)> Calls { get; } = new();

            public void ReadAsync(ulong offset, IntPtr buffer, uint size, Action<uint, uint, object> callback, object overlap)
            {
                Calls.Add((offset, buffer, size, callback, overlap));
            }

            public void Dispose() { }
        }

        private class DummyLogger : ILogger
        {
            public List<string> Errors { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (logLevel == LogLevel.Error)
                {
                    Errors.Add(formatter(state, exception));
                }
            }
        }

        private class DummyState
        {
            public HashBucket* tableAligned;
            public int size;
        }

        private class DummyHashBucket
        {
            public ulong[] bucket_entries = new ulong[Constants.kOverflowBucketIndex + 1];
        }

        private class DummyOverflowAllocator
        {
            public object GetPhysicalAddress(ulong address) => new DummyHashBucket();
            public bool IsRecoveryCompleted(bool wait) => true;
            public Task RecoverAsync(IDevice device, ulong size, int buckets, ulong ofbBytes, CancellationToken token) => Task.CompletedTask;
        }

        private class DummyConstants
        {
            public const int kOverflowBucketIndex = 4;
            public const ulong kAddressMask = 0xFFFFFFFFFFFFFFFF;
        }

        private class DummyConstantsHolder
        {
            public static class Constants
            {
                public const int kOverflowBucketIndex = 4;
                public const ulong kAddressMask = 0xFFFFFFFFFFFFFFFF;
            }
        }

        private class IndexRecoveryWrapper : IndexRecovery
        {
            public IndexRecoveryWrapper(ILogger logger, DummyState[] states, DummyOverflowAllocator overflowAllocator)
            {
                this.logger = logger;
                this.state = states;
                this.overflowBucketsAllocator = overflowAllocator;
            }

            public DummyState[] state;
            public DummyOverflowAllocator overflowBucketsAllocator;
            public ILogger logger;

            public void SetRecoveryCountdown(CountdownWrapper countdown)
            {
                this.recoveryCountdown = countdown;
            }
        }

        [Fact]
        public void AsyncPageReadCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var logger = new DummyLogger();
            var recovery = new IndexRecoveryWrapper(logger, new DummyState[1], new DummyOverflowAllocator());
            var countdown = new CountdownWrapper(1, false);
            recovery.SetRecoveryCountdown(countdown);

            // Act
            recovery.AsyncPageReadCallback(1, 0, null);

            // Assert
            Assert.Contains("AsyncPageReadCallback error: {errorCode}", logger.Errors[0]);
        }

        [Fact]
        public void AsyncPageReadCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var logger = new DummyLogger();
            var recovery = new IndexRecoveryWrapper(logger, new DummyState[1], new DummyOverflowAllocator());
            var countdown = new CountdownWrapper(1, false);
            recovery.SetRecoveryCountdown(countdown);

            // Act
            recovery.AsyncPageReadCallback(0, 0, null);

            // Assert
            Assert.Empty(logger.Errors);
        }

        [Fact]
        public void InitializeMainIndexRecovery_CallsDeviceReadAsync_CorrectNumberOfTimes()
        {
            // Arrange
            var device = new DummyDevice();
            var states = new DummyState[1];
            var state = new DummyState { size = 1024, tableAligned = (HashBucket*)0x1000 };
            states[0] = state;
            var overflowAllocator = new DummyOverflowAllocator();
            var logger = new DummyLogger();
            var recovery = new IndexRecoveryWrapper(logger, states, overflowAllocator);
            recovery.recoveryCountdown = new CountdownWrapper(1, false);

            // Act
            var indexRecovery = recovery;
            var totalSize = state.size * sizeof(HashBucket);
            indexRecovery.BeginMainIndexRecovery(0, device, (ulong)totalSize);

            // Assert
            Assert.Single(device.Calls);
        }
    }
}
