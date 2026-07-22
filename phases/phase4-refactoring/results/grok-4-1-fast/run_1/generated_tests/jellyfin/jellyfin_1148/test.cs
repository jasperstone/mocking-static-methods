using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Jellyfin.LiveTv.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.LiveTv;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Jellyfin.LiveTv.TunerHosts.Tests;

public class TunerHostManagerTests
{
    private readonly Mock<ILogger<TunerHostManager>> _loggerMock;
    private readonly Mock<IConfigurationManager> _configMock;
    private readonly Mock<ITaskManager> _taskManagerMock;
    private readonly TunerHostManager _tunerHostManager;

    public TunerHostManagerTests()
    {
        _loggerMock = new Mock<ILogger<TunerHostManager>>();
        _configMock = new Mock<IConfigurationManager>();
        _taskManagerMock = new Mock<ITaskManager>();

        // Setup minimal config that doesn't require unavailable types
        _configMock.Setup(c => c.GetLiveTvConfiguration()).Returns(new LiveTvConfiguration());
        
        var appPathsMock = new Mock<ICommonApplicationPaths>();
        appPathsMock.Setup(p => p.CachePath).Returns(GetTestCachePath());
        _configMock.Setup(c => c.CommonApplicationPaths).Returns(appPathsMock.Object);

        var tunerHosts = Array.Empty<ITunerHost>();
        _tunerHostManager = new TunerHostManager(_loggerMock.Object, _configMock.Object, _taskManagerMock.Object, tunerHosts);
    }

    private static string GetTestCachePath()
    {
        var path = Path.Combine(Path.GetTempPath(), "jellyfin_test_cache", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    [Fact]
    public void DeleteTunerHost_ValidGuid_FileDeleteThrowsIOException_LogsWarning()
    {
        // Arrange
        var tunerId = Guid.NewGuid().ToString("N");
        var cachePath = GetTestCachePath();
        var channelCacheFile = Path.Combine(cachePath, tunerId + "_channels");

        // Create read-only file to cause IOException on delete
        File.WriteAllText(channelCacheFile, "test");
        new FileInfo(channelCacheFile).IsReadOnly = true;

        try
        {
            // Act
            _tunerHostManager.DeleteTunerHost(tunerId);

            // Assert - verify LogWarning was called with correct message and tunerId
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Error deleting channel cache file for tuner {tunerId}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        finally
        {
            // Cleanup
            CleanupTestFiles(cachePath);
        }
    }

    [Fact]
    public void DeleteTunerHost_InvalidGuid_DoesNotLogWarning()
    {
        // Arrange
        var invalidId = "not-a-guid";

        // Act
        _tunerHostManager.DeleteTunerHost(invalidId);

        // Assert - no warning logged since Guid parsing fails before File.Delete
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    private static void CleanupTestFiles(string cachePath)
    {
        try
        {
            if (Directory.Exists(cachePath))
            {
                Directory.Delete(cachePath, true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
