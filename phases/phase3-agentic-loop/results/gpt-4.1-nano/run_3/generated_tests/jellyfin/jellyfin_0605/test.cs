using System;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using MediaBrowser.Controller.Entities;

namespace Jellyfin.Tests
{
    public class BaseItemLoggingTests
    {
        [Fact]
        public void FindLinkedChild_Should_LogWarning_When_ItemById_ReturnsNull_For_ItemId()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockLibraryManager = new Mock<ILibraryManager>();
            var baseItem = new TestBaseItem(mockLogger.Object, mockLibraryManager.Object);
            var linkedChild = new LinkedChild { ItemId = Guid.NewGuid() };
            mockLibraryManager.Setup(m => m.GetItemById(linkedChild.ItemId.Value)).Returns((BaseItem)null);
            baseItem.LibraryManager = mockLibraryManager.Object;

            // Act
            var result = baseItem.InvokeFindLinkedChild(linkedChild);

            // Assert
            mockLogger.Verify(
                x => x.LogWarning("Unable to find linked item by ItemId {0}", linkedChild.ItemId),
                Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void FindLinkedChild_Should_LogWarning_When_ItemByPath_ReturnsNull()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockLibraryManager = new Mock<ILibraryManager>();
            var baseItem = new TestBaseItem(mockLogger.Object, mockLibraryManager.Object);
            var info = new LinkedChild { Path = "somepath" };
            var absolutePath = "/abs/path";
            var pathMaker = new Mock<IFileSystem>();
            pathMaker.Setup(p => p.MakeAbsolutePath(It.IsAny<string>(), info.Path)).Returns(absolutePath);
            mockLibraryManager.Setup(m => m.FindByPath(absolutePath, null)).Returns((BaseItem)null);
            baseItem.LibraryManager = mockLibraryManager.Object;
            baseItem.FileSystem = pathMaker.Object;

            // Act
            var result = baseItem.InvokeFindLinkedChild(info);

            // Assert
            mockLogger.Verify(
                x => x.LogWarning("Unable to find linked item at path {0}", info.Path),
                Times.Once);
            Assert.Null(result);
        }
    }

    // Helper classes to facilitate testing
    public class TestBaseItem : BaseItem
    {
        private readonly ILogger _logger;
        public ILibraryManager LibraryManager { get; set; }
        public IFileSystem FileSystem { get; set; }

        public TestBaseItem(ILogger logger, ILibraryManager libraryManager)
        {
            _logger = logger;
            LibraryManager = libraryManager;
            FileSystem = new FileSystem(); // default implementation
        }

        public BaseItem InvokeFindLinkedChild(LinkedChild info)
        {
            return FindLinkedChild(info);
        }

        protected override ILogger Logger => _logger;
        protected override ILibraryManager LibraryManager => this.LibraryManager;
        protected override IFileSystem FileSystem => this.FileSystem;
    }

    // Mocked or simplified classes/interfaces
    public class LinkedChild
    {
        public Guid? ItemId { get; set; }
        public string Path { get; set; }
        public string LibraryItemId { get; set; }
    }

    public interface ILibraryManager
    {
        BaseItem GetItemById(Guid id);
        BaseItem FindByPath(string path, object options);
    }

    public interface IFileSystem
    {
        string MakeAbsolutePath(string containingFolderPath, string path);
    }

    public class FileSystem : IFileSystem
    {
        public string MakeAbsolutePath(string containingFolderPath, string path)
        {
            return Path.Combine(containingFolderPath, path);
        }
    }
}
