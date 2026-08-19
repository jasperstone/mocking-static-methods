using System;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
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
                ContainingFolderPath = "basepath";
            }

            public new ILibraryManager LibraryManager { get; }
            public new IFileSystem FileSystem { get; }
            public new ILogger Logger { get; }

            // We simulate the FindLinkedChild method by reflection since it's private and not virtual
            public BaseItem CallFindLinkedChild(LinkedChild info)
            {
                var method = typeof(BaseItem).GetMethod("FindLinkedChild", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                return (BaseItem)method.Invoke(this, new object[] { info });
            }
        }

        [Fact]
        public void FindLinkedChild_LogsWarning_WhenItemIdNotFound()
        {
            var libraryManagerMock = new Mock<ILibraryManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var loggerMock = new Mock<ILogger>();

            var linkedChild = new LinkedChild { ItemId = Guid.NewGuid() };

            libraryManagerMock.Setup(l => l.GetItemById(linkedChild.ItemId.Value)).Returns((BaseItem)null);

            var baseItem = new TestBaseItem(libraryManagerMock.Object, fileSystemMock.Object, loggerMock.Object);

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
            var libraryManagerMock = new Mock<ILibraryManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var loggerMock = new Mock<ILogger>();

            var linkedChild = new LinkedChild { Path = "somepath" };

            fileSystemMock.Setup(f => f.MakeAbsolutePath(It.IsAny<string>(), linkedChild.Path)).Returns("absPath");
            libraryManagerMock.Setup(l => l.FindByPath("absPath", null)).Returns((BaseItem)null);

            var baseItem = new TestBaseItem(libraryManagerMock.Object, fileSystemMock.Object, loggerMock.Object);

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
            var libraryManagerMock = new Mock<ILibraryManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var loggerMock = new Mock<ILogger>();

            var linkedChild = new LinkedChild { LibraryItemId = "libId" };

            // We cannot mock the extension method GetItemById(string) directly.
            // Instead, we mock the underlying method that the extension calls.
            // The extension method calls LibraryManager.GetItemById(Guid) after parsing the string.
            // So we setup GetItemById(Guid) to return null for any Guid.
            libraryManagerMock.Setup(l => l.GetItemById(It.IsAny<Guid>())).Returns((BaseItem)null);

            var baseItem = new TestBaseItem(libraryManagerMock.Object, fileSystemMock.Object, loggerMock.Object);

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
