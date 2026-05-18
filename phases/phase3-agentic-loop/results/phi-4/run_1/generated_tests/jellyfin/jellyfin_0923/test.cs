using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.IO;
using Xunit;

public class ProbeProviderTests
{
    [Fact]
    public void HasChanged_WhenExternalLyricsChange_LogsDebugMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ProbeProvider>>();
        var directoryServiceMock = new Mock<IDirectoryService>();
        var subtitleResolverMock = new Mock<ISubtitleResolver>();
        var audioResolverMock = new Mock<IAudioResolver>();
        var lyricResolverMock = new Mock<ILyricResolver>();

        var audio = new Audio
        {
            Path = "/path/to/audio",
            SupportsLocalMetadata = true,
            LyricFiles = new List<string> { "/path/to/lyric1" }
        };

        var externalLyrics = new List<MediaFile>
        {
            new MediaFile { Path = "/path/to/lyric2" }
        };

        lyricResolverMock.Setup(r => r.GetExternalFiles(audio, directoryServiceMock.Object, false))
            .Returns(externalLyrics);

        var probeProvider = new ProbeProvider(
            null, 
            null, 
            null, 
            null, 
            null, 
            null, 
            null, 
            null, 
            null, 
            null, 
            null, 
            null, 
            null, 
            null, 
            null, 
            loggerMock.Object, 
            null, 
            null, 
            null, 
            null, 
            null, 
            null, 
            null, 
            null, 
            null, 
            null, 
            null, 
            null, 
            null, 
            null, 
            null, 
            null, 
            null);

        // Act
        var result = probeProvider.HasChanged(audio, directoryServiceMock.Object);

        // Assert
        loggerMock.Verify(
            l => l.LogDebug(
                It.Is<string>(s => s == "Refreshing {ItemPath} due to external lyrics change."),
                It.Is<object[]>(o => o[0].ToString() == "/path/to/audio")),
            Times.Once);
        
        Assert.True(result);
    }
}
