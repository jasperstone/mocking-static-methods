using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Resolvers;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Library;
using MediaBrowser.Model.Tasks;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        // Minimal stub for IntroInfo to compile
        public class IntroInfo
        {
            public string? Path { get; set; }
        }

        [Fact]
        public void GetIntroVideo_LogsError_WhenExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(loggerMock.Object);

            var fileSystemMock = new Mock<IFileSystem>();
            var taskManagerMock = new Mock<ITaskManager>();
            var userManagerMock = new Mock<IUserManager>();
            var configManagerMock = new Mock<IServerConfigurationManager>();
            var userDataManagerMock = new Mock<IUserDataManager>();
            var libraryMonitorFactoryMock = new Lazy<ILibraryMonitor>(() => null!);
            var providerManagerFactoryMock = new Lazy<IProviderManager>(() => null!);
            var userViewManagerFactoryMock = new Lazy<IUserViewManager>(() => null!);
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var itemRepositoryMock = new Mock<IItemRepository>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();
            var nextUpServiceMock = new Mock<INextUpService>();
            var countServiceMock = new Mock<IItemCountService>();
            var linkedChildrenServiceMock = new Mock<ILinkedChildrenService>();
            var imageProcessorMock = new Mock<IImageProcessor>();
            var namingOptions = new NamingOptions();
            var directoryServiceMock = new Mock<IDirectoryService>();
            var peopleRepositoryMock = new Mock<IPeopleRepository>();
            var pathManagerMock = new Mock<IPathManager>();
            var dotIgnoreIgnoreRule = new DotIgnoreIgnoreRule();

            var libraryManager = new LibraryManager(
                null!,
                loggerFactoryMock.Object,
                taskManagerMock.Object,
                userManagerMock.Object,
                configManagerMock.Object,
                userDataManagerMock.Object,
                libraryMonitorFactoryMock,
                fileSystemMock.Object,
                providerManagerFactoryMock,
                userViewManagerFactoryMock,
                mediaEncoderMock.Object,
                itemRepositoryMock.Object,
                persistenceServiceMock.Object,
                nextUpServiceMock.Object,
                countServiceMock.Object,
                linkedChildrenServiceMock.Object,
                imageProcessorMock.Object,
                namingOptions,
                directoryServiceMock.Object,
                peopleRepositoryMock.Object,
                pathManagerMock.Object,
                dotIgnoreIgnoreRule);

            var introInfo = new IntroInfo { Path = "somepath" };

            // Setup file system to throw when GetFileSystemInfo is called
            fileSystemMock.Setup(fs => fs.GetFileSystemInfo(It.IsAny<string>())).Throws(new InvalidOperationException("fail"));

            // Act
            var result = libraryManager.GetIntroVideo(introInfo);

            // Assert
            Assert.Null(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error resolving path somepath.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
