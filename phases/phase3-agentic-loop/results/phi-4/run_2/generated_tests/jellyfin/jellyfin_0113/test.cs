using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

// Add necessary using directives
using Emby.Server.Implementations;
using MediaBrowser.Controller;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Tasks;
using MediaBrowser.Model.Entities;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Resolvers;
using MediaBrowser.Controller.Sorting;
using MediaBrowser.Model.Library;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Jellyfin.Data;
using MediaBrowser.Controller.Entities;

namespace Emby.Server.Tests.Library
{
    public class LibraryManagerTests
    {
        [Fact]
        public async Task LogWarningIsCalledWhenImageNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var taskManagerMock = new Mock<ITaskManager>();
            var userManagerMock = new Mock<IUserManager>();
            var userDataManagerMock = new Mock<IUserDataManager>();
            var configurationManagerMock = new Mock<IServerConfigurationManager>();
            var libraryMonitorFactoryMock = new Mock<Lazy<ILibraryMonitor>>();
            var fileSystemMock = new Mock<IFileSystem>();
            var providerManagerFactoryMock = new Mock<Lazy<IProviderManager>>();
            var userViewManagerFactoryMock = new Mock<Lazy<IUserViewManager>>();
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
            var dotIgnoreIgnoreRuleMock = new Mock<DotIgnoreIgnoreRule>();

            var libraryManager = new LibraryManager(
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<ILoggerFactory>(),
                taskManagerMock.Object,
                userManagerMock.Object,
                configurationManagerMock.Object,
                userDataManagerMock.Object,
                libraryMonitorFactoryMock.Object,
                fileSystemMock.Object,
                providerManagerFactoryMock.Object,
                userViewManagerFactoryMock.Object,
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
                dotIgnoreIgnoreRuleMock.Object);

            // Use reflection to set the private _logger field
            var loggerField = typeof(LibraryManager).GetField("_logger", BindingFlags.NonPublic | BindingFlags.Instance);
            loggerField.SetValue(libraryManager, loggerMock.Object);

            var item = new Item { Id = Guid.NewGuid() };
            var image = new Image { Path = "nonexistent.jpg" };

            // Act
            await libraryManager.ProcessImage(item, image);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(It.IsAny<Exception>(), "Image not found at {ImagePath}", image.Path),
                Times.Once);
        }
    }
}
