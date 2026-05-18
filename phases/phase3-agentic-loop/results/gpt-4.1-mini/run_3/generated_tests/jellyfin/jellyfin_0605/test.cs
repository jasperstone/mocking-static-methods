using System;
using System.Reflection;
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
            public TestBaseItem()
            {
                ContainingFolderPath = "/base/path";
            }

            public void SetLibraryManager(object libraryManager)
            {
                var prop = typeof(BaseItem).GetProperty("LibraryManager", BindingFlags.NonPublic | BindingFlags.Instance);
                prop.SetValue(this, libraryManager);
            }

            public void SetFileSystem(object fileSystem)
            {
                var prop = typeof(BaseItem).GetProperty("FileSystem", BindingFlags.NonPublic | BindingFlags.Instance);
                prop.SetValue(this, fileSystem);
            }

            public void SetLogger(ILogger logger)
            {
                var prop = typeof(BaseItem).GetProperty("Logger", BindingFlags.NonPublic | BindingFlags.Instance);
                prop.SetValue(this, logger);
            }

            public string ContainingFolderPath { get; set; }

            public BaseItem CallFindLinkedChild(LinkedChild info)
            {
                var method = typeof(BaseItem).GetMethod("FindLinkedChild", BindingFlags.NonPublic | BindingFlags.Instance);
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

        [Fact]
        public void FindLinkedChild_LogsWarning_WhenItemIdNotFound()
        {
            var loggerMock = new Mock<ILogger>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var fileSystemMock = new Mock<IFileSystem>();

            var missingGuid = Guid.NewGuid();
            libraryManagerMock.Setup(x => x.GetItemById(missingGuid)).Returns((BaseItem)null);

            var item = new TestBaseItem();
            item.SetLibraryManager(libraryManagerMock.Object);
            item.SetFileSystem(fileSystemMock.Object);
            item.SetLogger(loggerMock.Object);

            var linkedChild = new LinkedChild { ItemId = missingGuid };

            var result = item.CallFindLinkedChild(linkedChild);

            Assert.Null(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(missingGuid.ToString())),
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

            var testPath = "relative/path";
            var absolutePath = "/base/path/relative/path";

            libraryManagerMock.Setup(x => x.GetItemById(It.IsAny<Guid>())).Returns((BaseItem)null);
            fileSystemMock.Setup(x => x.MakeAbsolutePath("/base/path", testPath)).Returns(absolutePath);
            libraryManagerMock.Setup(x => x.FindByPath(absolutePath, null)).Returns((BaseItem)null);

            var item = new TestBaseItem();
            item.SetLibraryManager(libraryManagerMock.Object);
            item.SetFileSystem(fileSystemMock.Object);
            item.SetLogger(loggerMock.Object);

            var linkedChild = new LinkedChild { Path = testPath };

            var result = item.CallFindLinkedChild(linkedChild);

            Assert.Null(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(testPath)),
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

            var validGuidString = Guid.NewGuid().ToString();

            libraryManagerMock.Setup(x => x.GetItemById(It.IsAny<Guid>())).Returns((BaseItem)null);
            libraryManagerMock.Setup(x => x.FindByPath(It.IsAny<string>(), null)).Returns((BaseItem)null);
            libraryManagerMock.Setup(x => x.GetItemById(validGuidString)).Returns((BaseItem)null);

            var item = new TestBaseItem();
            item.SetLibraryManager(libraryManagerMock.Object);
            item.SetFileSystem(fileSystemMock.Object);
            item.SetLogger(loggerMock.Object);

            var linkedChild = new LinkedChild { LibraryItemId = validGuidString };

            var result = item.CallFindLinkedChild(linkedChild);

            Assert.Null(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(validGuidString)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
