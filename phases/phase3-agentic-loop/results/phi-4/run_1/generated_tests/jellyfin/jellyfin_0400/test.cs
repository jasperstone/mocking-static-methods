using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.Trickplay;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.IO;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Drawing;
using Microsoft.EntityFrameworkCore;

public class TrickplayManagerTests
{
    [Fact]
    public async Task RefreshTrickplayDataAsync_LogsInformationOnSuccess()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TrickplayManager>>();
        var mediaEncoderMock = new Mock<IMediaEncoder>();
        var fileSystemMock = new Mock<IFileSystem>();
        var encodingHelperMock = new Mock<EncodingHelper>();
        var configMock = new Mock<IServerConfigurationManager>();
        var imageEncoderMock = new Mock<IImageEncoder>();
        var dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
        var appPathsMock = new Mock<IApplicationPaths>();
        var pathManagerMock = new Mock<IPathManager>();

        var trickplayManager = new TrickplayManager(
            loggerMock.Object,
            mediaEncoderMock.Object,
            fileSystemMock.Object,
            encodingHelperMock.Object,
            configMock.Object,
            imageEncoderMock.Object,
            dbProviderMock.Object,
            appPathsMock.Object,
            pathManagerMock.Object);

        var video = new Video { Id = Guid.NewGuid(), Name = "Test Video" };
        var libraryOptions = new LibraryOptions { EnableTrickplayImageExtraction = true, SaveTrickplayWithMedia = true };
        var cancellationToken = CancellationToken.None;

        // Act
        await trickplayManager.RefreshTrickplayDataAsync(video, false, libraryOptions, cancellationToken);

        // Assert
        loggerMock.Verify(
            l => l.LogInformation("Finished creation of trickplay files for {0}", It.Is<string>(s => s == video.Name)),
            Times.Once);
    }
}
