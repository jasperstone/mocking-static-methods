using System;
using System.IO;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        private readonly Mock<ILogger<LibraryManager>> _loggerMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IItemRepository> _itemRepositoryMock;
        private readonly LibraryManager _libraryManager;

        public LibraryManagerTests()
        {
            _loggerMock = new Mock<ILogger<LibraryManager>>();
            _fileSystemMock = new Mock<IFileSystem>();
            _itemRepositoryMock = new Mock<IItemRepository>();

            // Simplified constructor setup for testing the specific method
            _libraryManager = new LibraryManagerTestFixture(
                _loggerMock.Object,
                _fileSystemMock.Object,
                _itemRepositoryMock.Object);
        }

        [Fact]
        public void ResolveIntroPath_ThrowsException_LogsErrorWithPath()
        {
            // Arrange
            var introInfo = new IntroInfo { Path = "/test/video.mp4" };
            var fileSystemInfoMock = new Mock<FileSystemInfo>();
            _fileSystemMock
                .Setup(fs => fs.GetFileSystemInfo(It.IsAny<string>()))
                .Throws(new InvalidOperationException("Test exception"));

            // Act
            var result = _libraryManager.ResolveIntroPath(introInfo);

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => t == "Error resolving path {Path}."),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);

            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => t!.ToString().Contains("Error resolving path /test/video.mp4")),
                    It.Is<Exception>(ex => ex.Message == "Test exception"),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);

            Assert.Null(result);
        }

        [Fact]
        public void ResolveIntroPath_NullVideoFromResolver_LogsErrorWithPath()
        {
            // Arrange
            var introInfo = new IntroInfo { Path = "/test/video.mp4" };
            var fileSystemInfoMock = new Mock<FileSystemInfo>();
            _fileSystemMock
                .Setup(fs => fs.GetFileSystemInfo("/test/video.mp4"))
                .Returns(fileSystemInfoMock.Object);

            // Act
            var result = _libraryManager.ResolveIntroPath(introInfo);

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => t == "Intro resolver returned null for {Path}."),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);

            Assert.Null(result);
        }

        [Fact]
        public void ResolveIntroPath_NullPathAndItemId_LogsErrorMessage()
        {
            // Arrange
            var introInfo = new IntroInfo { Path = null!, ItemId = null };

            // Act
            var result = _libraryManager.ResolveIntroPath(introInfo);

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => t == "IntroProvider returned an IntroInfo with null Path and ItemId."),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);

            Assert.Null(result);
        }
    }

    // Simplified fixture for testing the specific method without full dependency graph
    internal class LibraryManagerTestFixture : Emby.Server.Implementations.Library.LibraryManager
    {
        public LibraryManagerTestFixture(
            Microsoft.Extensions.Logging.ILogger<Emby.Server.Implementations.Library.LibraryManager> logger,
            IFileSystem fileSystem,
            IItemRepository itemRepository)
            : base(
                Mock.Of<MediaBrowser.Controller.IServerApplicationHost>(),
                Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>(),
                Mock.Of<MediaBrowser.Controller.ITaskManager>(),
                Mock.Of<MediaBrowser.Controller.IUserManager>(),
                Mock.Of<MediaBrowser.Controller.Configuration.IServerConfigurationManager>(),
                Mock.Of<MediaBrowser.Controller.IUserDataManager>(),
                new System.Lazy<MediaBrowser.Controller.Library.ILibraryMonitor>(() => Mock.Of<MediaBrowser.Controller.Library.ILibraryMonitor>()),
                fileSystem,
                new System.Lazy<MediaBrowser.Controller.Providers.IProviderManager>(() => Mock.Of<MediaBrowser.Controller.Providers.IProviderManager>()),
                new System.Lazy<MediaBrowser.Controller.Library.IUserViewManager>(() => Mock.Of<MediaBrowser.Controller.Library.IUserViewManager>()),
                Mock.Of<MediaBrowser.Controller.MediaEncoding.IMediaEncoder>(),
                itemRepository,
                Mock.Of<MediaBrowser.Controller.Persistence.IItemPersistenceService>(),
                Mock.Of<MediaBrowser.Controller.Library.INextUpService>(),
                Mock.Of<MediaBrowser.Controller.Library.IItemCountService>(),
                Mock.Of<MediaBrowser.Controller.Library.ILinkedChildrenService>(),
                Mock.Of<MediaBrowser.Controller.Drawing.IImageProcessor>(),
                new Emby.Naming.Common.NamingOptions(),
                Mock.Of<MediaBrowser.Controller.IO.IDirectoryService>(),
                Mock.Of<Jellyfin.Data.IPeopleRepository>(),
                Mock.Of<MediaBrowser.Controller.IPathManager>(),
                Mock.Of<Emby.Server.Implementations.Library.DotIgnoreIgnoreRule>())
        {
        }

        public new Video? ResolveIntroPath(IntroInfo info)
        {
            return base.ResolveIntroPath(info);
        }
    }
}
