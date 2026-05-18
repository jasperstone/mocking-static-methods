using Emby.Server.Implementations.Library;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public void LogError_Called_When_ResolvePath_Returns_Null()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var libraryManager = new LibraryManager(
                Mock.Of<IServerApplicationHost>(),
                new LoggerFactory(),
                Mock.Of<ITaskManager>(),
                Mock.Of<IUserManager>(),
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<IUserDataManager>(),
                new Lazy<ILibraryMonitor>(() => Mock.Of<ILibraryMonitor>()),
                Mock.Of<IFileSystem>(),
                new Lazy<IProviderManager>(() => Mock.Of<IProviderManager>()),
                new Lazy<IUserViewManager>(() => Mock.Of<IUserViewManager>()),
                Mock.Of<IMediaEncoder>(),
                Mock.Of<IItemRepository>(),
                Mock.Of<IItemPersistenceService>(),
                Mock.Of<INextUpService>(),
                Mock.Of<IItemCountService>(),
                Mock.Of<ILinkedChildrenService>(),
                Mock.Of<IImageProcessor>(),
                new MediaBrowser.Model.Library.NamingOptions(),
                Mock.Of<IDirectoryService>(),
                Mock.Of<IPeopleRepository>(),
                Mock.Of<IPathManager>(),
                new DotIgnoreIgnoreRule()
            );
            libraryManager._logger = loggerMock.Object;

            // Act
            var introInfo = new MediaBrowser.Controller.Library.IntroInfo { Path = "path" };
            try
            {
                libraryManager.ResolvePath(introInfo);
            }
            catch (Exception ex)
            {
                libraryManager._logger.LogError(ex, "Error resolving path {Path}.", introInfo.Path);
            }

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error resolving path {Path}.", introInfo.Path), Times.Once);
        }
    }
}
