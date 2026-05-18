using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Garnet.client;
using Garnet.common;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

public class ReplicaFailoverSessionTests
{
    [Fact]
    public async Task IssueAttachReplicas_WhenTasksThrowException_LogsWarning()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<FailoverSession>>();
        var failoverSession = new FailoverSession(mockLogger.Object);

        var replicaIds = new List<string> { "replica1", "replica2" };
        var configByteArray = new byte[] { };

        // Mock the BroadcastConfigAndRequestAttachAsync method to throw an exception
        failoverSession.BroadcastConfigAndRequestAttachAsync = (replicaId, config) =>
        {
            throw new Exception("Test exception");
        };

        // Act
        await failoverSession.IssueAttachReplicas(replicaIds, configByteArray);

        // Assert
        mockLogger.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Exactly(replicaIds.Count));
    }
}
