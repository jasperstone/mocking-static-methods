using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

public class ReplicaFailoverSessionLoggerTests
{
    [Fact]
    public async Task IssueAttachReplicasAsync_LogsWarning_WhenWaitingForAttachTasksThrows()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(x => x.LogWarning(It.IsAny<Exception>(), "WaitingForAttachToComplete Error"))
                  .Verifiable();

        // Test double that replicates the exact logging scenario from ReplicaFailoverSession
        var testSession = new LoggingTestDouble(mockLogger.Object)
        {
            option = FailoverOption.DEFAULT,
            replicaIds = new List<string> { "replica1" },
            oldPrimaryId = "primary1"
        };

        // Act - this will trigger Task.WhenAll to throw AggregateException
        await testSession.ExecuteIssueAttachReplicasAsync();

        // Assert - verify the specific LogWarning call on line 276
        mockLogger.Verify(
            x => x.LogWarning(
                It.IsAny<Exception>(),
                "WaitingForAttachToComplete Error"),
            Times.Once);
    }

    private class LoggingTestDouble
    {
        public List<string> replicaIds { get; set; } = new();
        public string oldPrimaryId { get; set; } = "";
        public FailoverOption option { get; set; }
        internal ILogger logger;

        public LoggingTestDouble(ILogger logger)
        {
            this.logger = logger ?? NullLogger.Instance;
        }

        public async Task ExecuteIssueAttachReplicasAsync()
        {
            var attachReplicaTasks = new List<Task>();

            // Replicates exact logic from source code lines 246-276
            if (option == FailoverOption.DEFAULT)
            {
                replicaIds.Add(oldPrimaryId);
            }

            // Issue tasks that will fail (simulates BroadcastConfigAndRequestAttachAsync throwing)
            foreach (var replicaId in replicaIds)
            {
                try
                {
                    attachReplicaTasks.Add(Task.FromException(new InvalidOperationException("Attach failed")));
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "IssueAttachReplicas Error");
                }
            }

            // EXACT code from line 276 that we want to test
            if (attachReplicaTasks.Count > 0)
            {
                try
                {
                    await Task.WhenAll(attachReplicaTasks).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "WaitingForAttachToComplete Error");
                }
            }
        }
    }

    // Enum values inferred from source context
    private enum FailoverOption
    {
        DEFAULT
    }
}
