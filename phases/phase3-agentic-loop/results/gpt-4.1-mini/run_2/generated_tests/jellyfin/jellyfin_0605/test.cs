using System;
using System.Reflection;
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
                var method = typeof(BaseItem).GetMethod("FindLinkedChild", BindingFlags.NonPublic | BindingFlags.Instance);
                return (BaseItem)method.Invoke(this, new object[] { info });
            }
        }

        [Fact]
        public void FindLinkedChild_LogsWarning_WhenItemIdNotFound()
        {
            var libraryManagerMock = new Mock<ILibraryManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var loggerMock = new Mock<ILogger>();

            var guid = Guid.NewGuid();

            libraryManagerMock.Setup(x => x.GetItemById(guid)).Returns((BaseItem)null);

            var item = new TestBaseItem(libraryManagerMock.Object, fileSystemMock.Object, loggerMock.Object);

            var linkedChild = new LinkedChild
            {
                ItemId = guid
            };

            var result = item.CallFindLinkedChild(linkedChild);

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
            var libraryManagerMock = new Mock<ILibraryManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var loggerMock = new Mock<ILogger>();

            var path = "relative/path";

            fileSystemMock.Setup(x => x.MakeAbsolutePath("/base/path", path)).Returns("/base/path/relative/path");
            libraryManagerMock.Setup(x => x.FindByPath("/base/path/relative/path", null)).Returns((BaseItem)null);

            var item = new TestBaseItem(libraryManagerMock.Object, fileSystemMock.Object, loggerMock.Object);

            var linkedChild = new LinkedChild
            {
                Path = path
            };

            var result = item.CallFindLinkedChild(linkedChild);

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
            var libraryManagerMock = new Mock<ILibraryManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var loggerMock = new Mock<ILogger>();

            var libraryItemId = "libraryItemId";

            libraryManagerMock.Setup(x => x.GetItemById(libraryItemId)).Returns((BaseItem)null);

            var item = new TestBaseItem(libraryManagerMock.Object, fileSystemMock.Object, loggerMock.Object);

            var linkedChild = new LinkedChild
            {
                LibraryItemId = libraryItemId
            };

            var result = item.CallFindLinkedChild(linkedChild);

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

    // Minimal LinkedChild class for testing
    public class LinkedChild
    {
        public Guid? ItemId { get; set; }
        public string Path { get; set; }
        public string LibraryItemId { get; set; }
    }
}
