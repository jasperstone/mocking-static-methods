using System;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class BaseItemTests
    {
        private class TestBaseItem : BaseItem
        {
            public TestBaseItem(ILibraryManager libraryManager, ILogger logger)
            {
                LibraryManager = libraryManager;
                Logger = logger;
                ContainingFolderPath = "/base/path";
            }

            public new BaseItem FindLinkedChild(LinkedChild info)
            {
                return base.FindLinkedChild(info);
            }

            // We override MakeAbsolutePath to simulate FileSystem.MakeAbsolutePath behavior
            protected virtual string MakeAbsolutePath(string basePath, string relativePath)
            {
                return System.IO.Path.Combine(basePath, relativePath);
            }
        }

        [Fact]
        public void FindLinkedChild_LogsWarning_WhenItemIdNotFound()
        {
            var libraryManagerMock = new Mock<ILibraryManager>();
            var loggerMock = new Mock<ILogger>();

            var testItem = new TestBaseItem(libraryManagerMock.Object, loggerMock.Object);

            var guid = Guid.NewGuid();
            var linkedChild = new LinkedChild { ItemId = guid };

            libraryManagerMock.Setup(x => x.GetItemById(guid)).Returns((BaseItem)null);

            var result = testItem.FindLinkedChild(linkedChild);

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
            var loggerMock = new Mock<ILogger>();

            var testItem = new TestBaseItem(libraryManagerMock.Object, loggerMock.Object);

            var path = "relative/path/to/item";
            var linkedChild = new LinkedChild { Path = path };

            // We simulate MakeAbsolutePath by overriding the method in TestBaseItem
            // But since the original code uses FileSystem.MakeAbsolutePath, we simulate by setting ContainingFolderPath and Path
            // We will patch LibraryManager.FindByPath to return null for the absolute path
            var absolutePath = System.IO.Path.Combine("/base/path", path);
            libraryManagerMock.Setup(x => x.FindByPath(absolutePath, null)).Returns((BaseItem)null);

            var result = testItem.FindLinkedChild(linkedChild);

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
            var loggerMock = new Mock<ILogger>();

            var testItem = new TestBaseItem(libraryManagerMock.Object, loggerMock.Object);

            var libraryItemId = "library-item-id";
            var linkedChild = new LinkedChild { LibraryItemId = libraryItemId };

            libraryManagerMock.Setup(x => x.GetItemById(libraryItemId)).Returns((BaseItem)null);

            var result = testItem.FindLinkedChild(linkedChild);

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
