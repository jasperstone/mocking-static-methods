using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tsavorite.core;
using Xunit;

namespace Tsavorite.Tests
{
    public class RecoveryTests
    {
        private class TestLogger : ILogger
        {
            public System.Collections.Generic.List<string> LoggedMessages { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (logLevel == LogLevel.Information)
                {
                    LoggedMessages.Add(formatter(state, exception));
                }
            }
        }

        [Fact]
        public async Task InternalRecoverAsync_LogsInformation_WhenTailAddressGreaterThanFirstValidLogicalAddress()
        {
            // Arrange
            var logger = new TestLogger();

            // Create instance of TsavoriteKV with logger set via reflection
            var kvType = typeof(TsavoriteKV<,,,>);
            var genericKvType = kvType.MakeGenericType(typeof(int), typeof(int), typeof(object), typeof(object));
            var kvInstance = Activator.CreateInstance(genericKvType, nonPublic: true);

            // Set logger field via reflection
            var loggerField = genericKvType.GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(kvInstance, logger);

            // Prepare parameters for InternalRecoverAsync
            var method = genericKvType.GetMethod("InternalRecoverAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Create dummy parameters for method call
            var indexCheckpointInfoType = kvType.Assembly.GetType("Tsavorite.core.IndexCheckpointInfo");
            var hybridLogCheckpointInfoType = kvType.Assembly.GetType("Tsavorite.core.HybridLogCheckpointInfo");

            var recoveredICInfo = Activator.CreateInstance(indexCheckpointInfoType);
            var recoveredHLCInfo = Activator.CreateInstance(hybridLogCheckpointInfoType);

            int numPagesToPreload = 0;
            bool undoNextVersion = false;
            long recoverTo = 0;
            var cancellationToken = CancellationToken.None;

            // Act
            var task = (ValueTask<long>)method.Invoke(kvInstance, new object[] { recoveredICInfo, recoveredHLCInfo, numPagesToPreload, undoNextVersion, recoverTo, cancellationToken });
            await task.AsTask();

            // Assert
            Assert.Contains("Recovery called on non-empty log - resetting to empty state first. Make sure store is quiesced before calling Recover on a running store.", logger.LoggedMessages);
        }
    }
}
