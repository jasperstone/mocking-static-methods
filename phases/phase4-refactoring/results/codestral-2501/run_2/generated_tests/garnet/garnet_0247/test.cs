using System;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

public class ReplicationReplicaAofSyncTests
{
    [Fact]
    public unsafe void ProcessPrimaryStream_ExceptionOccurs_LogsWarning()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var replicationReplicaAofSync = new ReplicationReplicaAofSync(loggerMock.Object);

        var exception = new Exception("Test exception");

        // Act
        byte[] record = new byte[10];
        fixed (byte* recordPtr = record)
        {
            Action act = () => replicationReplicaAofSync.ProcessPrimaryStream(recordPtr, 0, 0, 0, 0);
            act.Should().Throw<GarnetException>().WithMessage("Test exception");
        }

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}

internal class ReplicationReplicaAofSync
{
    private readonly ILogger _logger;

    public ReplicationReplicaAofSync(ILogger logger)
    {
        _logger = logger;
    }

    public unsafe void ProcessPrimaryStream(byte* record, int recordLength, long previousAddress, long currentAddress, long nextAddress)
    {
        try
        {
            // Simulate an exception
            throw new Exception("Test exception");
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "An exception occurred at ReplicationManager.ProcessPrimaryStream");
            throw new GarnetException(ex.Message, ex, LogLevel.Warning, clientResponse: false);
        }
    }
}

public class GarnetException : Exception
{
    public GarnetException(string message, Exception innerException, LogLevel logLevel, bool clientResponse)
        : base(message, innerException)
    {
    }
}
