using System;
using System.Reflection;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class BaseItemFindLinkedChildTests
    {
        private static MethodInfo GetFindLinkedChildMethod()
        {
            var method = typeof(BaseItem).GetMethod("FindLinkedChild", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method);
            return method!;
        }

        private class DummyBaseItem : BaseItem
        {
            public DummyBaseItem(ILibraryManager libraryManager, ILogger logger)
            {
                LibraryManager = libraryManager;
                Logger = logger;
                ContainingFolderPath = "/base/path";
            }

            public ILibraryManager LibraryManager { get; }
            public ILogger Logger { get; }

            // We need to set ContainingFolderPath for MakeAbsolutePath call
            public string ContainingFolderPath { get; set; }

            // We need to override FileSystem property to provide MakeAbsolutePath
            public IFileSystem FileSystem { get; set; } = null!;
        }

        [Fact]
        public void FindLinkedChild_LogsWarning_WhenItemIdNotFound()
        {
            var libraryManagerMock = new Mock<ILibraryManager>();
            var loggerMock = new Mock<ILogger>();
            var fileSystemMock = new Mock<IFileSystem>();

            var linkedChild = new LinkedChild { ItemId = Guid.NewGuid() };

            libraryManagerMock.Setup(x => x.GetItemById(linkedChild.ItemId.Value)).Returns((BaseItem)null);

            var item = new DummyBaseItem(libraryManagerMock.Object, loggerMock.Object)
            {
                FileSystem = fileSystemMock.Object
            };

            var method = GetFindLinkedChildMethod();

            var result = method.Invoke(item, new object[] { linkedChild });

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
            var fileSystemMock = new Mock<IFileSystem>();

            var linkedChild = new LinkedChild { Path = "relative/path" };

            fileSystemMock.Setup(x => x.MakeAbsolutePath(It.IsAny<string>(), linkedChild.Path)).Returns("/base/path/relative/path");
            libraryManagerMock.Setup(x => x.FindByPath("/base/path/relative/path", null)).Returns((BaseItem)null);

            var item = new DummyBaseItem(libraryManagerMock.Object, loggerMock.Object)
            {
                FileSystem = fileSystemMock.Object
            };

            var method = GetFindLinkedChildMethod();

            var result = method.Invoke(item, new object[] { linkedChild });

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
            var fileSystemMock = new Mock<IFileSystem>();

            var linkedChild = new LinkedChild { LibraryItemId = "libid" };

            libraryManagerMock.Setup(x => x.GetItemById("libid")).Returns((BaseItem)null);

            var item = new DummyBaseItem(libraryManagerMock.Object, loggerMock.Object)
            {
                FileSystem = fileSystemMock.Object
            };

            var method = GetFindLinkedChildMethod();

            var result = method.Invoke(item, new object[] { linkedChild });

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
