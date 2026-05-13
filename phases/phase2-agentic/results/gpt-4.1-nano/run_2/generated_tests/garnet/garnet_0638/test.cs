using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Tsavorite.core;

namespace Tsavorite.Tests
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

        private class DummyOverflowAllocator
        {
            public List<IntPtr> Addresses { get; } = new();
            public void RecoverAsync(IDevice device, ulong indexSize, int numBuckets, ulong numOfbBytes, CancellationToken token)
            {
                // simulate recovery
            }
            public IntPtr GetPhysicalAddress(IntPtr address) => address;
            public bool IsRecoveryCompleted(bool wait) => true;
        }

        private class DummyState
        {
            public int Size { get; set; }
            public HashBucket* TableAligned { get; set; }
        }

        private class DummyHashBucket
        {
            public ulong[] bucket_entries = new ulong[Constants.kOverflowBucketIndex + 1];
        }

        private class DummyConstants
        {
            public const int kOverflowBucketIndex = 4;
            public const ulong kAddressMask = 0xFFFFFFFFFFFFFFFF;
        }

        private class DummyHashBucketStruct
        {
            public ulong[] bucket_entries = new ulong[Constants.kOverflowBucketIndex + 1];
        }

        private class DummyConstantsClass
        {
            public const int kOverflowBucketIndex = 4;
            public const ulong kAddressMask = 0xFFFFFFFFFFFFFFFF;
        }

        private class DummyHashBucketEntry
        {
            public ulong word;
            public bool Tentative => (word & 1) != 0;
        }

        private class DummyOverflowBucketsAllocator
        {
            public List<IntPtr> Addresses { get; } = new();
            public IntPtr GetPhysicalAddress(IntPtr address) => address;
            public bool IsRecoveryCompleted(bool wait) => true;
        }

        private class DummyStateArray
        {
            public DummyState[] States { get; } = new DummyState[10];

            public DummyState this[int index] => States[index];
        }

        private class DummyResizeInfo
        {
            public int version = 0;
        }

        private class DummyLoggerFactory : ILoggerFactory
        {
            public ILogger CreateLogger(string categoryName) => new DummyLogger();
            public void AddProvider(ILoggerProvider provider) { }
            public void Dispose() { }
        }

        [Fact]
        public void AsyncPageReadCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var recovery = new IndexRecovery
            {
                logger = loggerMock.Object,
                recoveryCountdown = new CountdownWrapper(1, false)
            };
            uint errorCode = 123;
            uint numBytes = 0;
            object overlap = null;

            // Act
            recovery.AsyncPageReadCallback(errorCode, numBytes, overlap);

            // Assert
            loggerMock.Verify(l => l.LogError(It.Is<string>(s => s.Contains("AsyncPageReadCallback error")), errorCode), Times.Once);
        }

        [Fact]
        public void AsyncPageReadCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var recovery = new IndexRecovery
            {
                logger = loggerMock.Object,
                recoveryCountdown = new CountdownWrapper(1, false)
            };
            uint errorCode = 0;
            uint numBytes = 0;
            object overlap = null;

            // Act
            recovery.AsyncPageReadCallback(errorCode, numBytes, overlap);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<uint>()), Times.Never);
        }

        [Fact]
        public void BeginMainIndexRecovery_SplitsIntoChunksAndCallsReadAsync()
        {
            // Arrange
            var device = new DummyDevice();
            var stateArray = new DummyState[1];
            var startBucket = new DummyHashBucket();
            var startPtr = (HashBucket*)System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(startBucket.bucket_entries, 0);
            var recovery = new IndexRecovery
            {
                recoveryCountdown = new CountdownWrapper(1, false),
                logger = new DummyLogger(),
                state = new DummyState[] { new DummyState { size = 10, tableAligned = startPtr } },
                overflowBucketsAllocator = new DummyOverflowBucketsAllocator()
            };
            int version = 0;
            ulong totalSize = (ulong)(stateArray[0].size * sizeof(HashBucket));
            ulong expectedNumBytes = totalSize;
            // Act
            recovery.BeginMainIndexRecovery(version, device, expectedNumBytes, false);

            // Assert
            Assert.NotEmpty(device.Calls);
            var call = device.Calls.First();
            Assert.Equal(expectedNumBytes, call.size);
        }

        [Fact]
        public void IsMainIndexRecoveryCompleted_ReturnsTrue_WhenRecoveryIsComplete()
        {
            // Arrange
            var recovery = new IndexRecovery
            {
                recoveryCountdown = new CountdownWrapper(1, false)
            };
            recovery.recoveryCountdown.SetCompleted();

            // Act
            var result = recovery.IsMainIndexRecoveryCompleted();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsMainIndexRecoveryCompleted_Waits_WhenNotCompletedAndWaitFlagIsTrue()
        {
            // Arrange
            var recovery = new IndexRecovery
            {
                recoveryCountdown = new CountdownWrapper(1, false)
            };
            var waitCalled = false;
            recovery.recoveryCountdown = new CountdownWrapper(1, false);
            recovery.recoveryCountdown.Wait = () => { waitCalled = true; };

            // Act
            var result = recovery.IsMainIndexRecoveryCompleted(true);

            // Assert
            Assert.True(waitCalled);
            Assert.True(result);
        }
    }
}
