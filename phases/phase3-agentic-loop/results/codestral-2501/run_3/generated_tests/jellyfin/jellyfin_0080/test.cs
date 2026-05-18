using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using System.Collections.Generic;
using System.IO;
using System;
using Emby.Server.Implementations.Library;
using MediaBrowser.Model.Library;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Sorting;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Tasks;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Controller.Resolvers;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Dto;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Entities;
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
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public class LibraryManagerTests
{
    [Fact]
    public void DeleteItem_LogsDebugMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LibraryManager>>();
        var fileSystemMock = new Mock<IFileSystem>();
        var itemRepositoryMock = new Mock<IItemRepository>();
        var persistenceServiceMock = new Mock<IItemPersistenceService>();
        var nextUpServiceMock = new Mock<INextUpService>();
        var countServiceMock = new Mock<IItemCountService>();
        var linkedChildrenServiceMock = new Mock<ILinkedChildrenService>();
        var imageProcessorMock = new Mock<IImageProcessor>();
        var pathManagerMock = new Mock<IPathManager>();
        var dotIgnoreIgnoreRuleMock = new Mock<DotIgnoreIgnoreRule>();

        var libraryManager = new LibraryManager(
            null,
            Mock.Of<ILoggerFactory>(),
            null,
            null,
            null,
            null,
            null,
            fileSystemMock.Object,
            null,
            null,
            null,
            itemRepositoryMock.Object,
            persistenceServiceMock.Object,
            nextUpServiceMock.Object,
            countServiceMock.Object,
            linkedChildrenServiceMock.Object,
            imageProcessorMock.Object,
            null,
            null,
            null,
            pathManagerMock.Object,
            dotIgnoreIgnoreRuleMock.Object);

        var item = new Video
        {
            Id = Guid.NewGuid(),
            Name = "Test Item",
            IsFolder = false
        };

        var metadataPath = "test/path";
        fileSystemMock.Setup(fs => fs.DirectoryExists(metadataPath)).Returns(true);

        // Act
        libraryManager.DeleteItem(item, new DeleteOptions());

        // Assert
        loggerMock.Verify(
            logger => logger.LogDebug(
                "Deleting metadata path, Type: {Type}, Name: {Name}, Path: {Path}, Id: {Id}",
                It.IsAny<object[]>()),
            Times.Once);
    }
}
