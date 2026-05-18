using System;
using System.Collections.Generic;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Tests.Entities
{
    public class FolderTests
    {
        private readonly Mock<ILogger<Folder>> _loggerMock;
        private readonly TestFolder _folder;

        public FolderTests()
        {
            _loggerMock = new Mock<ILogger<Folder>>();
            _loggerMock.Setup(x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>())); // Suppress CA2254
            _folder = new TestFolder(_loggerMock.Object);
        }

        [Fact]
        public void GetActualChildrenDictionary_LogsErrorOnDuplicateIds_WithPath()
        {
            // Arrange
            _folder.Path = "/test/path";
            var child1 = new TestBaseItem { Id = Guid.NewGuid(), Name = "Child1", Path = "/test/child1" };
            var child2 = new TestBaseItem { Id = child1.Id, Name = "Child2", Path = "/test/child2" };
            _folder.SetChildren(new List<BaseItem> { child1, child2 });

            // Act
            _folder.InvokeGetActualChildrenDictionary();

            // Assert
            _loggerMock.Verify(
                x => x.LogError(
                    "Found folder containing items with duplicate id. Path: {Path}, Child Name: {ChildName}",
                    "/test/path",
                    "/test/child1"),
                Times.Once);
        }

        [Fact]
        public void GetActualChildrenDictionary_LogsErrorOnDuplicateIds_UsesFolderNameWhenPathNull()
        {
            // Arrange
            _folder.Path = null;
            _folder.Name = "TestFolder";
            var child1 = new TestBaseItem { Id = Guid.NewGuid(), Name = "Child1", Path = "/test/child1" };
            var child2 = new TestBaseItem { Id = child1.Id, Name = "Child2", Path = "/test/child2" };
            _folder.SetChildren(new List<BaseItem> { child1, child2 });

            // Act
            _folder.InvokeGetActualChildrenDictionary();

            // Assert
            _loggerMock.Verify(
                x => x.LogError(
                    "Found folder containing items with duplicate id. Path: {Path}, Child Name: {ChildName}",
                    "TestFolder",
                    "/test/child1"),
                Times.Once);
        }

        [Fact]
        public void GetActualChildrenDictionary_LogsErrorOnDuplicateIds_UsesChildNameWhenChildPathNull()
        {
            // Arrange
            _folder.Path = "/test/path";
            var child1 = new TestBaseItem { Id = Guid.NewGuid(), Name = "ChildWithNoPath", Path = null };
            var child2 = new TestBaseItem { Id = child1.Id, Name = "Child2", Path = "/test/child2" };
            _folder.SetChildren(new List<BaseItem> { child1, child2 });

            // Act
            _folder.InvokeGetActualChildrenDictionary();

            // Assert
            _loggerMock.Verify(
                x => x.LogError(
                    "Found folder containing items with duplicate id. Path: {Path}, Child Name: {ChildName}",
                    "/test/path",
                    "ChildWithNoPath"),
                Times.Once);
        }

        [Fact]
        public void GetActualChildrenDictionary_NoErrorWhenNoDuplicates()
        {
            // Arrange
            var child1 = new TestBaseItem { Id = Guid.NewGuid(), Name = "Child1" };
            var child2 = new TestBaseItem { Id = Guid.NewGuid(), Name = "Child2" };
            _folder.SetChildren(new List<BaseItem> { child1, child2 });

            // Act
            _folder.InvokeGetActualChildrenDictionary();

            // Assert
            _loggerMock.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }

    public class TestFolder : Folder
    {
        public TestFolder(ILogger<Folder> logger)
        {
            Logger = logger;
        }

        public void SetChildren(List<BaseItem> children)
        {
            Children = children;
        }

        public Dictionary<Guid, BaseItem> InvokeGetActualChildrenDictionary()
        {
            return ((Func<Dictionary<Guid, BaseItem>>)Delegate.CreateDelegate(typeof(Func<Dictionary<Guid, BaseItem>>), this, "GetActualChildrenDictionary"))();
        }
    }

    public class TestBaseItem : BaseItem
    {
        public TestBaseItem()
        {
            Id = Guid.NewGuid();
        }
    }
}
