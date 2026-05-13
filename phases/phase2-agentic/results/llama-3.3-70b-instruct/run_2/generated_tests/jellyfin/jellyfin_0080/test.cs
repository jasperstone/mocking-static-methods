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
        private readonly Mock<ILogger<LibraryManager>> _loggerMock;
        private readonly Mock<IItemRepository> _itemRepositoryMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly LibraryManager _libraryManager;

        public LibraryManagerTests()
        {
            _loggerMock = new Mock<ILogger<LibraryManager>>();
            _itemRepositoryMock = new Mock<IItemRepository>();
            _fileSystemMock = new Mock<IFileSystem>();

            _libraryManager = new LibraryManager(
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                _fileSystemMock.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                _loggerMock.Object);
        }

        [Fact]
        public void DeleteItem_LogsDebugMessage_WhenDeletingMetadataPath()
        {
            // Arrange
            var item = new BaseItem { Id = Guid.NewGuid(), Name = "Test Item" };
            var metadataPath = Path.Combine("path", "to", "metadata");
            _fileSystemMock.Setup(fs => fs.Directory.Exists(metadataPath)).Returns(true);

            // Act
            _libraryManager.DeleteItem(item, new DeleteOptions { DeleteFileLocation = true }, CancellationToken.None);

            // Assert
            _loggerMock.Verify(logger => logger.LogDebug(
                "Deleting metadata path, Type: {Type}, Name: {Name}, Path: {Path}, Id: {Id}",
                item.GetType().Name,
                item.Name,
                metadataPath,
                item.Id),
                Times.Once);
        }
    }
}
