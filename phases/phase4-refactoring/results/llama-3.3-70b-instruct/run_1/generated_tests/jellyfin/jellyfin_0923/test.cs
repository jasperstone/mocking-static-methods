using Xunit;
using Moq;
using MediaBrowser.Providers.MediaInfo;
using Microsoft.Extensions.Logging;

public class ProbeProviderTests
{
    [Fact]
    public void HasChanged_LogsDebugMessage_WhenExternalLyricsChange()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ProbeProvider>>();
        var probeProvider = new ProbeProvider(
            Mock.Of<MediaBrowser.Controller.MediaSourceManager.IMediaSourceManager>(),
            Mock.Of<MediaBrowser.Controller.MediaEncoding.IMediaEncoder>(),
            Mock.Of<MediaBrowser.Controller.Examiners.IBlurayExaminer>(),
            Mock.Of<MediaBrowser.Controller.Globalization.ILocalizationManager>(),
            Mock.Of<MediaBrowser.Controller.Chapters.IChapterManager>(),
            Mock.Of<MediaBrowser.Controller.Configuration.IServerConfigurationManager>(),
            Mock.Of<MediaBrowser.Controller.Subtitles.ISubtitleManager>(),
            Mock.Of<MediaBrowser.Controller.Library.ILibraryManager>(),
            Mock.Of<MediaBrowser.Model.IO.IFileSystem>(),
            new LoggerFactory(),
            new MediaBrowser.Model.Globalization.NamingOptions(),
            Mock.Of<MediaBrowser.Controller.Lyrics.ILyricManager>(),
            Mock.Of<MediaBrowser.Controller.MediaAttachments.IMediaAttachmentRepository>(),
            Mock.Of<MediaBrowser.Controller.MediaStreams.IMediaStreamRepository>()
        );
        probeProvider._logger = loggerMock.Object;
        var item = new MediaBrowser.Model.Entities.Audio();
        item.Path = "path";
        item.LyricFiles = new[] { "lyric1" };
        var directoryServiceMock = new Mock<MediaBrowser.Model.IO.IDirectoryService>();
        directoryServiceMock.Setup(ds => ds.GetFile(It.IsAny<string>())).Returns(new FileInfo("path"));
        var lyricResolverMock = new Mock<MediaBrowser.Controller.Lyrics.ILyricResolver>();
        lyricResolverMock.Setup(lr => lr.GetExternalFiles(It.IsAny<MediaBrowser.Model.Entities.Audio>(), It.IsAny<MediaBrowser.Model.IO.IDirectoryService>(), It.IsAny<bool>()))
            .Returns(new[] { new MediaBrowser.Controller.Lyrics.LyricFileInfo { Path = "lyric2" } });

        // Act
        var result = probeProvider.HasChanged(item, directoryServiceMock.Object);

        // Assert
        loggerMock.Verify(l => l.LogDebug("Refreshing {ItemPath} due to external lyrics change.", item.Path), Times.Once);
    }
}
