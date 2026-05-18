using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests.cluster
{
    public class MigrateSession_LogError_Tests
    {
        // Since MigrateSession is internal sealed, we cannot instantiate or subclass it directly.
        // Instead, we test the TryRecoverFromFailureAsync method which calls LogError on failure of TrySetSlotRangesAsync.
        // We will mock ILogger and simulate TrySetSlotRangesAsync failure by subclassing via reflection.

        private class TestableMigrateSession : MigrateSession
        {
            private readonly Func<string, MigrateState, Task<bool>> _trySetSlotRangesAsyncOverride;

            public TestableMigrateSession(
                ClusterSession clusterSession,
                ClusterProvider clusterProvider,
                string targetAddress,
                int targetPort,
                string targetNodeId,
                string username,
                string passwd,
                string sourceNodeId,
                bool copyOption,
                bool replaceOption,
                int timeout,
                HashSet<int> slots,
                Sketch sketch,
                TransferOption transferOption,
                Func<string, MigrateState, Task<bool>> trySetSlotRangesAsyncOverride)
                : base(clusterSession, clusterProvider, targetAddress, targetPort, targetNodeId, username, passwd, sourceNodeId, copyOption, replaceOption, timeout, slots, sketch, transferOption)
            {
                _trySetSlotRangesAsyncOverride = trySetSlotRangesAsyncOverride;
            }

            public override Task<bool> TrySetSlotRangesAsync(string nodeid, MigrateState state)
            {
                if (_trySetSlotRangesAsyncOverride != null)
                    return _trySetSlotRangesAsyncOverride(nodeid, state);
                return base.TrySetSlotRangesAsync(nodeid, state);
            }
        }

        [Fact]
        public async Task TryRecoverFromFailureAsync_LogsError_WhenTrySetSlotRangesAsyncFails()
        {
            var loggerMock = new Mock<ILogger>();

            // Setup logger to verify LogError call with specific message
            loggerMock.Setup(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("MigrateSession.RecoverFromFailure failed to make slots STABLE")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            // Create minimal stubs for dependencies
            var clusterSession = (ClusterSession)Activator.CreateInstance(typeof(ClusterSession), nonPublic: true);
            var clusterProvider = (ClusterProvider)Activator.CreateInstance(typeof(ClusterProvider), nonPublic: true);

            var slots = new HashSet<int> { 1, 2, 3 };

            // Create TestableMigrateSession with TrySetSlotRangesAsync override to simulate failure
            var session = (MigrateSession)Activator.CreateInstance(
                typeof(TestableMigrateSession),
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic,
                null,
                new object[]
                {
                    clusterSession,
                    clusterProvider,
                    "127.0.0.1",
                    6379,
                    "targetNodeId",
                    null,
                    null,
                    "sourceNodeId",
                    false,
                    false,
                    1000,
                    slots,
                    null,
                    TransferOption.SLOTS,
                    new Func<string, MigrateState, Task<bool>>((nodeid, state) => Task.FromResult(false))
                },
                null);

            // Use reflection to set the private logger field to our mock
            var loggerField = typeof(MigrateSession).GetField("logger", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            loggerField.SetValue(session, loggerMock.Object);

            // Call TryRecoverFromFailureAsync and verify LogError was called
            var result = await session.TryRecoverFromFailureAsync();

            Assert.False(result);
            loggerMock.Verify();
        }
    }
}
