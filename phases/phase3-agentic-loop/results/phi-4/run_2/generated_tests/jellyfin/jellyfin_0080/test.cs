using Moq;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Jellyfin.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public void Should_LogDebug_When_Deleting_Metadata_Path()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var item = new Mock<Item>(); // Assuming Item is the type of 'item'
            var metadataPath = "/path/to/metadata";
            item.Setup(i => i.GetType().Name).Returns("ItemType");
            item.Setup(i => i.Name).Returns("ItemName");
            item.Setup(i => i.Id).Returns(1);

            var libraryManager = new LibraryManager(mockLogger.Object);
            libraryManager._logger = mockLogger.Object; // Assuming _logger is a field

            // Act
            libraryManager.DeleteMetadataPath(item.Object, metadataPath);

            // Assert
            mockLogger.Verify(
                l => l.LogDebug(
                    "Deleting metadata path, Type: {Type}, Name: {Name}, Path: {Path}, Id: {Id}",
                    It.Is<string>(t => t == "ItemType"),
                    It.Is<string>(n => n == "ItemName"),
                    It.Is<string>(p => p == metadataPath),
                    It.Is<int>(id => id == 1)
                ),
                Times.Once
            );
        }
    }
}
