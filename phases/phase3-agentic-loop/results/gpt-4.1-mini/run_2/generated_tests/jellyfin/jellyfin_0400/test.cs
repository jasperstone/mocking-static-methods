using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Implementations.Trickplay;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Drawing;
using Jellyfin.Database.Implementations;
using Microsoft.EntityFrameworkCore;
using Jellyfin.Database.Implementations.Entities;
using MediaBrowser.Model.IO;

namespace Jellyfin.Server.Implementations.Trickplay.Tests
{
    public class TrickplayManagerTests
    {
        [Fact]
        public async Task RefreshTrickplayDataAsync_LogsInformationOnSuccessfulSave()
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

            var trickplayOptions = new TrickplayOptions
            {
                Interval = 1000,
                WidthResolutions = new List<int> { 320 }
            };
            var configuration = new Configuration
            {
                TrickplayOptions = trickplayOptions
            };
            configMock.Setup(c => c.Configuration).Returns(configuration);

            var video = new Video { Id = Guid.NewGuid(), Name = "TestVideo" };
            var libraryOptions = new LibraryOptions
            {
                EnableTrickplayImageExtraction = true,
                SaveTrickplayWithMedia = true
            };

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

            // Setup pathManager to return a directory path
            pathManagerMock.Setup(p => p.GetTrickplayDirectory(It.IsAny<Video>(), It.IsAny<bool>())).Returns("somepath");

            // Setup dbProvider to return a mock DbContext with TrickplayInfos that supports ExecuteDeleteAsync
            var dbContextMock = new Mock<JellyfinDbContext>();
            var trickplayInfosMock = new Mock<DbSet<TrickplayInfo>>();
            dbContextMock.Setup(d => d.TrickplayInfos).Returns(trickplayInfosMock.Object);
            dbProviderMock.Setup(d => d.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dbContextMock.Object);

            // Act
            await trickplayManager.RefreshTrickplayDataAsync(video, true, libraryOptions, CancellationToken.None);

            // Assert
            // Verify that LogInformation was called with the expected message template containing "Finished creation of trickplay files"
            // The exact call is on _logger.LogInformation("Finished creation of trickplay files for {0}", mediaPath);
            // We cannot directly verify that because mediaPath is internal, so we verify any LogInformation call with that message.

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Finished creation of trickplay files")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
