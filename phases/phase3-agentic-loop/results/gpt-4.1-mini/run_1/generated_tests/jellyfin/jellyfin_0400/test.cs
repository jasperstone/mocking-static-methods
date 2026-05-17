using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.Trickplay;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.Trickplay.Tests
{
    public class TrickplayManagerTests
    {
        [Fact]
        public async Task MoveGeneratedTrickplayDataAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TrickplayManager>>();
            var mediaEncoderMock = new Mock<MediaBrowser.Controller.MediaEncoding.IMediaEncoder>();
            var fileSystemMock = new Mock<MediaBrowser.Controller.IO.IFileSystem>();
            var encodingHelperMock = new Mock<EncodingHelper>();
            var configMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var imageEncoderMock = new Mock<MediaBrowser.Controller.Drawing.IImageEncoder>();
            var dbContextFactoryMock = new Mock<Microsoft.EntityFrameworkCore.IDbContextFactory<Jellyfin.Database.Implementations.JellyfinDbContext>>();
            var appPathsMock = new Mock<MediaBrowser.Common.Configuration.IApplicationPaths>();
            var pathManagerMock = new Mock<MediaBrowser.Controller.IO.IPathManager>();

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

            var configurationMock = new Mock<MediaBrowser.Model.Configuration.IApplicationConfiguration>();
            configurationMock.SetupGet(c => c.TrickplayOptions).Returns(trickplayOptions);
            configMock.SetupGet(c => c.Configuration).Returns(configurationMock.Object);

            var video = new Video { Id = Guid.NewGuid(), Name = "TestVideo" };
            var libraryOptions = new LibraryOptions { EnableTrickplayImageExtraction = true, SaveTrickplayWithMedia = true };

            // We cannot easily mock private methods or DirectoryInfo, so we will simulate the conditions by mocking IFileSystem.MoveDirectory
            // and setting up the trickplay resolutions to cause the log call.

            var trickplayManager = new TrickplayManager(
                loggerMock.Object,
                mediaEncoderMock.Object,
                fileSystemMock.Object,
                encodingHelperMock.Object,
                configMock.Object,
                imageEncoderMock.Object,
                dbContextFactoryMock.Object,
                appPathsMock.Object,
                pathManagerMock.Object);

            // Act
            // We expect no exception and logger.LogInformation to be called at least once if conditions are met.
            // Since private methods and directory checks are not mockable here, this test mainly ensures no exceptions and logger usage.

            await trickplayManager.MoveGeneratedTrickplayDataAsync(video, libraryOptions, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Moved trickplay images for")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
