using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using System;

namespace MediaBrowser.Tests.Entities
{
    public class BaseItemTests
    {
        private class TestBaseItem : BaseItem
        {
            public override bool SupportsAddingToPlaylist => false;
        }

        [Fact]
        public void FindLinkedChild_ShouldLogWarning_WhenItemNotFoundByPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var containingFolderPath = "C:\\Media";

            // Setup LibraryManager to return null for GetItemById and FindByPath
            libraryManagerMock.Setup(m => m.GetItemById(It.IsAny<Guid>())).Returns((BaseItem)null);
            libraryManagerMock.Setup(m => m.FindByPath(It.IsAny<string>(), null)).Returns((BaseItem)null);

            var linkedChild = new LinkedChild
            {
                ItemId = null,
                Path = "somepath",
                LibraryItemId = null
            };

            var item = new TestBaseItem
            {
                LibraryManager = libraryManagerMock.Object,
                Logger = loggerMock.Object,
                ContainingFolderPath = containingFolderPath
            };

            // Act
            var result = item.FindLinkedChild(linkedChild);

            // Assert
            Assert.Null(result);
            loggerMock.Verify(
                x => x.LogWarning("Unable to find linked item at path {0}", linkedChild.Path),
                Times.Once);
        }
    }
}
