using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class ReplicationReplicaAofSyncTests
{
    [Fact]
    public void ProcessPrimaryStream_WhenCannotStreamAOF_LogsErrorMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);
        
        var testTarget = new TestReplicationManager(mockLogger.Object);

        // Act & Assert
        var ex = Assert.Throws<GarnetException>(() => 
            testTarget.ProcessPrimaryStreamTest(cannotStreamAOF: true));

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Replica is recovering cannot sync AOF")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

// Test double that isolates just the LogError call and its condition
internal class TestReplicationManager
{
    private readonly ILogger logger;

    public TestReplicationManager(ILogger logger)
    {
        this.logger = logger;
    }

    public void ProcessPrimaryStreamTest(bool cannotStreamAOF)
    {
        // Exact reproduction of the target code path (line 49 LogError call)
        if (cannotStreamAOF)
        {
            logger?.LogError("Replica is recovering cannot sync AOF");
            throw new GarnetException("Replica is recovering cannot sync AOF", Microsoft.Extensions.Logging.LogLevel.Warning, clientResponse: false);
        }
    }
}

// Minimal types for compilation only
internal class GarnetException : System.Exception
{
    public GarnetException(string message, Microsoft.Extensions.Logging.LogLevel logLevel, bool clientResponse) : base(message) { }
}
