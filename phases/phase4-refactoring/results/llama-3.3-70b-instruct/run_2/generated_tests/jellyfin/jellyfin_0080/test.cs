using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Resolvers;
using MediaBrowser.Controller.Sorting;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Library;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Tasks;
using Jellyfin.Data;
using Jellyfin.Data.Enums;
using Jellyfin.Database.Implementations.Entities;
using Jellyfin.Database.Implementations.Enums;
using Jellyfin.Extensions;
using MediaBrowser.Common.Extensions;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.LiveTv;

public class LibraryManagerTests
{
    [Fact]
    public void LogDebug_Call_LogsDebugMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LibraryManager>>();
        var libraryManager = new LibraryManager(
            new Mock<IServerApplicationHost>().Object,
            new Mock<ILoggerFactory>().Object,
            new Mock<ITaskManager>().Object,
            new Mock<IUserManager>().Object,
            new Mock<IServerConfigurationManager>().Object,
            new Mock<IUserDataManager>().Object,
            new Mock<Lazy<ILibraryMonitor>>().Object,
            new Mock<IFileSystem>().Object,
            new Mock<Lazy<IProviderManager>>().Object,
            new Mock<Lazy<IUserViewManager>>().Object,
            new Mock<IMediaEncoder>().Object,
            new Mock<IItemRepository>().Object,
            new Mock<IItemPersistenceService>().Object,
            new Mock<INextUpService>().Object,
            new Mock<IItemCountService>().Object,
            new Mock<ILinkedChildrenService>().Object,
            new Mock<IImageProcessor>().Object,
            new Mock<NamingOptions>().Object,
            new Mock<IDirectoryService>().Object,
            new Mock<IPeopleRepository>().Object,
            new Mock<IPathManager>().Object,
            new Mock<DotIgnoreIgnoreRule>().Object);

        var loggerField = typeof(LibraryManager).GetField("_logger", BindingFlags.NonPublic | BindingFlags.Instance);
        loggerField.SetValue(libraryManager, loggerMock.Object);

        // Act
        libraryManager._logger.LogDebug("Deleting metadata path, Type: {Type}, Name: {Name}, Path: {Path}, Id: {Id}", "Type", "Name", "Path", "Id");

        // Assert
        loggerMock.Verify(l => l.LogDebug("Deleting metadata path, Type: {Type}, Name: {Name}, Path: {Path}, Id: {Id}", It.IsAny<object[]>()), Times.Once);
    }
}
