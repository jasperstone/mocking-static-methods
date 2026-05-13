using System;
using System.Collections.Generic;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
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

            public new ILibraryManager LibraryManager { get; set; }
            public new IFileSystem FileSystem { get; set; }
            public new ILogger Logger { get; set; }
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

            var testItem = new TestBaseItem(libraryManagerMock.Object, fileSystemMock.Object, loggerMock.Object, "/base/path");

            var guid = Guid.NewGuid();
            var linkedChild = new LinkedChild { ItemId = guid };

            libraryManagerMock.Setup(x => x.GetItemById(guid)).Returns((BaseItem)null);

            var result = testItem.CallFindLinkedChild(linkedChild);

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

            var testItem = new TestBaseItem(libraryManagerMock.Object, fileSystemMock.Object, loggerMock.Object, "/base/path");

            var linkedChild = new LinkedChild { Path = "relative/path" };

            fileSystemMock.Setup(x => x.MakeAbsolutePath("/base/path", "relative/path")).Returns("/base/path/relative/path");
            libraryManagerMock.Setup(x => x.FindByPath("/base/path/relative/path", null)).Returns((BaseItem)null);

            var result = testItem.CallFindLinkedChild(linkedChild);

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

            var testItem = new TestBaseItem(libraryManagerMock.Object, fileSystemMock.Object, loggerMock.Object, "/base/path");

            var linkedChild = new LinkedChild { LibraryItemId = "library-id" };

            libraryManagerMock.Setup(x => x.GetItemById("library-id")).Returns((BaseItem)null);

            var result = testItem.CallFindLinkedChild(linkedChild);

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

    // Minimal interfaces for dependencies
    public interface ILibraryManager
    {
        BaseItem GetItemById(Guid id);
        BaseItem GetItemById(string id);
        BaseItem FindByPath(string path, object arg);
    }

    public interface IFileSystem
    {
        string MakeAbsolutePath(string basePath, string relativePath);
    }
}
