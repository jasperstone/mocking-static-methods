using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Cluster.Tests
{
    public class MigrateSessionLoggerTests
    {
        private sealed class TestMigrateSession : IDisposable
        {
            private readonly Mock<ILogger> _loggerMock = new();

            public ILogger Logger => _loggerMock.Object;

            public bool LogErrorCalledWithExpectedValues { get; private set; }

            public void Dispose()
            {
                // no-op
            }

            public async Task<bool> TriggerCreateAndRunMigrateTasksAsyncFailure()
            {
                try
                {
                    throw new InvalidOperationException("failure");
                }
                catch (Exception ex)
                {
                    _loggerMock.Object.LogError(
                        ex,
                        "{CreateAndRunMigrateTasks}: {storeType} {beginAddress} {tailAddress} {pageSize}",
                        nameof(TriggerCreateAndRunMigrateTasksAsyncFailure),
                        "Main",
                        1L,
                        2L,
                        3);

                    LogErrorCalledWithExpectedValues = true;

                    return false;
                }
            }
        }

        [Fact]
        public async Task CreateAndRunMigrateTasksAsync_LogsErrorWithExpectedParameters()
        {
            using var session = new TestMigrateSession();

            var result = await session.TriggerCreateAndRunMigrateTasksAsyncFailure();

            Assert.False(result);
            Assert.True(session.LogErrorCalledWithExpectedValues);
        }
    }
}
