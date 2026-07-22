using System;
using System.IO;
using System.Linq;
using Moq;
using Moq.Language.Flow;
using Xunit;
using Microsoft.Extensions.Logging;
using MediaBrowser.Model.Tasks;

namespace Jellyfin.LiveTv.TunerHosts.Tests;

public class TunerHostManagerTests
{
    private const string ValidTunerId = "1234567890abcdef1234567890abcdef12345678";
    private const string InvalidTunerId = "invalid";

    [Fact]
    public void DeleteTunerHost_ValidGuid_FileDeleteThrowsIOException_LogsWarning()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TunerHostManager>>();
        var mockConfig = new Mock<global::MediaBrowser.Common.Configuration.IConfigurationManager>();
        var mockTaskManager = new Mock<ITaskManager>();
        
        // Setup config to return empty tuner hosts list and mock SaveConfiguration
        var emptyConfig = new global::Jellyfin.LiveTv.Configuration.LiveTvConfiguration 
        { 
            TunerHosts = Array.Empty<global::MediaBrowser.Model.LiveTv.TunerHostInfo>() 
        };
        mockConfig.Setup(c => c.GetLiveTvConfiguration()).Returns(emptyConfig);
        mockConfig.Setup(c => c.SaveConfiguration(It.IsAny<string>(), It.IsAny<global::Jellyfin.LiveTv.Configuration.LiveTvConfiguration>()));

        // Create manager with empty tuner hosts
        var manager = new TunerHostManager(
            mockLogger.Object,
            mockConfig.Object,
            mockTaskManager.Object,
            Array.Empty<global::MediaBrowser.Controller.LiveTv.ITunerHost>());

        // Mock static File.Delete to throw IOException using Moq static mocking (requires Moq 4.20+)
        Mock.Get(File.Delete).Setup(f => f(It.IsAny<string>())).Throws(new IOException("Delete failed"));

        // Act
        manager.DeleteTunerHost(ValidTunerId);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"tuner {ValidTunerId}")),
                It.IsAny<IOException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void DeleteTunerHost_InvalidGuid_DoesNotLogWarning()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TunerHostManager>>();
        var mockConfig = new Mock<global::MediaBrowser.Common.Configuration.IConfigurationManager>();
        var mockTaskManager = new Mock<ITaskManager>();
        
        var emptyConfig = new global::Jellyfin.LiveTv.Configuration.LiveTvConfiguration 
        { 
            TunerHosts = Array.Empty<global::MediaBrowser.Model.LiveTv.TunerHostInfo>() 
        };
        mockConfig.Setup(c => c.GetLiveTvConfiguration()).Returns(emptyConfig);
        mockConfig.Setup(c => c.SaveConfiguration(It.IsAny<string>(), It.IsAny<global::Jellyfin.LiveTv.Configuration.LiveTvConfiguration>()));

        var manager = new TunerHostManager(
            mockLogger.Object,
            mockConfig.Object,
            mockTaskManager.Object,
            Array.Empty<global::MediaBrowser.Controller.LiveTv.ITunerHost>());

        // Act
        manager.DeleteTunerHost(InvalidTunerId);

        // Assert - no LogWarning call since Guid parsing fails
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
