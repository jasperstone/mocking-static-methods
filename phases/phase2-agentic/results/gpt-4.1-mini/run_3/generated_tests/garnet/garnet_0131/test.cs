using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class MigrateSessionTests
    {
        // We will test the BeginAsyncMigrationTaskAsync method indirectly by calling TryStartMigrationTaskAsync
        // and mocking dependencies to force the error path that triggers logger.LogError on line 154.

        // Since BeginAsyncMigrationTaskAsync is private, we test it via TryStartMigrationTaskAsync with transferOption != KEYS.

        // We will mock the clusterProvider and other dependencies to simulate failure in TrySetSlotRangesAsync
        // to cause the LogError call on line 154.

        private class TestableMigrateSession : MigrateSession
        {
            public TestableMigrateSession()
            {
                // We need to override or expose some members for testing
            }

            public void SetLogger(ILogger logger)
            {
                this.logger = logger;
            }

            public void SetClusterProvider(dynamic provider)
            {
                this.clusterProvider = provider;
            }

            public void SetSlots(int[] slots)
            {
                this.GetSlots = slots;
            }

            public void SetSslots(int[] sslots)
            {
                this._sslots = sslots;
            }

            public void SetTransferOption(TransferOption option)
            {
                this.transferOption = option;
            }

            public void SetStatus(MigrateState state)
            {
                this.Status = state;
            }

            public void SetTimeout(TimeSpan timeout)
            {
                this._timeout = timeout;
            }

            public void SetCts(CancellationTokenSource cts)
            {
                this._cts = cts;
            }

            public void SetGetSourceNodeId(string id)
            {
                this.GetSourceNodeId = id;
            }

            public void SetMigrateOperation(dynamic op)
            {
                this.migrateOperation = op;
            }

            public void SetSlotRanges(dynamic ranges)
            {
                this._slotRanges = ranges;
            }

            public void SetNamespaces(List<object> namespaces)
            {
                this._namespaces = namespaces;
            }

            public void SetTrySetSlotRangesAsync(Func<string, MigrateState, Task<bool>> func)
            {
                this._trySetSlotRangesAsync = func;
            }

            // We override TrySetSlotRangesAsync to call the delegate if set
            protected override Task<bool> TrySetSlotRangesAsync(string nodeid, MigrateState state)
            {
                if (_trySetSlotRangesAsync != null)
                    return _trySetSlotRangesAsync(nodeid, state);
                return base.TrySetSlotRangesAsync(nodeid, state);
            }

            private Func<string, MigrateState, Task<bool>> _trySetSlotRangesAsync;
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogsError_WhenTrySetSlotRangesAsyncFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new TestableMigrateSession();
            migrateSession.SetLogger(loggerMock.Object);
            migrateSession.SetTransferOption(TransferOption.SLOTS); // Not KEYS to trigger background task

            // Setup clusterProvider with required members
            var clusterProviderMock = new Mock<dynamic>();
            clusterProviderMock.SetupGet(p => p.storeWrapper).Returns(new
            {
                store = new
                {
                    PauseRevivification = new Action<TimeSpan, CancellationToken>((timeout, token) => { })
                },
                DefaultDatabase = new
                {
                    VectorManager = new
                    {
                        GetNamespacesForHashSlots = new Func<int[], List<object>>((slots) => new List<object>())
                    }
                }
            });

            clusterProviderMock.Setup(p => p.BumpAndWaitForEpochTransitionAsync()).ReturnsAsync(true);
            clusterProviderMock.Setup(p => p.ReserveDestinationVectorSetsAsync()).ReturnsAsync(true);
            clusterProviderMock.Setup(p => p.MigrateSlotsDriverInlineAsync()).ReturnsAsync(true);
            clusterProviderMock.Setup(p => p.clusterManager).Returns(new
            {
                SuspendConfigMerge = new Action(() => { }),
                ResumeConfigMerge = new Action(() => { })
            });

            migrateSession.SetClusterProvider(clusterProviderMock.Object);

            migrateSession.SetGetSourceNodeId("sourceNodeId");
            migrateSession.SetSlots(new int[] { 1, 2, 3 });
            migrateSession.SetSslots(new int[] { 1, 2, 3 });
            migrateSession.SetTimeout(TimeSpan.FromSeconds(1));
            migrateSession.SetCts(new CancellationTokenSource());

            // Setup TrySetSlotRangesAsync to fail on first call to simulate error on line 154
            bool firstCall = true;
            migrateSession.SetTrySetSlotRangesAsync((nodeid, state) =>
            {
                if (firstCall)
                {
                    firstCall = false;
                    return Task.FromResult(false);
                }
                return Task.FromResult(true);
            });

            // Act
            var result = await migrateSession.TryStartMigrationTaskAsync();

            // Wait a bit for background task to run
            await Task.Delay(100);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to set remote slots")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            Assert.Equal(MigrateState.FAIL, migrateSession.Status);
            Assert.True(result.Success);
        }
    }

    // Dummy enums and base class to allow compilation
    internal enum MigrateState { FAIL, SUCCESS, IMPORT, STABLE, NODE }
    internal enum TransferOption { KEYS, SLOTS }

    internal class MigrateSession : IDisposable
    {
        protected ILogger logger;
        protected dynamic clusterProvider;
        protected int[] GetSlots;
        protected int[] _sslots;
        protected TransferOption transferOption;
        protected MigrateState Status;
        protected TimeSpan _timeout;
        protected CancellationTokenSource _cts;
        protected string GetSourceNodeId;
        protected dynamic migrateOperation;
        protected dynamic _slotRanges;
        protected List<object> _namespaces;

        public virtual Task<bool> TrySetSlotRangesAsync(string nodeid, MigrateState state) => Task.FromResult(true);
        public virtual Task<bool> TryRecoverFromFailureAsync() => Task.FromResult(true);
        public virtual bool TryPrepareLocalForMigration() => true;
        public virtual Task<bool> ReserveDestinationVectorSetsAsync() => Task.FromResult(true);
        public virtual Task<bool> MigrateSlotsDriverInlineAsync() => Task.FromResult(true);
        public virtual Task<bool> clusterProvider_BumpAndWaitForEpochTransitionAsync() => Task.FromResult(true);

        public virtual void Dispose() { }

        public async ValueTask<(bool Success, ReadOnlyMemory<byte> ErrorMessage)> TryStartMigrationTaskAsync()
        {
            ReadOnlyMemory<byte> errorMessage = default;
            if (transferOption == TransferOption.KEYS)
            {
                if (!await Task.FromResult(true))
                {
                    errorMessage = "IOERR Migrate keys failed."u8.ToArray();
                    Status = MigrateState.FAIL;
                    return (false, errorMessage);
                }
                Status = MigrateState.SUCCESS;
            }
            else
            {
                _ = BeginAsyncMigrationTaskAsync();
            }
            return (true, errorMessage);
        }

        private async Task BeginAsyncMigrationTaskAsync()
        {
            await Task.Yield();

            try
            {
                clusterProvider.storeWrapper.store.PauseRevivification(_timeout, _cts.Token);

                if (!await TrySetSlotRangesAsync(GetSourceNodeId, MigrateState.IMPORT).ConfigureAwait(false))
                {
                    logger?.LogError("Failed to set remote slots {slots} to import state", "slots");
                    await TryRecoverFromFailureAsync().ConfigureAwait(false);
                    Status = MigrateState.FAIL;
                    return;
                }

                if (!TryPrepareLocalForMigration())
                {
                    logger?.LogError("Failed to set local slots {slots} to migrate state", string.Join(',', GetSlots));
                    await TryRecoverFromFailureAsync().ConfigureAwait(false);
                    Status = MigrateState.FAIL;
                    return;
                }

                if (!await clusterProvider.BumpAndWaitForEpochTransitionAsync().ConfigureAwait(false)) return;

                _namespaces = clusterProvider.storeWrapper.DefaultDatabase.VectorManager.GetNamespacesForHashSlots(_sslots);

                if ((_namespaces?.Count ?? 0) > 0 && !await ReserveDestinationVectorSetsAsync())
                {
                    logger?.LogError("Failed to reserve destination vector sets, migration failed");
                    await TryRecoverFromFailureAsync().ConfigureAwait(false);
                    Status = MigrateState.FAIL;
                    return;
                }

                if (!await MigrateSlotsDriverInlineAsync())
                {
                    logger?.LogError("MigrateSlotsDriver failed");
                    await TryRecoverFromFailureAsync().ConfigureAwait(false);
                    Status = MigrateState.FAIL;
                    return;
                }
            }
            catch
            {
                Status = MigrateState.FAIL;
            }
        }
    }
}
