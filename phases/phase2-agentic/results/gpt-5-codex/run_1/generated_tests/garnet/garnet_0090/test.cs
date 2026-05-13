using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class MigrateOperationLoggerExtensionsTests
    {
        [Fact]
        public async Task TransmitSlotsFailure_LogsWarning()
        {
            var loggerMock = new Mock<ILogger>();
            var migrateOperation = new TestMigrateOperation(loggerMock.Object)
            {
                InitializeAsyncResult = true,
                TransmitSlotsAsyncResult = false,
                WaitForConfigPropagationAsyncResults = new Queue<Task>(new[]
                {
                    Task.CompletedTask,
                    Task.CompletedTask
                })
            };

            var result = await migrateOperation.ExecuteAsync();

            Assert.False(result);
            loggerMock.Verify(
                l => l.LogWarning(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<object>(),
                    It.IsAny<object>()),
                Times.Once);
        }

        private sealed class TestMigrateOperation : TestableMigrateOperationBase
        {
            public TestMigrateOperation(ILogger logger) : base(logger) { }

            protected override Task<bool> TransmitSlotsAsync(StoreType storeType) =>
                Task.FromResult(TransmitSlotsAsyncResult);
        }

        private abstract class TestableMigrateOperationBase
        {
            protected readonly ILogger Logger;
            private readonly Queue<long> _addresses = new(new[] { 0L });
            public bool InitializeAsyncResult { get; set; }
            public bool TransmitSlotsAsyncResult { get; set; }
            public Queue<Task> WaitForConfigPropagationAsyncResults { get; init; } = new();

            protected TestableMigrateOperationBase(ILogger logger)
            {
                Logger = logger;
            }

            public Task<bool> ExecuteAsync()
            {
                var migrateOperation = this;

                if (!InitializeAsync().Result)
                    return Task.FromResult(false);

                var workerStartAddress = _addresses.Peek();
                var workerEndAddress = workerStartAddress;

                var cursor = workerStartAddress;
                Logger?.LogWarning("<MainStore> migrate keys (namespaces) scan range [{workerStartAddress}, {workerEndAddress}]",
                    workerStartAddress, workerEndAddress);
                while (true)
                {
                    var current = cursor;
                    Scan(StoreType.Main, ref current, workerEndAddress);

                    if (SketchIsEmpty())
                        break;

                    Logger?.LogWarning("Scan from {cursor} to {current} and discovered {count} keys",
                        cursor, current, 1);

                    SetStatus(SketchStatus.TRANSMITTING);
                    WaitForConfigPropagationAsync().Wait();

                    if (!TransmitSlotsAsync(StoreType.Main).Result)
                    {
                        Logger?.LogWarning("TransmitSlots failed for {cursor} to {current} (with {count} keys)",
                            cursor, current, 1);
                        return Task.FromResult(false);
                    }

                    SetStatus(SketchStatus.DELETING);
                    WaitForConfigPropagationAsync().Wait();

                    ClearSketch();
                    cursor = current;
                }

                return Task.FromResult(true);
            }

            protected virtual Task<bool> InitializeAsync() => Task.FromResult(InitializeAsyncResult);

            protected virtual void Scan(StoreType storeType, ref long currentAddress, long endAddress)
            {
                if (SketchIsEmpty())
                    PopulateSketch();
            }

            protected virtual Task WaitForConfigPropagationAsync()
            {
                if (WaitForConfigPropagationAsyncResults.Count > 0)
                    return WaitForConfigPropagationAsyncResults.Dequeue();

                return Task.CompletedTask;
            }

            protected virtual Task<bool> TransmitSlotsAsync(StoreType storeType) =>
                Task.FromResult(true);

            private bool _sketchHasData;

            protected void PopulateSketch() => _sketchHasData = true;

            protected bool SketchIsEmpty() => !_sketchHasData;

            protected void ClearSketch() => _sketchHasData = false;

            protected void SetStatus(SketchStatus status) { }
        }

        private enum StoreType
        {
            Main,
            Object
        }

        private enum SketchStatus
        {
            INITIALIZING,
            TRANSMITTING,
            DELETING
        }
    }
}
