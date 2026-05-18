using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Moq;
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

            var item = new BaseItem
            {
                Id = Guid.NewGuid(),
                Name = "Test Item",
                IsFolder = false
            };

            var metadataPath = "test/path";
            fileSystemMock.Setup(fs => fs.DirectoryExists(metadataPath)).Returns(true);

            // Act
            libraryManager.DeleteItem(item, new DeleteOptions(), CancellationToken.None);

            // Assert
            loggerMock.Verify(
                logger => logger.LogDebug(
                    "Deleting metadata path, Type: {Type}, Name: {Name}, Path: {Path}, Id: {Id}",
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
