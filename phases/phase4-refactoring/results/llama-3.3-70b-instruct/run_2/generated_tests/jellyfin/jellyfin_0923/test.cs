using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emby.Naming.Common;
using MediaBrowser.Controller.Chapters;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Lyrics;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Subtitles;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.MediaInfo;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Serialization;
using Xunit;

namespace MediaBrowser.Providers.MediaInfo
{
    public class ProbeProviderTests
    {
        [Fact]
        public async Task FetchAsync_VideoItem_ReturnsTrue()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<ProbeProvider>();
            var mediaSourceManager = new Mock<IMediaSourceManager>().Object;
            var mediaEncoder = new Mock<IMediaEncoder>().Object;
            var blurayExaminer = new Mock<IBlurayExaminer>().Object;
            var localization = new Mock<ILocalizationManager>().Object;
            var chapterManager = new Mock<IChapterManager>().Object;
            var config = new Mock<IServerConfigurationManager>().Object;
            var subtitleManager = new Mock<ISubtitleManager>().Object;
            var libraryManager = new Mock<ILibraryManager>().Object;
            var fileSystem = new Mock<IFileSystem>().Object;
            var namingOptions = new NamingOptions();
            var lyricManager = new Mock<ILyricManager>().Object;
            var mediaAttachmentRepository = new Mock<IMediaAttachmentRepository>().Object;
            var mediaStreamRepository = new Mock<IMediaStreamRepository>().Object;

            var probeProvider = new ProbeProvider(
                mediaSourceManager,
                mediaEncoder,
                blurayExaminer,
                localization,
                chapterManager,
                config,
                subtitleManager,
                libraryManager,
                fileSystem,
                loggerFactory,
                namingOptions,
                lyricManager,
                mediaAttachmentRepository,
                mediaStreamRepository);

            var item = new Video
            {
                Path = "path",
                SupportsLocalMetadata = true,
                VideoType = VideoType.VideoFile
            };

            var directoryService = new Mock<IDirectoryService>();
            directoryService.Setup(ds => ds.GetFile(It.IsAny<string>())).Returns(new Mock<IFileInfo>().Object);

            // Act
            var result = await probeProvider.FetchAsync(item, new MetadataRefreshOptions(), CancellationToken.None);

            // Assert
            Assert.True(result == ItemUpdateType.MetadataChanged);
        }

        [Fact]
        public async Task FetchAsync_AudioItem_ReturnsTrue()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<ProbeProvider>();
            var mediaSourceManager = new Mock<IMediaSourceManager>().Object;
            var mediaEncoder = new Mock<IMediaEncoder>().Object;
            var blurayExaminer = new Mock<IBlurayExaminer>().Object;
            var localization = new Mock<ILocalizationManager>().Object;
            var chapterManager = new Mock<IChapterManager>().Object;
            var config = new Mock<IServerConfigurationManager>().Object;
            var subtitleManager = new Mock<ISubtitleManager>().Object;
            var libraryManager = new Mock<ILibraryManager>().Object;
            var fileSystem = new Mock<IFileSystem>().Object;
            var namingOptions = new NamingOptions();
            var lyricManager = new Mock<ILyricManager>().Object;
            var mediaAttachmentRepository = new Mock<IMediaAttachmentRepository>().Object;
            var mediaStreamRepository = new Mock<IMediaStreamRepository>().Object;

            var probeProvider = new ProbeProvider(
                mediaSourceManager,
                mediaEncoder,
                blurayExaminer,
                localization,
                chapterManager,
                config,
                subtitleManager,
                libraryManager,
                fileSystem,
                loggerFactory,
                namingOptions,
                lyricManager,
                mediaAttachmentRepository,
                mediaStreamRepository);

            var item = new Audio
            {
                Path = "path",
                SupportsLocalMetadata = true
            };

            var directoryService = new Mock<IDirectoryService>();
            directoryService.Setup(ds => ds.GetFile(It.IsAny<string>())).Returns(new Mock<IFileInfo>().Object);

            // Act
            var result = await probeProvider.FetchAsync(item, new MetadataRefreshOptions(), CancellationToken.None);

            // Assert
            Assert.True(result == ItemUpdateType.MetadataChanged);
        }
    }
}
