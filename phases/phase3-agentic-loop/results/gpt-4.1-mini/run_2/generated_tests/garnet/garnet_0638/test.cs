using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Tsavorite.core;
using Xunit;

namespace Tsavorite.Tests
{
    public class TsavoriteBaseTests
    {
        private class TestLogger : ILogger
        {
            public string LastLogMessage { get; private set; }
            public LogLevel? LastLogLevel { get; private set; }

            public IDisposable BeginScope<TState>(TState state) => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                LastLogLevel = logLevel;
                LastLogMessage = formatter(state, exception);
            }
        }

        private class DummyDevice : IDevice
        {
            public uint SectorSize => 4096;
            public string FileName => "dummy";
            public long Capacity => 1024 * 1024;
            public long SegmentSize => 4096;
            public int StartSegment => 0;
            public int EndSegment => 0;
            public int ThrottleLimit { get; set; }

            public void Dispose() { }

            public void Initialize(long segmentSize, LightEpoch epoch = null, bool omitSegmentIdFromFilename = false) { }

            public bool TryComplete() => true;

            public bool Throttle() => false;

            public void WriteAsync(IntPtr sourceAddress, int segmentId, ulong destinationAddress, uint numBytesToWrite, DeviceIOCompletionCallback callback, object context)
            {
                callback(0, numBytesToWrite, context);
            }

            public void ReadAsync(int segmentId, ulong sourceAddress, IntPtr destinationAddress, uint readLength, DeviceIOCompletionCallback callback, object context)
            {
                callback(1, readLength, context); // simulate error to trigger LogError
            }

            public void WriteAsync(IntPtr alignedSourceAddress, ulong alignedDestinationAddress, uint numBytesToWrite, DeviceIOCompletionCallback callback, object context)
            {
                callback(0, numBytesToWrite, context);
            }

            public void ReadAsync(ulong alignedSourceAddress, IntPtr alignedDestinationAddress, uint aligned_read_length, DeviceIOCompletionCallback callback, object context)
            {
                callback(1, aligned_read_length, context); // simulate error to trigger LogError
            }

            public void TruncateUntilAddressAsync(long toAddress, AsyncCallback callback, IAsyncResult result) { }

            public void TruncateUntilAddress(long toAddress) { }

            public void TruncateUntilSegmentAsync(int toSegment, AsyncCallback callback, IAsyncResult result) { }

            public void TruncateUntilSegment(int toSegment) { }

            public void RemoveSegmentAsync(int segment, AsyncCallback callback, IAsyncResult result) { }

            public void RemoveSegment(int segment) { }
        }

        [Fact]
        public void AsyncPageReadCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var tsavorite = new TsavoriteBase();
            var logger = new TestLogger();
            var loggerField = typeof(TsavoriteBase).GetField("logger", BindingFlags.NonPublic | BindingFlags.Instance);
            loggerField.SetValue(tsavorite, logger);

            var countdownType = typeof(TsavoriteBase).Assembly.GetType("Tsavorite.core.CountdownWrapper");
            var countdownCtor = countdownType.GetConstructor(new Type[] { typeof(int), typeof(bool) });
            var countdown = countdownCtor.Invoke(new object[] { 1, false });
            var recoveryCountdownField = typeof(TsavoriteBase).GetField("recoveryCountdown", BindingFlags.NonPublic | BindingFlags.Instance);
            recoveryCountdownField.SetValue(tsavorite, countdown);

            // Act
            var method = typeof(TsavoriteBase).GetMethod("AsyncPageReadCallback", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(tsavorite, new object[] { (uint)1, (uint)1234, new object() });

            // Assert
            Assert.NotNull(logger.LastLogMessage);
            Assert.Contains("AsyncPageReadCallback error", logger.LastLogMessage);
            Assert.Equal(LogLevel.Error, logger.LastLogLevel);
        }
    }
}
