using System;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class BaseItemTests
    {
        private class TestBaseItem : BaseItem
        {
            public TestBaseItem(ILibraryManager libraryManager, IFileSystem fileSystem, ILogger logger)
            {
                LibraryManager = libraryManager;
                FileSystem = fileSystem;
                Logger = logger;
                ContainingFolderPath = "/base/path";
            }

            public new ILibraryManager LibraryManager { get; }
            public new IFileSystem FileSystem { get; }
            public new ILogger Logger { get; }
            public new string ContainingFolderPath { get; set; }

            public BaseItem CallFindLinkedChild(LinkedChild info)
            {
                return FindLinkedChild(info);
            }
        }

        [Fact]
        public void FindLinkedChild_LogsWarning_WhenItemIdNotFound()
        {
            var libraryManagerMock = new Mock<ILibraryManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var loggerMock = new Mock<ILogger>();

            var testItem = new TestBaseItem(libraryManagerMock.Object, fileSystemMock.Object, loggerMock.Object);

            var guid = Guid.NewGuid();
            var linkedChild = new LinkedChild { ItemId = guid };

            libraryManagerMock.Setup(x => x.GetItemById(guid)).Returns((BaseItem)null);

            var result = testItem.CallFindLinkedChild(linkedChild);

            Assert.Null(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(guid.ToString())),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void FindLinkedChild_LogsWarning_WhenPathNotFound()
        {
            var libraryManagerMock = new Mock<ILibraryManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var loggerMock = new Mock<ILogger>();

            var testItem = new TestBaseItem(libraryManagerMock.Object, fileSystemMock.Object, loggerMock.Object);

            var path = "relative/path";
            var absolutePath = "/base/path/relative/path";

            var linkedChild = new LinkedChild { Path = path };

            fileSystemMock.Setup(x => x.MakeAbsolutePath("/base/path", path)).Returns(absolutePath);
            libraryManagerMock.Setup(x => x.FindByPath(absolutePath, null)).Returns((BaseItem)null);

            var result = testItem.CallFindLinkedChild(linkedChild);

            Assert.Null(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(path)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void FindLinkedChild_LogsWarning_WhenLibraryItemIdNotFound()
        {
            var libraryManagerMock = new Mock<ILibraryManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var loggerMock = new Mock<ILogger>();

            var testItem = new TestBaseItem(libraryManagerMock.Object, fileSystemMock.Object, loggerMock.Object);

            var libraryItemId = "libraryItemId123";
            var linkedChild = new LinkedChild { LibraryItemId = libraryItemId };

            libraryManagerMock.Setup(x => x.GetItemById(libraryItemId)).Returns((BaseItem)null);

            var result = testItem.CallFindLinkedChild(linkedChild);

            Assert.Null(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(libraryItemId)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Minimal LinkedChild class for testing
    public class LinkedChild
    {
        public Guid? ItemId { get; set; }
        public string Path { get; set; }
        public string LibraryItemId { get; set; }
    }
}
