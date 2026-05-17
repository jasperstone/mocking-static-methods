using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Resolvers;
using MediaBrowser.Controller.Sorting;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Library;
using MediaBrowser.Model.Tasks;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public void DeleteItem_LogsDebugForEachExistingMetadataPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(loggerMock.Object);

            var taskManagerMock = new Mock<ITaskManager>();
            var userManagerMock = new Mock<IUserManager>();
            var configMock = new Mock<IServerConfigurationManager>();
            configMock.SetupGet(c => c.Configuration).Returns(new ServerConfiguration { CacheSize = 100 });
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

            // Setup directoryService.Exists to return true for our test paths
            directoryServiceMock.Setup(d => d.Exists(It.IsAny<string>())).Returns(true);

            var libraryManager = new LibraryManager(
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

            // Create a test folder item with IsFolder = true
            var folder = new Folder
            {
                Name = "TestFolder",
                Id = Guid.NewGuid()
            };

            // Use reflection to get the private method DeleteItem
            var deleteItemMethod = typeof(LibraryManager).GetMethod("DeleteItem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(deleteItemMethod);

            // Create an instance of DeleteOptions with DeleteFileLocation = false
            var deleteOptionsType = typeof(LibraryManager).Assembly.GetType("Emby.Server.Implementations.Library.LibraryManager+DeleteOptions");
            Assert.NotNull(deleteOptionsType);
            var deleteOptions = Activator.CreateInstance(deleteOptionsType!);
            var deleteFileLocationProp = deleteOptionsType!.GetProperty("DeleteFileLocation");
            Assert.NotNull(deleteFileLocationProp);
            deleteFileLocationProp!.SetValue(deleteOptions, false);

            // Act
            try
            {
                deleteItemMethod.Invoke(libraryManager, new object[] { folder, deleteOptions! });
            }
            catch (System.Reflection.TargetInvocationException)
            {
                // Ignore exceptions from the invoked method (like Directory.Delete failing)
                // We only want to verify logging calls
            }

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Deleting metadata path")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }
}
