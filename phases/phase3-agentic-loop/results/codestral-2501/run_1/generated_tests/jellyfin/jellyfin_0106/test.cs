using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Resolvers;
using MediaBrowser.Model.IO;
using Emby.Server.Implementations.Library;
using Emby.Server.Implementations.Library.Resolvers;
using System;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        private readonly Mock<ILogger<LibraryManager>> _loggerMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly LibraryManager _libraryManager;

        public LibraryManagerTests()
        {
            _loggerMock = new Mock<ILogger<LibraryManager>>();
            _fileSystemMock = new Mock<IFileSystem>();

            _libraryManager = new LibraryManager(
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<ILoggerFactory>(),
                Mock.Of<ITaskManager>(),
                Mock.Of<IUserManager>(),
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<IUserDataManager>(),
                new Lazy<ILibraryMonitor>(() => Mock.Of<ILibraryMonitor>()),
                _fileSystemMock.Object,
                new Lazy<IProviderManager>(() => Mock.Of<IProviderManager>()),
                new Lazy<IUserViewManager>(() => Mock.Of<IUserViewManager>()),
                Mock.Of<IMediaEncoder>(),
                Mock.Of<IItemRepository>(),
                Mock.Of<IItemPersistenceService>(),
                Mock.Of<INextUpService>(),
                Mock.Of<IItemCountService>(),
                Mock.Of<ILinkedChildrenService>(),
                Mock.Of<IImageProcessor>(),
                new NamingOptions(),
                Mock.Of<IDirectoryService>(),
                Mock.Of<IPeopleRepository>(),
                Mock.Of<IPathManager>(),
                new DotIgnoreIgnoreRule()
            );
        }

        [Fact]
        public void ResolvePath_WhenPathIsNull_LogsError()
        {
            // Arrange
            var info = new IntroInfo { Path = null };

            // Act
            var result = _libraryManager.ResolvePath(info);

            // Assert
            _loggerMock.Verify(
                x => x.LogError("IntroProvider returned an IntroInfo with null Path and ItemId."),
                Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void ResolvePath_WhenPathIsEmpty_LogsError()
        {
            // Arrange
            var info = new IntroInfo { Path = string.Empty };

            // Act
            var result = _libraryManager.ResolvePath(info);

            // Assert
            _loggerMock.Verify(
                x => x.LogError("IntroProvider returned an IntroInfo with null Path and ItemId."),
                Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void ResolvePath_WhenResolvePathThrowsException_LogsError()
        {
            // Arrange
            var info = new IntroInfo { Path = "testPath" };
            _fileSystemMock.Setup(fs => fs.GetFileSystemInfo(It.IsAny<string>())).Throws(new Exception("Test exception"));

            // Act
            var result = _libraryManager.ResolvePath(info);

            // Assert
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), "Error resolving path {Path}.", info.Path),
                Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void ResolvePath_WhenResolvePathReturnsNull_LogsError()
        {
            // Arrange
            var info = new IntroInfo { Path = "testPath" };
            _fileSystemMock.Setup(fs => fs.GetFileSystemInfo(It.IsAny<string>())).Returns(Mock.Of<IFileSystemMetadata>());

            // Act
            var result = _libraryManager.ResolvePath(info);

            // Assert
            _loggerMock.Verify(
                x => x.LogError("Intro resolver returned null for {Path}.", info.Path),
                Times.Once);
            Assert.Null(result);
        }
    }
}
