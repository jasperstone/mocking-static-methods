using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using Emby.Naming.TV;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Controller.Providers;

namespace Emby.Server.Implementations.Tests.Library
{
    public class LibraryManagerTests
    {
        private readonly Mock<ILogger<LibraryManager>> _loggerMock;
        private readonly Mock<MediaBrowser.Controller.IO.IFileSystem> _fileSystemMock;
        private readonly LibraryManager _libraryManager;

        public LibraryManagerTests()
        {
            _loggerMock = new Mock<ILogger<LibraryManager>>();
            _fileSystemMock = new Mock<MediaBrowser.Controller.IO.IFileSystem>();

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(_loggerMock.Object);

            _libraryManager = new LibraryManager(
                Mock.Of<MediaBrowser.Controller.IServerApplicationHost>(),
                loggerFactoryMock.Object,
                Mock.Of<MediaBrowser.Controller.ITaskManager>(),
                Mock.Of<MediaBrowser.Controller.IUserManager>(),
                Mock.Of<MediaBrowser.Controller.Configuration.IServerConfigurationManager>(),
                Mock.Of<MediaBrowser.Controller.IUserDataManager>(),
                new Lazy<MediaBrowser.Controller.Library.ILibraryMonitor>(() => Mock.Of<MediaBrowser.Controller.Library.ILibraryMonitor>()),
                _fileSystemMock.Object,
                new Lazy<MediaBrowser.Controller.Providers.IProviderManager>(() => Mock.Of<MediaBrowser.Controller.Providers.IProviderManager>()),
                new Lazy<MediaBrowser.Controller.Library.IUserViewManager>(() => Mock.Of<MediaBrowser.Controller.Library.IUserViewManager>()),
                Mock.Of<MediaBrowser.Controller.MediaEncoding.IMediaEncoder>(),
                Mock.Of<MediaBrowser.Controller.Persistence.IItemRepository>(),
                Mock.Of<MediaBrowser.Controller.Persistence.IItemPersistenceService>(),
                Mock.Of<MediaBrowser.Controller.Library.INextUpService>(),
                Mock.Of<MediaBrowser.Controller.Library.IItemCountService>(),
                Mock.Of<MediaBrowser.Controller.Library.ILinkedChildrenService>(),
                Mock.Of<MediaBrowser.Controller.Drawing.IImageProcessor>(),
                new Emby.Naming.Common.NamingOptions(),
                Mock.Of<MediaBrowser.Controller.IO.IDirectoryService>(),
                Mock.Of<MediaBrowser.Controller.Persistence.IPeopleRepository>(),
                Mock.Of<MediaBrowser.Controller.IPathManager>(),
                Mock.Of<Emby.Server.Implementations.Library.DotIgnoreIgnoreRule>());
        }

        [Fact]
        public void ResolveIntroPath_ExceptionThrown_LogsErrorWithPath()
        {
            // Arrange
            var introInfo = new IntroInfo { Path = "/test/video.mp4" };
            var testException = new InvalidOperationException("Test file system error");

            _fileSystemMock
                .Setup(fs => fs.GetFileSystemInfo("/test/video.mp4"))
                .Throws(testException);

            // Act
            var result = InvokePrivateResolveIntroPath(_libraryManager, introInfo);

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => 
                        state.ToString()!.Contains("Error resolving path /test/video.mp4")),
                    It.Is<Exception>(ex => ex == testException),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.Null(result);
        }

        private static object? InvokePrivateResolveIntroPath(LibraryManager manager, IntroInfo info)
        {
            var method = typeof(LibraryManager).GetMethod(
                "ResolveIntroPath", 
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { typeof(IntroInfo) },
                null);

            return method?.Invoke(manager, new object?[] { info });
        }
    }
}
