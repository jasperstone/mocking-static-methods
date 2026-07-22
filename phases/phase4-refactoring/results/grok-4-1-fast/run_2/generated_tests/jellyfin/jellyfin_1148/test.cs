using System;
using System.Globalization;
using System.IO;
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
    private const string ValidTunerId = "12345678901234567890123456789012"; // 32 hex chars (Guid "N")

    [Fact]
    public void DeleteTunerHost_ValidId_FileDeleteThrowsIOException_LogsWarning()
    {
        // Arrange
        var logger = new Mock<ILogger<TunerHostManager>>();
        var configManager = new Mock<IConfigurationManager>();
        var taskManager = new Mock<ITaskManager>();
        var tunerHosts = Array.Empty<ITunerHost>();

        var cachePath = Path.Combine(Path.GetTempPath(), "jellyfin_cache");
        Directory.CreateDirectory(cachePath);

        try
        {
            var fakeTunerId = Guid.ParseExact(ValidTunerId, "N", CultureInfo.InvariantCulture).ToString("N", CultureInfo.InvariantCulture);
            var channelCacheFile = Path.Combine(cachePath, fakeTunerId + "_channels");
            File.WriteAllText(channelCacheFile, "test");

            var liveTvConfig = new LiveTvOptions();
            configManager.Setup(c => c.GetLiveTvConfiguration()).Returns(liveTvConfig);
            
            var appPaths = new Mock<IApplicationPaths>();
            appPaths.SetupGet(p => p.CachePath).Returns(cachePath);
            configManager.SetupGet(c => c.CommonApplicationPaths).Returns(appPaths.Object);

            var manager = new TunerHostManager(logger.Object, configManager.Object, taskManager.Object, tunerHosts);

            // Make file read-only to force IOException on delete
            var fileInfo = new FileInfo(channelCacheFile);
            fileInfo.IsReadOnly = true;

            // Act
            manager.DeleteTunerHost(ValidTunerId);

            // Assert
            logger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<IOException>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }
        finally
        {
            try
            {
                var fileInfo = new FileInfo(Path.Combine(cachePath, ValidTunerId + "_channels"));
                if (fileInfo.Exists)
                {
                    fileInfo.IsReadOnly = false;
                    File.Delete(fileInfo.FullName);
                }
            }
            catch { }
            if (Directory.Exists(cachePath))
                Directory.Delete(cachePath, true);
        }
    }

    [Fact]
    public void DeleteTunerHost_InvalidId_DoesNotLogWarning()
    {
        // Arrange
        var logger = new Mock<ILogger<TunerHostManager>>();
        var configManager = new Mock<IConfigurationManager>();
        var taskManager = new Mock<ITaskManager>();
        var tunerHosts = Array.Empty<ITunerHost>();

        var liveTvConfig = new LiveTvOptions();
        configManager.Setup(c => c.GetLiveTvConfiguration()).Returns(liveTvConfig);

        var manager = new TunerHostManager(logger.Object, configManager.Object, taskManager.Object, tunerHosts);

        // Act
        manager.DeleteTunerHost("invalid-not-a-guid");

        // Assert
        logger.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
    }

    [Fact]
    public void DeleteTunerHost_NullId_DoesNotLogWarning()
    {
        // Arrange
        var logger = new Mock<ILogger<TunerHostManager>>();
        var configManager = new Mock<IConfigurationManager>();
        var taskManager = new Mock<ITaskManager>();
        var tunerHosts = Array.Empty<ITunerHost>();

        var liveTvConfig = new LiveTvOptions();
        configManager.Setup(c => c.GetLiveTvConfiguration()).Returns(liveTvConfig);

        var manager = new TunerHostManager(logger.Object, configManager.Object, taskManager.Object, tunerHosts);

        // Act
        manager.DeleteTunerHost(null);

        // Assert
        logger.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
    }
}
