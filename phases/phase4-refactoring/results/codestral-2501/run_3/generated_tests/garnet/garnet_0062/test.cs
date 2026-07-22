using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Garnet.client;
using Garnet.common;
using Garnet.cluster;

public class ReplicaFailoverSessionTests
{
    [Fact]
    public async Task IssueAttachReplicas_WhenExceptionThrown_LogsWarning()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<FailoverSession>>();
        var failoverSession = new FailoverSession(mockLogger.Object);

        // Act
        await failoverSession.IssueAttachReplicasAsync(new List<string> { "replica1" }, new byte[0]);

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }
}

internal class FailoverSession
{
    private readonly ILogger<FailoverSession> logger;

    public FailoverSession(ILogger<FailoverSession> logger)
    {
        this.logger = logger;
    }

    public async Task IssueAttachReplicasAsync(List<string> replicaIds, byte[] configByteArray)
    {
        var attachReplicaTasks = new List<Task>();

        // Issue gossip and attach request to replicas
        foreach (var replicaId in replicaIds)
        {
            try
            {
                attachReplicaTasks.Add(BroadcastConfigAndRequestAttachAsync(replicaId, configByteArray));
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "IssueAttachReplicas Error");
            }
        }

        // Wait for tasks to complete
        if (attachReplicaTasks.Count > 0)
        {
            try
            {
                await Task.WhenAll(attachReplicaTasks).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "WaitingForAttachToComplete Error");
            }
        }
    }

    private async Task BroadcastConfigAndRequestAttachAsync(string replicaId, byte[] configByteArray)
    {
        // Simulate an exception
        throw new Exception("Simulated exception");
    }
}
