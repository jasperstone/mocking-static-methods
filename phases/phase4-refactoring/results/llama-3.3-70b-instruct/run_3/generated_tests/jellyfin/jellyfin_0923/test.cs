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
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, loggerMock.Object);
        var item = new Audio();
        item.Path = "path";
        item.LyricFiles = new[] { "lyric1" };
        var directoryServiceMock = new Mock<IDirectoryService>();
        directoryServiceMock.Setup(ds => ds.GetFile(It.IsAny<string>())).Returns(new FileInfo("path"));
        var lyricResolverMock = new Mock<ILyricResolver>();
        lyricResolverMock.Setup(lr => lr.GetExternalFiles(item, directoryServiceMock.Object, false))
            .Returns(new[] { new ExternalFile { Path = "lyric2" } });

        // Act
        var result = probeProvider.HasChanged(item, directoryServiceMock.Object);

        // Assert
        loggerMock.Verify(l => l.LogDebug("Refreshing {ItemPath} due to external lyrics change.", item.Path), Times.Once);
    }
}
