using System;
using System.Collections.Generic;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Model.Tasks;
using Emby.Naming.Common;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerLoggerTests
    {
        [Fact]
        public void LibraryManager_ResolveIntroPath_ThrowsException_LogsErrorWithPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(loggerMock.Object);

            var fileSystemMock = new Mock<IFileSystem>();
            fileSystemMock.Setup(fs => fs.GetFileSystemInfo(It.IsAny<string>()))
                         .Throws(new InvalidOperationException("Test exception"));

            // Minimal mocks with proper namespaces
            var mocks = CreateMinimalMocks(loggerFactoryMock.Object, fileSystemMock.Object);

            var libraryManager = new LibraryManager(
                mocks.appHostMock.Object,
                loggerFactoryMock.Object,
                mocks.taskManagerMock.Object,
                mocks.userManagerMock.Object,
                mocks.configurationManagerMock.Object,
                mocks.userDataManagerMock.Object,
                mocks.libraryMonitorFactoryMock.Object,
                fileSystemMock.Object,
                mocks.providerManagerFactoryMock.Object,
                mocks.userViewManagerFactoryMock.Object,
                mocks.mediaEncoderMock.Object,
                mocks.itemRepositoryMock.Object,
                mocks.persistenceServiceMock.Object,
                mocks.nextUpServiceMock.Object,
                mocks.countServiceMock.Object,
                mocks.linkedChildrenServiceMock.Object,
                mocks.imageProcessorMock.Object,
                new NamingOptions(),
                mocks.directoryServiceMock.Object,
                mocks.peopleRepositoryMock.Object,
                mocks.pathManagerMock.Object,
                new DotIgnoreIgnoreRule());

            var introInfo = new IntroInfo { Path = "/test/video.mp4" };

            // Act
            var result = libraryManager.ResolveIntroPath(introInfo);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>(
                        func => func(null!, new InvalidOperationException("Test exception"))
                                .Contains("Error resolving path /test/video.mp4") ?? false)),
                Times.Once);
        }

        private static (Mock<IServerApplicationHost> appHostMock, Mock<ITaskManager> taskManagerMock, Mock<IUserManager> userManagerMock, Mock<IServerConfigurationManager> configurationManagerMock, Mock<IUserDataManager> userDataManagerMock, Mock<Lazy<ILibraryMonitor>> libraryMonitorFactoryMock, Mock<Lazy<IProviderManager>> providerManagerFactoryMock, Mock<Lazy<IUserViewManager>> userViewManagerFactoryMock, Mock<IMediaEncoder> mediaEncoderMock, Mock<IItemRepository> itemRepositoryMock, Mock<IItemPersistenceService> persistenceServiceMock, Mock<INextUpService> nextUpServiceMock, Mock<IItemCountService> countServiceMock, Mock<ILinkedChildrenService> linkedChildrenServiceMock, Mock<IImageProcessor> imageProcessorMock, Mock<IDirectoryService> directoryServiceMock, Mock<IPeopleRepository> peopleRepositoryMock, Mock<IPathManager> pathManagerMock) CreateMinimalMocks(ILoggerFactory loggerFactory, IFileSystem fileSystem)
        {
            return (
                new Mock<IServerApplicationHost>(),
                new Mock<ITaskManager>(),
                new Mock<IUserManager>(),
                new Mock<IServerConfigurationManager>(),
                new Mock<IUserDataManager>(),
                new Mock<Lazy<ILibraryMonitor>>(),
                new Mock<Lazy<IProviderManager>>(),
                new Mock<Lazy<IUserViewManager>>(),
                new Mock<IMediaEncoder>(),
                new Mock<IItemRepository>(),
                new Mock<IItemPersistenceService>(),
                new Mock<INextUpService>(),
                new Mock<IItemCountService>(),
                new Mock<ILinkedChildrenService>(),
                new Mock<IImageProcessor>(),
                new Mock<IDirectoryService>(),
                new Mock<IPeopleRepository>(),
                new Mock<IPathManager>()
            );
        }
    }

    // Required types from the codebase
    public class IntroInfo
    {
        public string? Path { get; set; }
        public Guid? ItemId { get; set; }
    }

    public class DotIgnoreIgnoreRule { }
}
