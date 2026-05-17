using Emby.Server.Implementations.Library;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public void DeleteItem_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var libraryManager = new LibraryManager(
                Mock.Of<MediaBrowser.Controller.IServerApplicationHost>(),
                new LoggerFactory(),
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
                new DotIgnoreIgnoreRule()
            );
            libraryManager._logger = loggerMock.Object;

            var item = new MediaBrowser.Controller.Entities.Video
            {
                Id = Guid.NewGuid(),
                Name = "Test Video",
                Path = "/path/to/video"
            };

            // Act
            libraryManager.DeleteItem(item, new MediaBrowser.Model.Dto.DeleteOptions());

            // Assert
            loggerMock.Verify(l => l.LogDebug(
                "Deleting metadata path, Type: {Type}, Name: {Name}, Path: {Path}, Id: {Id}",
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Guid>()), Times.Once);
        }
    }
}
