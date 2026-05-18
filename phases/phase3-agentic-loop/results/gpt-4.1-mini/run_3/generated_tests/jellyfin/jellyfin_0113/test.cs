using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Resolvers;
using MediaBrowser.Controller.Sorting;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.IO;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        private class TestImage : MediaBrowser.Model.Entities.Image
        {
            public TestImage(string path, bool isLocalFile)
            {
                Path = path;
                IsLocalFile = isLocalFile;
            }
        }

        private class TestItem : BaseItem
        {
            public List<MediaBrowser.Model.Entities.Image> Images { get; } = new();

            public override int GetImageIndex(MediaBrowser.Model.Entities.Image image)
            {
                return Images.IndexOf(image);
            }
        }

        [Fact]
        public async Task LogsWarningWhenImageFileNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(loggerMock.Object);

            var taskManagerMock = new Mock<ITaskManager>();
            var userManagerMock = new Mock<IUserManager>();
            var configMock = new Mock<IServerConfigurationManager>();
            configMock.SetupGet(c => c.Configuration).Returns(new ServerConfiguration());
            var userDataManagerMock = new Mock<IUserDataManager>();
            var libraryMonitorFactoryMock = new Lazy<ILibraryMonitor>(() => null!);
            var fileSystemMock = new Mock<IFileSystem>();
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

            var libraryManager = new TestLibraryManager(
                appHost: null!,
                loggerFactory: loggerFactoryMock.Object,
                taskManager: taskManagerMock.Object,
                userManager: userManagerMock.Object,
                configurationManager: configMock.Object,
                userDataManager: userDataManagerMock.Object,
                libraryMonitorFactory: libraryMonitorFactoryMock,
                fileSystem: fileSystemMock.Object,
                providerManagerFactory: providerManagerFactoryMock,
                userViewManagerFactory: userViewManagerFactoryMock,
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
            var outdatedImage = new TestImage("nonexistent.jpg", true);
            item.Images.Add(outdatedImage);

            // Setup File.Exists to false for the image path
            fileSystemMock.Setup(fs => fs.FileExists(outdatedImage.Path)).Returns(false);

            // Setup imageProcessor to return dummy values
            imageProcessorMock.Setup(ip => ip.GetImageDimensions(item, It.IsAny<MediaBrowser.Model.Entities.Image>()))
                .Returns(new ImageDimensions(100, 100));
            imageProcessorMock.Setup(ip => ip.GetImageBlurHash(It.IsAny<string>(), It.IsAny<ImageDimensions>()))
                .Returns("blurhash");

            // Act
            await libraryManager.InvokeProcessOutdatedImagesAsync(item, new List<MediaBrowser.Model.Entities.Image> { outdatedImage });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Image not found at") && v.ToString().Contains(outdatedImage.Path)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private class TestLibraryManager : LibraryManager
        {
            private readonly IFileSystem _fileSystem;

            public TestLibraryManager(
                IServerApplicationHost appHost,
                ILoggerFactory loggerFactory,
                ITaskManager taskManager,
                IUserManager userManager,
                IServerConfigurationManager configurationManager,
                IUserDataManager userDataManager,
                Lazy<ILibraryMonitor> libraryMonitorFactory,
                IFileSystem fileSystem,
                Lazy<IProviderManager> providerManagerFactory,
                Lazy<IUserViewManager> userViewManagerFactory,
                IMediaEncoder mediaEncoder,
                IItemRepository itemRepository,
                IItemPersistenceService persistenceService,
                INextUpService nextUpService,
                IItemCountService countService,
                ILinkedChildrenService linkedChildrenService,
                IImageProcessor imageProcessor,
                NamingOptions namingOptions,
                IDirectoryService directoryService,
                IPeopleRepository peopleRepository,
                IPathManager pathManager,
                DotIgnoreIgnoreRule dotIgnoreIgnoreRule)
                : base(appHost, loggerFactory, taskManager, userManager, configurationManager, userDataManager,
                      libraryMonitorFactory, fileSystem, providerManagerFactory, userViewManagerFactory, mediaEncoder,
                      itemRepository, persistenceService, nextUpService, countService, linkedChildrenService,
                      imageProcessor, namingOptions, directoryService, peopleRepository, pathManager, dotIgnoreIgnoreRule)
            {
                _fileSystem = fileSystem;
            }

            public async Task InvokeProcessOutdatedImagesAsync(BaseItem item, List<MediaBrowser.Model.Entities.Image> outdated)
            {
                var method = typeof(LibraryManager).GetMethod("ProcessOutdatedImagesAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method == null)
                    throw new InvalidOperationException("Method ProcessOutdatedImagesAsync not found");

                var task = (Task)method.Invoke(this, new object[] { item, outdated });
                await task.ConfigureAwait(false);
            }

            // Override ConvertImageToLocal to return the same image
            private new async Task<MediaBrowser.Model.Entities.Image> ConvertImageToLocal(BaseItem item, MediaBrowser.Model.Entities.Image img, int index, bool flag)
            {
                await Task.Yield();
                return img;
            }
        }
    }
}
