using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations;
using Jellyfin.Server.Implementations.Trickplay;
using Jellyfin.Database.Implementations;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class TrickplayManagerTests
{
    [Fact]
    public async Task MoveGeneratedTrickplayDataAsync_LogsInformationOnSuccess()
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

        var video = new Video { Id = Guid.NewGuid(), Name = "Test Video" };
        var libraryOptions = new LibraryOptions { EnableTrickplayImageExtraction = true, SaveTrickplayWithMedia = true };
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

        // Setup mocks
        configMock.Setup(c => c.Configuration).Returns(new ServerConfiguration
        {
            TrickplayOptions = new TrickplayOptions
            {
                Interval = 1000,
                WidthResolutions = new[] { 1280, 1920 }
            }
        });

        fileSystemMock.Setup(f => f.GetFiles(It.IsAny<string>(), It.IsAny<string[]>(), false, false))
            .Returns(new[]
            {
                new FileSystemMetadata { Name = "image1.jpg", FullName = "image1.jpg" },
                new FileSystemMetadata { Name = "image2.jpg", FullName = "image2.jpg" }
            });

        // Act
        await trickplayManager.MoveGeneratedTrickplayDataAsync(video, libraryOptions, CancellationToken.None);

        // Assert
        loggerMock.Verify(
            l => l.LogInformation("Finished creation of trickplay files for {0}", It.IsAny<string>()),
            Times.Once);
    }
}
