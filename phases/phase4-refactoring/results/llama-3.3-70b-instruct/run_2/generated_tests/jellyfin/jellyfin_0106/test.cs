using Emby.Server.Implementations.Library;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public void LogError_CalledWithExceptionAndMessage_LoggerLogsError()
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
                Mock.Of<MediaBrowser.Controller.MediaEncoding.IMediaEncoder>(),
                Mock.Of<MediaBrowser.Controller.IItemRepository>(),
                Mock.Of<MediaBrowser.Controller.IItemPersistenceService>(),
                Mock.Of<MediaBrowser.Controller.INextUpService>(),
                Mock.Of<MediaBrowser.Controller.IItemCountService>(),
                Mock.Of<MediaBrowser.Controller.ILinkedChildrenService>(),
                Mock.Of<MediaBrowser.Controller.Drawing.IImageProcessor>(),
                new MediaBrowser.Model.Configuration.NamingOptions(),
                Mock.Of<MediaBrowser.Controller.IO.IDirectoryService>(),
                Mock.Of<MediaBrowser.Controller.Entities.IPeopleRepository>(),
                Mock.Of<MediaBrowser.Controller.IO.IPathManager>(),
                new DotIgnoreIgnoreRule()
            );
            libraryManager._logger = loggerMock.Object;

            var exception = new Exception("Test exception");
            var message = "Test message";

            // Act
            libraryManager._logger.LogError(exception, message);

            // Assert
            loggerMock.Verify(l => l.LogError(exception, message), Times.Once);
        }
    }
}
