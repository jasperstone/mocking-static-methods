using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Implementations.Trickplay;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Configuration;

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
            var encodingHelper = new EncodingHelper();
            var configMock = new Mock<IServerConfigurationManager>();
            var imageEncoderMock = new Mock<IImageEncoder>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<Jellyfin.Database.Implementations.JellyfinDbContext>>();
            var appPathsMock = new Mock<IApplicationPaths>();
            var pathManagerMock = new Mock<IPathManager>();

            var trickplayOptions = new TrickplayOptions
            {
                Interval = 1000,
                WidthResolutions = new List<int> { 320 },
                EnableHwAcceleration = false,
                EnableHwEncoding = false,
                ProcessThreads = 1,
                Qscale = 1,
                ProcessPriority = 0,
                EnableKeyFrameOnlyExtraction = false
            };

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.SetupGet(c => c.TrickplayOptions).Returns(trickplayOptions);
            configMock.SetupGet(c => c.Configuration).Returns(configurationMock.Object);

            var video = new Video { Id = Guid.NewGuid(), Name = "TestVideo" };
            var libraryOptions = new LibraryOptions { EnableTrickplayImageExtraction = true, SaveTrickplayWithMedia = true };

            var trickplayManager = new TrickplayManager(
                loggerMock.Object,
                mediaEncoderMock.Object,
                fileSystemMock.Object,
                encodingHelper,
                configMock.Object,
                imageEncoderMock.Object,
                dbContextFactoryMock.Object,
                appPathsMock.Object,
                pathManagerMock.Object);

            // Setup path manager to return a directory path
            var trickplayDir = Path.Combine(Path.GetTempPath(), "trickplay");
            pathManagerMock.Setup(pm => pm.GetTrickplayDirectory(video, true)).Returns(trickplayDir);

            // Setup db context and trickplay info save simulation
            var dbContextMock = new Mock<Jellyfin.Database.Implementations.JellyfinDbContext>();
            var trickplayInfosMock = new Mock<Microsoft.EntityFrameworkCore.DbSet<Jellyfin.Database.Implementations.Entities.TrickplayInfo>>();
            dbContextMock.SetupGet(d => d.TrickplayInfos).Returns(trickplayInfosMock.Object);
            dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dbContextMock.Object);

            // Setup media encoder to simulate image extraction and tile creation
            mediaEncoderMock.Setup(m => m.ExtractImagesAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<EncodingHelper>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(trickplayDir);

            // Setup file system to simulate files existing in the directory
            fileSystemMock.Setup(fs => fs.GetFiles(trickplayDir, It.IsAny<string[]>(), false, false))
                .Returns(new List<IFileInfo> { new Mock<IFileInfo>().Object });

            // Setup directory existence
            Directory.CreateDirectory(trickplayDir);

            // Act
            await trickplayManager.RefreshTrickplayDataAsync(video, true, libraryOptions, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Finished creation of trickplay files for")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            // Cleanup
            if (Directory.Exists(trickplayDir))
            {
                Directory.Delete(trickplayDir, true);
            }
        }
    }
}
