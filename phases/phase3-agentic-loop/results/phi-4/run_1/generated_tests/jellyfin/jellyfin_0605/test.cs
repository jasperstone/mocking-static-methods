using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Controller.Library; // Ensure this is the correct namespace
using System;

namespace MediaBrowser.Tests.Controller.Entities
{
    public class BaseItemTests
    {
        [Fact]
        public void FindLinkedChild_LogsWarning_WhenItemIdNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var baseItem = new BaseItem
            {
                Logger = loggerMock.Object
            };

            // Use a wrapper to inject the library manager
            var libraryManagerWrapper = new LibraryManagerWrapper(libraryManagerMock.Object);
            baseItem.LibraryManager = libraryManagerWrapper;

            var linkedChild = new LinkedChild
            {
                ItemId = Guid.NewGuid(),
                Path = null,
                LibraryItemId = null
            };

            libraryManagerMock.Setup(m => m.GetItemById(linkedChild.ItemId.Value)).Returns((BaseItem)null);

            // Act
            baseItem.FindLinkedChild(linkedChild);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains(linkedChild.ItemId.ToString())), linkedChild.ItemId), Times.Once);
        }

        [Fact]
        public void FindLinkedChild_LogsWarning_WhenPathNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var baseItem = new BaseItem
            {
                Logger = loggerMock.Object
            };

            var libraryManagerWrapper = new LibraryManagerWrapper(libraryManagerMock.Object);
            baseItem.LibraryManager = libraryManagerWrapper;

            var linkedChild = new LinkedChild
            {
                ItemId = null,
                Path = "some/path",
                LibraryItemId = null
            };

            libraryManagerMock.Setup(m => m.FindByPath(It.IsAny<string>(), null)).Returns((BaseItem)null);

            // Act
            baseItem.FindLinkedChild(linkedChild);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains(linkedChild.Path)), linkedChild.Path), Times.Once);
        }

        [Fact]
        public void FindLinkedChild_LogsWarning_WhenLibraryItemIdNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var baseItem = new BaseItem
            {
                Logger = loggerMock.Object
            };

            var libraryManagerWrapper = new LibraryManagerWrapper(libraryManagerMock.Object);
            baseItem.LibraryManager = libraryManagerWrapper;

            var linkedChild = new LinkedChild
            {
                ItemId = null,
                Path = null,
                LibraryItemId = "some-id"
            };

            libraryManagerMock.Setup(m => m.GetItemById(linkedChild.LibraryItemId)).Returns((BaseItem)null);

            // Act
            baseItem.FindLinkedChild(linkedChild);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains(linkedChild.LibraryItemId)), linkedChild.LibraryItemId), Times.Once);
        }

        // Wrapper class to inject the library manager
        private class LibraryManagerWrapper : ILibraryManager
        {
            private readonly ILibraryManager _libraryManager;

            public LibraryManagerWrapper(ILibraryManager libraryManager)
            {
                _libraryManager = libraryManager;
            }

            // Implement necessary methods of ILibraryManager
            public BaseItem GetItemById(Guid id) => _libraryManager.GetItemById(id);
            public BaseItem FindByPath(string path, object arg) => _libraryManager.FindByPath(path, arg);
        }
    }
}
