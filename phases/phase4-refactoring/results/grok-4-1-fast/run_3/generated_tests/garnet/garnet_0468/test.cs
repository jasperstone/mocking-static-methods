using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.server;

public class VectorManagerTests
{
    [Fact]
    public void VectorManager_Constructor_CreatesLoggerWithCorrectNameFormat()
    {
        // Arrange
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(NullLogger<VectorManager>.Instance);
        
        // Act
        _ = new VectorManager(0, new GarnetServerOptions { EnableVectorSetPreview = true }, 
                             () => NullMessageConsumer.Instance, loggerFactoryMock.Object);
        
        // Assert - verifies logger creation with expected name pattern
        loggerFactoryMock.Verify(
            f => f.CreateLogger(It.Is<string>(name => name.StartsWith("VectorManager:") && name.Contains(":00000000-"))), 
            Times.Once);
    }
    
    [Fact]
    public void VectorManager_Constructor_LogsCreationMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<VectorManager>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
        
        // Act
        _ = new VectorManager(0, new GarnetServerOptions { EnableVectorSetPreview = true }, 
                             () => NullMessageConsumer.Instance, loggerFactoryMock.Object);
        
        // Assert - constructor logs "Created VectorManager" message
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Created VectorManager")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }
    
    [Fact]
    public void ResumePostRecovery_LogsInformation_ForInProgressDeleteCleanup()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<VectorManager>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
        
        var vectorManager = new VectorManager(0, new GarnetServerOptions { EnableVectorSetPreview = true }, 
                                            () => NullMessageConsumer.Instance, loggerFactoryMock.Object);
        
        // Act
        vectorManager.ResumePostRecovery();
        
        // Assert - ResumePostRecovery unconditionally logs cleanup info when failedDeletes has entries
        // This tests the logger?.LogInformation call at the start of the failed deletes loop (line ~200)
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cleaning up in progress Vector Set delete")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.AtLeastOnce
        );
    }
}

// Minimal IMessageConsumer implementation for constructor
public sealed class NullMessageConsumer : Garnet.networking.IMessageConsumer, IDisposable
{
    public static readonly NullMessageConsumer Instance = new();
    
    private NullMessageConsumer() { }
    
    public void Consume(Garnet.networking.Connection connection, System.ReadOnlyMemory<byte> data) { }
    public bool TryConsumeMessages(byte* data, int length) => true;
    public void Dispose() { }
}
