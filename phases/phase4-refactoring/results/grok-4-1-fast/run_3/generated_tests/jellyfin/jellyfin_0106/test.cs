using System;
using System.IO;
using System.Reflection;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        private readonly Mock<ILogger<LibraryManager>> _mockLogger;
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly LibraryManager _libraryManager;

        public LibraryManagerTests()
        {
            _mockLogger = new Mock<ILogger<LibraryManager>>();
            _mockFileSystem = new Mock<IFileSystem>();

            // Simplified constructor - only provide what's strictly needed for the specific path
            // Many dependencies are lazy or have default values that work with nulls/mocks
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger<LibraryManager>()).Returns(_mockLogger.Object);

            _libraryManager = new LibraryManager(
                null!, // IServerApplicationHost
                mockLoggerFactory.Object,
                null!, // ITaskManager
                null!, // IUserManager
                null!, // IServerConfigurationManager
                null!, // IUserDataManager
                new Lazy<ILibraryMonitor>(() => null!), // Lazy<ILibraryMonitor>
                _mockFileSystem.Object,
                new Lazy<IProviderManager>(() => null!), // Lazy<IProviderManager>
                new Lazy<IUserViewManager>(() => null!), // Lazy<IUserViewManager>
                null!, // IMediaEncoder
                null!, // IItemRepository
                null!, // IItemPersistenceService
                null!, // INextUpService
                null!, // IItemCountService
                null!, // ILinkedChildrenService
                null!, // IImageProcessor
                null!, // NamingOptions
                null!, // IDirectoryService
                null!, // IPeopleRepository
                null!, // IPathManager
                null!  // DotIgnoreIgnoreRule
            );
        }

        [Fact]
        public void ResolveIntroPath_ExceptionThrown_LogsErrorWithPath()
        {
            // Arrange
            var introInfo = new IntroInfo { Path = "/test/path/video.mp4" };
            var testException = new InvalidOperationException("Test file system error");
            
            _mockFileSystem.Setup(fs => fs.GetFileSystemInfo("/test/path/video.mp4"))
                .Throws(testException);

            // Act & Assert - Verifies the LogError call at line 2129
            var result = InvokeResolveIntroPathPrivate(introInfo);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("Error resolving path /test/path/video.mp4")),
                    testException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void ResolveIntroPath_NullPathAndItemId_LogsErrorMessage()
        {
            // Arrange
            var introInfo = new IntroInfo();

            // Act & Assert
            var result = InvokeResolveIntroPathPrivate(introInfo);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("IntroProvider returned an IntroInfo with null Path and ItemId")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void ResolveIntroPath_NullVideoResolution_LogsErrorMessage()
        {
            // Arrange
            var introInfo = new IntroInfo { Path = "/test/path/video.mp4" };
            var mockFileInfo = new Mock<FileSystemMetadata>();
            mockFileInfo.Setup(f => f.IsDirectory).Returns(false);
            
            _mockFileSystem.Setup(fs => fs.GetFileSystemInfo("/test/path/video.mp4"))
                .Returns(mockFileInfo.Object);

            // Act & Assert
            var result = InvokeResolveIntroPathPrivate(introInfo);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("Intro resolver returned null for /test/path/video.mp4")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private Video? InvokeResolveIntroPathPrivate(IntroInfo info)
        {
            // Use reflection to call the private method containing the LogError at line 2129
            var method = typeof(LibraryManager).GetMethod(
                "ResolveIntroPath",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            
            return (Video?)method?.Invoke(_libraryManager, new object?[] { info });
        }
    }

    // Test double for IntroInfo (internal type)
    public class IntroInfo
    {
        public string? Path { get; set; }
        public Guid? ItemId { get; set; }
    }
}
