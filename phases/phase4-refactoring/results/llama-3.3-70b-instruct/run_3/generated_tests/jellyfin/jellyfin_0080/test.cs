using Emby.Server.Implementations.Library;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public void LogDebug_CalledWithCorrectParameters()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var libraryManager = new LibraryManager(
                Mock.Of<MediaBrowser.Controller.IServerApplicationHost>(),
                Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>(),
                Mock.Of<MediaBrowser.Controller.ITaskManager>(),
                Mock.Of<MediaBrowser.Controller.IUserManager>(),
                Mock.Of<MediaBrowser.Controller.IServerConfigurationManager>(),
                Mock.Of<MediaBrowser.Controller.IUserDataManager>(),
                new Lazy<MediaBrowser.Controller.ILibraryMonitor>(() => Mock.Of<MediaBrowser.Controller.ILibraryMonitor>()),
                Mock.Of<MediaBrowser.Controller.IO.IFileSystem>(),
                new Lazy<MediaBrowser.Controller.IProviderManager>(() => Mock.Of<MediaBrowser.Controller.IProviderManager>()),
                new Lazy<MediaBrowser.Controller.IUserViewManager>(() => Mock.Of<MediaBrowser.Controller.IUserViewManager>()),
                Mock.Of<MediaBrowser.Controller.IMediaEncoder>(),
                Mock.Of<MediaBrowser.Controller.IItemRepository>(),
                Mock.Of<MediaBrowser.Controller.IItemPersistenceService>(),
                Mock.Of<MediaBrowser.Controller.INextUpService>(),
                Mock.Of<MediaBrowser.Controller.IItemCountService>(),
                Mock.Of<MediaBrowser.Controller.ILinkedChildrenService>(),
                Mock.Of<MediaBrowser.Controller.IImageProcessor>(),
                new MediaBrowser.Model.Configuration.NamingOptions(),
                Mock.Of<MediaBrowser.Controller.IDirectoryService>(),
                Mock.Of<MediaBrowser.Controller.IPeopleRepository>(),
                Mock.Of<MediaBrowser.Controller.IPathManager>(),
                new MediaBrowser.Controller.IO.DotIgnoreIgnoreRule()
            );
            libraryManager._logger = loggerMock.Object;

            var item = new MediaBrowser.Controller.Entities.BaseItem { Id = Guid.NewGuid(), Name = "Test Item" };
            var metadataPath = Path.GetTempFileName();

            // Act
            libraryManager.DeleteItem(item, new MediaBrowser.Model.Library.DeleteOptions());

            // Assert
            loggerMock.Verify(l => l.LogDebug(
                "Deleting metadata path, Type: {Type}, Name: {Name}, Path: {Path}, Id: {Id}",
                item.GetType().Name,
                item.Name,
                metadataPath,
                item.Id),
                Times.Once);
        }
    }
}
