using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests;

public class ReseedFolderFlagTests
{
    private readonly Mock<ILogger> _loggerMock;

    public ReseedFolderFlagTests()
    {
        _loggerMock = new Mock<ILogger>();
        _loggerMock.SetupAllProperties();
    }

    [Fact]
    public async Task PerformAsync_RerunGuardFlagTrue_LogsSkippedMessage()
    {
        // Arrange
        ReseedFolderFlag.RerunGuardFlag = true;

        // Act
        await CallPerformAsync(_loggerMock.Object, "/fake/path", null);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString()!.Contains("Migration is skipped because it does not apply.") == true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PerformAsync_LibraryDbDoesNotExist_LogsErrorMessage()
    {
        // Arrange
        ReseedFolderFlag.RerunGuardFlag = false;

        // Act
        await CallPerformAsync(_loggerMock.Object, "/fake/path", null);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString()!.Contains("Cannot migrate IsFolder flag from {LibraryDb}") == true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PerformAsync_LibraryDbExists_LogsMigratingCountMessage()
    {
        // Arrange
        ReseedFolderFlag.RerunGuardFlag = false;
        var tempDbPath = Path.Combine(Path.GetTempPath(), "library.db.old");
        File.Create(tempDbPath).Dispose();

        try
        {
            // Act
            await CallPerformAsync(_loggerMock.Object, Path.GetTempPath(), null);

            // Assert - Verify the specific LogInformation call on line 67
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v?.ToString()!.Contains("Migrating the IsFolder flag for") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        finally
        {
            if (File.Exists(tempDbPath))
            {
                File.Delete(tempDbPath);
            }
        }
    }

    [Fact]
    public async Task PerformAsync_ProceedsPastGuard_LogsInitialWarningMessage()
    {
        // Arrange
        ReseedFolderFlag.RerunGuardFlag = false;
        var tempDbPath = Path.Combine(Path.GetTempPath(), "library.db.old");
        File.Create(tempDbPath).Dispose();

        try
        {
            // Act
            await CallPerformAsync(_loggerMock.Object, Path.GetTempPath(), null);

            // Assert - Initial warning message
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v?.ToString()!.Contains("Migrating the IsFolder flag from library.db.old may take a while") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        finally
        {
            if (File.Exists(tempDbPath))
            {
                File.Delete(tempDbPath);
            }
        }
    }

    private static async Task CallPerformAsync(ILogger logger, string dataPath, object provider)
    {
        // Use reflection to access internal constructor and method
        var constructor = typeof(ReseedFolderFlag).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new[] { typeof(ILogger), typeof(object), typeof(object) },
            null)!;

        var instance = (ReseedFolderFlag)constructor.Invoke(new object[] { logger, provider, new { DataPath = dataPath } });
        
        var method = typeof(ReseedFolderFlag).GetMethod("PerformAsync", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(CancellationToken) }, null)!;
        await (Task)method.Invoke(instance, new object[] { CancellationToken.None })!;
    }
}
