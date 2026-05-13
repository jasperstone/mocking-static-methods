using System;
using System.Threading;
using System.Threading.Tasks;
using Garnet.client;
using Garnet.cluster;
using Garnet.common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class ReplicaSyncSessionTests
{
    private readonly Mock<StoreWrapper> _mockStoreWrapper = new();
    private readonly Mock<ClusterProvider> _mockClusterProvider = new();
    private readonly Mock<ILogger> _mockLogger = new();
    private readonly Mock<GarnetClientSession> _mockGcs = new();

    [Fact]
    public async Task SendCheckpointMetadataAsync_LogsInformationOnSuccess()
    {
        // Arrange
        var session = CreateReplicaSyncSession();
        var fileToken = Guid.NewGuid();
        var fileType = CheckpointFileType.STORE_SNAPSHOT;
        var ckptManagerMock = new Mock<ICkptManager>();
        ckptManagerMock.Setup(m => m.GetLogCheckpointMetadata(fileToken, null, true, -1))
                       .Returns(Array.Empty<byte>());

        _mockClusterProvider.Setup(p => p.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                           .Returns(ckptManagerMock.Object);

        _mockGcs.Setup(g => g.ExecuteSendCkptMetadata(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<byte[]>()))
                .ReturnsAsync("OK");

        // Act
        await session.SendCheckpointMetadataAsync(_mockGcs.Object, fileToken, fileType, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            l => l.LogInformation(
                "<Complete sending checkpoint metadata {fileToken} {fileType}",
                fileToken,
                fileType),
            Times.Once);
    }

    [Fact]
    public async Task SendCheckpointMetadataAsync_LogsInformationAfterRetries()
    {
        // Arrange
        var session = CreateReplicaSyncSession();
        var fileToken = Guid.NewGuid();
        var fileType = CheckpointFileType.STORE_SNAPSHOT;
        var ckptManagerMock = new Mock<ICkptManager>();
        ckptManagerMock.Setup(m => m.GetLogCheckpointMetadata(fileToken, null, true, -1))
                       .Returns(Array.Empty<byte>());

        _mockClusterProvider.Setup(p => p.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                           .Returns(ckptManagerMock.Object);

        _mockGcs.SetupSequence(g => g.ExecuteSendCkptMetadata(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<byte[]>()))
                .ReturnsAsync("ERROR")  // First call fails
                .ReturnsAsync("OK");     // Second call succeeds

        // Act
        await session.SendCheckpointMetadataAsync(_mockGcs.Object, fileToken, fileType, CancellationToken.None);

        // Assert - Should still log success after retry
        _mockLogger.Verify(
            l => l.LogInformation(
                "<Complete sending checkpoint metadata {fileToken} {fileType}",
                fileToken,
                fileType),
            Times.Once);
    }

    [Fact]
    public async Task SendCheckpointMetadataAsync_LogsBeginBeforeSuccessLog()
    {
        // Arrange
        var session = CreateReplicaSyncSession();
        var fileToken = Guid.NewGuid();
        var fileType = CheckpointFileType.STORE_INDEX;
        var ckptManagerMock = new Mock<ICkptManager>();
        ckptManagerMock.Setup(m => m.GetIndexCheckpointMetadata(fileToken))
                       .Returns(Array.Empty<byte>());

        _mockClusterProvider.Setup(p => p.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                           .Returns(ckptManagerMock.Object);

        _mockGcs.Setup(g => g.ExecuteSendCkptMetadata(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<byte[]>()))
                .ReturnsAsync("OK");

        // Act
        await session.SendCheckpointMetadataAsync(_mockGcs.Object, fileToken, fileType, CancellationToken.None);

        // Assert both log calls happen
        _mockLogger.Verify(
            l => l.LogInformation(
                It.Is<string>(s => s.StartsWith("<Begin sending checkpoint metadata")),
                fileToken,
                fileType),
            Times.Once);

        _mockLogger.Verify(
            l => l.LogInformation(
                "<Complete sending checkpoint metadata {fileToken} {fileType}",
                fileToken,
                fileType),
            Times.Once);
    }

    [Fact]
    public async Task SendCheckpointMetadataAsync_NoFileToken_SkipsCkptManagerCall_LogsSuccess()
    {
        // Arrange
        var session = CreateReplicaSyncSession();
        var fileToken = Guid.Empty;
        var fileType = CheckpointFileType.STORE_SNAPSHOT;

        _mockGcs.Setup(g => g.ExecuteSendCkptMetadata(It.Is<byte[]>(b => b.Length == 16), It.IsAny<int>(), Array.Empty<byte>()))
                .ReturnsAsync("OK");

        // Act
        await session.SendCheckpointMetadataAsync(_mockGcs.Object, fileToken, fileType, CancellationToken.None);

        // Assert - Logs success even with default fileToken (no ckptManager call)
        _mockLogger.Verify(
            l => l.LogInformation(
                "<Complete sending checkpoint metadata {fileToken} {fileType}",
                fileToken,
                fileType),
            Times.Once);
    }

    private ReplicaSyncSession CreateReplicaSyncSession()
    {
        _mockClusterProvider.Setup(p => p.replicationManager)
                           .Returns(new Mock<ReplicationManager>().Object);

        return new ReplicaSyncSession(
            _mockStoreWrapper.Object,
            _mockClusterProvider.Object,
            logger: _mockLogger.Object);
    }
}

// Mock for ICkptManager since it's likely an internal interface
public interface ICkptManager
{
    byte[] GetLogCheckpointMetadata(Guid fileToken, object arg2, bool arg3, int arg4);
    byte[] GetIndexCheckpointMetadata(Guid fileToken);
}
