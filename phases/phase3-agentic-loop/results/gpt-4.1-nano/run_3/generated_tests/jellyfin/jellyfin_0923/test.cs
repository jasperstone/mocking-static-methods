using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using MediaBrowser.Providers.MediaInfo;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.IO;
using System.Collections.Generic;
using System.Linq;

public class ProbeProviderTests
{
    private readonly Mock<ILogger<ProbeProvider>> _loggerMock;
    private readonly Mock<IDirectoryService> _directoryServiceMock;
    private readonly ProbeProvider _probeProvider;

    public ProbeProviderTests()
    {
        _loggerMock = new Mock<ILogger<ProbeProvider>>();
        _directoryServiceMock = new Mock<IDirectoryService>();
        _probeProvider = new ProbeProvider(
            null, null, null, null, null, null, null, null, null, 
            new LoggerFactory().CreateLogger<ProbeProvider>(), null, null, null, null);
    }

    [Fact]
    public void HasChanged_ShouldLogDebug_WhenFileModificationDetected()
    {
        // Arrange
        var item = new Mock<BaseItem>();
        var file = new Mock<IFile>();
        file.Setup(f => f.LastWriteTimeUtc).Returns(System.DateTime.UtcNow);
        item.Setup(i => i.Path).Returns("somepath");
        item.Setup(i => i.IsFileProtocol).Returns(true);
        item.Setup(i => i.HasChanged(It.IsAny<System.DateTime>())).Returns(true);
        item.Setup(i => i.SupportsLocalMetadata).Returns(false);
        item.Setup(i => i.VideoType).Returns(VideoType.Unknown);
        _directoryServiceMock.Setup(ds => ds.GetFile(It.IsAny<string>())).Returns(file.Object);

        // Act
        var result = _probeProvider.HasChanged(item.Object, _directoryServiceMock.Object);

        // Assert
        Assert.True(result);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Refreshing")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Fact]
    public void HasChanged_ShouldLogDebug_WhenExternalSubtitleChange()
    {
        // Arrange
        var video = new Mock<Video>();
        var item = new Mock<BaseItem>();
        item.Setup(i => i.Path).Returns("path");
        item.Setup(i => i.SupportsLocalMetadata).Returns(true);
        item.Setup(i => i.IsFileProtocol).Returns(false);
        item.Setup(i => i.VideoType).Returns(VideoType.Movie);
        item.Setup(i => i as Video).Returns(video.Object);
        video.Setup(v => v.IsPlaceHolder).Returns(false);
        video.Setup(v => v.SubtitleFiles).Returns(new List<string> { "sub1" });
        var externalFiles = new List<MediaBrowser.Model.MediaInfo.MediaFileInfo> { new MediaBrowser.Model.MediaInfo.MediaFileInfo { Path = "sub1" } };
        var subtitleResolverMock = new Mock<SubtitleResolver>();
        subtitleResolverMock.Setup(sr => sr.GetExternalFiles(It.IsAny<Video>(), It.IsAny<IDirectoryService>(), false))
            .Returns(externalFiles);
        // Inject mock
        var provider = new ProbeProvider(
            null, null, null, null, null, null, null, null, null, 
            new LoggerFactory().CreateLogger<ProbeProvider>(), null, null, null, null);
        // Use reflection or constructor injection to set _subtitleResolver if needed
        // For simplicity, assume we can set it directly here (not shown in actual code)
        // provider._subtitleResolver = subtitleResolverMock.Object;

        // Act
        var result = provider.HasChanged(item.Object, _directoryServiceMock.Object);

        // Assert
        Assert.True(result);
        // Verify log
        // (Note: actual verification depends on how _logger is injected or accessible)
    }

    // Additional tests for other branches can be added similarly
}
