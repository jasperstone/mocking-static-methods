using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;

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
            }

            public new ILibraryManager LibraryManager { get; }
            public new IFileSystem FileSystem { get; }
            public new ILogger Logger { get; }
        }

        private BaseItem CreateTestBaseItem(
            out Mock<ILibraryManager> libraryManagerMock,
            out Mock<IFileSystem> fileSystemMock,
            out Mock<ILogger> loggerMock)
        {
            libraryManagerMock = new Mock<ILibraryManager>();
            fileSystemMock = new Mock<IFileSystem>();
            loggerMock = new Mock<ILogger>();

            return new TestBaseItem(libraryManagerMock.Object, fileSystemMock.Object, loggerMock.Object);
        }

        private BaseItem InvokeFindLinkedChild(BaseItem item, LinkedChild info)
        {
            var method = typeof(BaseItem).GetMethod("FindLinkedChild", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method);
            return (BaseItem)method.Invoke(item, new object[] { info });
        }

        [Fact]
        public void FindLinkedChild_LogsWarning_WhenItemIdNotFound()
        {
            var item = CreateTestBaseItem(out var libraryManagerMock, out var fileSystemMock, out var loggerMock);

            var linkedChild = new LinkedChild
            {
                ItemId = Guid.NewGuid()
            };

            libraryManagerMock.Setup(x => x.GetItemById(linkedChild.ItemId.Value)).Returns((BaseItem)null);

            var result = InvokeFindLinkedChild(item, linkedChild);

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
            var item = CreateTestBaseItem(out var libraryManagerMock, out var fileSystemMock, out var loggerMock);

            var linkedChild = new LinkedChild
            {
                Path = "relative/path"
            };

            fileSystemMock.Setup(x => x.MakeAbsolutePath(It.IsAny<string>(), linkedChild.Path)).Returns("absolute/path");
            libraryManagerMock.Setup(x => x.FindByPath("absolute/path", null)).Returns((BaseItem)null);

            var result = InvokeFindLinkedChild(item, linkedChild);

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
            var item = CreateTestBaseItem(out var libraryManagerMock, out var fileSystemMock, out var loggerMock);

            var linkedChild = new LinkedChild
            {
                LibraryItemId = "some-library-id"
            };

            libraryManagerMock.Setup(x => x.GetItemById(linkedChild.LibraryItemId)).Returns((BaseItem)null);

            var result = InvokeFindLinkedChild(item, linkedChild);

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

    // Minimal IFileSystem interface for testing
    public interface IFileSystem
    {
        string MakeAbsolutePath(string basePath, string path);
    }
}
