using System;
using System.IO;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
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
        private readonly LibraryManager _libraryManager;

        public LibraryManagerTests()
        {
            _loggerMock = new Mock<ILogger<LibraryManager>>();
            _fileSystemMock = new Mock<IFileSystem>();

            var mockTaskManager = new Mock<ITaskManager>();
            var mockUserManager = new Mock<IUserManager>();
            var mockUserDataManager = new Mock<IUserDataManager>();
            var mockConfigurationManager = new Mock<IServerConfigurationManager>();
            var mockLibraryMonitorFactory = new Mock<Lazy<ILibraryMonitor>>();
            var mockProviderManagerFactory = new Mock<Lazy<IProviderManager>>();
            var mockUserViewManagerFactory = new Mock<Lazy<IUserViewManager>>();
            var mockAppHost = new Mock<IServerApplicationHost>();
            var mockMediaEncoder = new Mock<IMediaEncoder>();
            var mockItemRepository = new Mock<IItemRepository>();
            var mockPersistenceService = new Mock<IItemPersistenceService>();
            var mockNextUpService = new Mock<INextUpService>();
            var mockCountService = new Mock<IItemCountService>();
            var mockLinkedChildrenService = new Mock<ILinkedChildrenService>();
            var mockImageProcessor = new Mock<IImageProcessor>();
            var mockNamingOptions = new NamingOptions();
            var mockDirectoryService = new Mock<IDirectoryService>();
            var mockPeopleRepository = new Mock<IPeopleRepository>();
            var mockPathManager = new Mock<IPathManager>();
            var mockDotIgnoreIgnoreRule = new Mock<DotIgnoreIgnoreRule>();

            _libraryManager = new LibraryManager(
                mockAppHost.Object,
                _loggerMock.Object,
                mockTaskManager.Object,
                mockUserManager.Object,
                mockConfigurationManager.Object,
                mockUserDataManager.Object,
                mockLibraryMonitorFactory.Object,
                _fileSystemMock.Object,
                mockProviderManagerFactory.Object,
                mockUserViewManagerFactory.Object,
                mockMediaEncoder.Object,
                mockItemRepository.Object,
                mockPersistenceService.Object,
                mockNextUpService.Object,
                mockCountService.Object,
                mockLinkedChildrenService.Object,
                mockImageProcessor.Object,
                mockNamingOptions,
                mockDirectoryService.Object,
                mockPeopleRepository.Object,
                mockPathManager.Object,
                mockDotIgnoreIgnoreRule.Object);
        }

        [Fact]
        public void ResolvePath_LogError_WhenIntroResolverReturnsNull()
        {
            // Arrange
            var info = new IntroInfo { Path = "testPath" };
            _fileSystemMock.Setup(fs => fs.GetFileSystemInfo(info.Path)).Returns(new FileSystemMetadata { IsDirectory = false });

            // Act
            var result = _libraryManager.ResolvePath(info);

            // Assert
            _loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Intro resolver returned null for {Path}.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void ResolvePath_LogError_WhenExceptionIsThrown()
        {
            // Arrange
            var info = new IntroInfo { Path = "testPath" };
            _fileSystemMock.Setup(fs => fs.GetFileSystemInfo(info.Path)).Throws(new Exception("Test exception"));

            // Act
            var result = _libraryManager.ResolvePath(info);

            // Assert
            _loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error resolving path {Path}.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void ResolvePath_LogError_WhenIntroProviderReturnsNullPathAndItemId()
        {
            // Arrange
            var info = new IntroInfo { Path = null, ItemId = null };

            // Act
            var result = _libraryManager.ResolvePath(info);

            // Assert
            _loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("IntroProvider returned an IntroInfo with null Path and ItemId.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.Null(result);
        }
    }
}
