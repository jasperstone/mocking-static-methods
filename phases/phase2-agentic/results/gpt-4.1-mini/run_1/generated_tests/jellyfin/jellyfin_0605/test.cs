using System;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class BaseItemTests
    {
        private class TestBaseItem : BaseItem
        {
            public TestBaseItem(ILibraryManager libraryManager, IFileSystem fileSystem, ILogger logger, string containingFolderPath)
            {
                LibraryManager = libraryManager;
                FileSystem = fileSystem;
                Logger = logger;
                ContainingFolderPath = containingFolderPath;
            }

            public ILibraryManager LibraryManager { get; }
            public IFileSystem FileSystem { get; }
            public ILogger Logger { get; }
            public string ContainingFolderPath { get; }

            // Expose the private FindLinkedChild method for testing via reflection
            public BaseItem CallFindLinkedChild(LinkedChild info)
            {
                // Use reflection to call private method
                var method = typeof(BaseItem).GetMethod("FindLinkedChild", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                return (BaseItem)method.Invoke(this, new object[] { info });
            }
        }

        public interface ILibraryManager
        {
            BaseItem GetItemById(Guid id);
            BaseItem GetItemById(string id);
            BaseItem FindByPath(string path, object arg);
        }

        public interface IFileSystem
        {
            string MakeAbsolutePath(string basePath, string path);
        }

        public class LinkedChild
        {
            public Guid? ItemId { get; set; }
            public string Path { get; set; }
            public string LibraryItemId { get; set; }
        }

        [Fact]
        public void FindLinkedChild_LogsWarning_WhenItemIdNotFound()
        {
            var loggerMock = new Mock<ILogger>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var fileSystemMock = new Mock<IFileSystem>();

            var itemId = Guid.NewGuid();

            libraryManagerMock.Setup(x => x.GetItemById(itemId)).Returns((BaseItem)null);

            var baseItem = new TestBaseItem(libraryManagerMock.Object, fileSystemMock.Object, loggerMock.Object, "/base/path");

            var linkedChild = new LinkedChild { ItemId = itemId };

            var result = baseItem.CallFindLinkedChild(linkedChild);

            Assert.Null(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to find linked item by ItemId")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void FindLinkedChild_LogsWarning_WhenPathNotFound()
        {
            var loggerMock = new Mock<ILogger>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var fileSystemMock = new Mock<IFileSystem>();

            var path = "relative/path";

            fileSystemMock.Setup(x => x.MakeAbsolutePath("/base/path", path)).Returns("/base/path/relative/path");
            libraryManagerMock.Setup(x => x.FindByPath("/base/path/relative/path", null)).Returns((BaseItem)null);

            var baseItem = new TestBaseItem(libraryManagerMock.Object, fileSystemMock.Object, loggerMock.Object, "/base/path");

            var linkedChild = new LinkedChild { Path = path };

            var result = baseItem.CallFindLinkedChild(linkedChild);

            Assert.Null(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to find linked item at path")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void FindLinkedChild_LogsWarning_WhenLibraryItemIdNotFound()
        {
            var loggerMock = new Mock<ILogger>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var fileSystemMock = new Mock<IFileSystem>();

            var libraryItemId = "library-id";

            libraryManagerMock.Setup(x => x.GetItemById(libraryItemId)).Returns((BaseItem)null);

            var baseItem = new TestBaseItem(libraryManagerMock.Object, fileSystemMock.Object, loggerMock.Object, "/base/path");

            var linkedChild = new LinkedChild { LibraryItemId = libraryItemId };

            var result = baseItem.CallFindLinkedChild(linkedChild);

            Assert.Null(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to find linked item by LibraryItemId")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
