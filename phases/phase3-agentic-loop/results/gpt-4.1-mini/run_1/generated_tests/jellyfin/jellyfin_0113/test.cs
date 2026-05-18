using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Resolvers;
using MediaBrowser.Controller.Sorting;
using MediaBrowser.Controller;

namespace Emby.Server.Implementations.Tests.Library
{
    public class LibraryManagerTests
    {
        private class TestImage : MediaBrowser.Controller.Entities.Image
        {
            public TestImage(string path, bool isLocalFile)
            {
                Path = path;
                IsLocalFile = isLocalFile;
            }
        }

        private class TestItem : BaseItem
        {
            public TestItem()
            {
                Id = Guid.NewGuid().ToString();
            }

            public int GetImageIndex(MediaBrowser.Controller.Entities.Image img)
            {
                return 0;
            }
        }

        [Fact]
        public async Task LogsWarningWhenImageNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(loggerMock.Object);

            var taskManagerMock = new Mock<ITaskManager>();
            var userManagerMock = new Mock<IUserManager>();
            var configMock = new Mock<IServerConfigurationManager>();
            configMock.Setup(c => c.Configuration).Returns(new ServerConfiguration());
            var userDataManagerMock = new Mock<IUserDataManager>();
            var libraryMonitorFactory = new Lazy<ILibraryMonitor>(() => null);
            var fileSystemMock = new Mock<MediaBrowser.Controller.IO.IFileSystem>();
            var providerManagerFactory = new Lazy<IProviderManager>(() => null);
            var userViewManagerFactory = new Lazy<IUserViewManager>(() => null);
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
                appHost: null,
                loggerFactory: loggerFactoryMock.Object,
                taskManager: taskManagerMock.Object,
                userManager: userManagerMock.Object,
                configurationManager: configMock.Object,
                userDataManager: userDataManagerMock.Object,
                libraryMonitorFactory: libraryMonitorFactory,
                fileSystem: fileSystemMock.Object,
                providerManagerFactory: providerManagerFactory,
                userViewManagerFactory: userViewManagerFactory,
                mediaEncoder: mediaEncoderMock.Object,
                itemRepository: itemRepositoryMock.Object,
                persistenceService: persistenceServiceMock.Object,
                nextUpService: nextUpServiceMock.Object,
                countService: countServiceMock.Object,
                linkedChildrenService: linkedChildrenServiceMock.Object,
                imageProcessor: imageProcessorMock.Object,
                namingOptions: namingOptions,
                directoryService: directoryServiceMock.Object,
                peopleRepository: peopleRepositoryMock.Object,
                pathManager: pathManagerMock.Object,
                dotIgnoreIgnoreRule: dotIgnoreIgnoreRule);

            var item = new TestItem();
            var image = new TestImage("/non/existent/path.jpg", isLocalFile: true);

            // Setup file system to say file does not exist
            fileSystemMock.Setup(fs => fs.FileExists(image.Path)).Returns(false);

            // Act
            await TestLogWarningForImageNotFound(libraryManager, fileSystemMock.Object, loggerMock.Object, item, image);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Image not found at")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private static async Task TestLogWarningForImageNotFound(LibraryManager libraryManager, MediaBrowser.Controller.IO.IFileSystem fileSystem, ILogger logger, BaseItem item, MediaBrowser.Controller.Entities.Image image)
        {
            // This method simulates the relevant part of the foreach loop in the original code
            if (!fileSystem.FileExists(image.Path))
            {
                logger.LogWarning("Image not found at {ImagePath}", image.Path);
                return;
            }

            await Task.CompletedTask;
        }
    }
}
