using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class FolderTests
    {
        private class TestFolder : Folder
        {
            public TestFolder(ILogger logger)
            {
                Logger = logger;
            }

            public new ILogger Logger { get; }

            // Override the base Logger property to use the injected logger
            protected override ILogger Logger => Logger;

            // Expose the GetActualChildrenDictionary method for testing
            public Dictionary<Guid, BaseItem> CallGetActualChildrenDictionary()
            {
                return base.GetActualChildrenDictionary();
            }
        }

        private class TestBaseItem : BaseItem
        {
            public TestBaseItem(Guid id, string name, string path = null)
            {
                Id = id;
                Name = name;
                Path = path;
            }
        }

        [Fact]
        public void GetActualChildrenDictionary_LogsErrorOnDuplicateIds()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var folder = new TestFolder(loggerMock.Object)
            {
                Path = "/test/folder"
            };

            var duplicateId = Guid.NewGuid();

            var child1 = new TestBaseItem(duplicateId, "Child1", "/test/folder/child1");
            var child2 = new TestBaseItem(duplicateId, "Child2", "/test/folder/child2");

            folder.Children = new List<BaseItem> { child1, child2 };

            // Act
            var result = folder.CallGetActualChildrenDictionary();

            // Assert
            Assert.Single(result);
            Assert.True(result.ContainsKey(duplicateId));
            Assert.Equal(child1, result[duplicateId]);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Found folder containing items with duplicate id")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void GetActualChildrenDictionary_ReturnsDictionaryWithoutDuplicates()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var folder = new TestFolder(loggerMock.Object)
            {
                Path = "/test/folder"
            };

            var child1 = new TestBaseItem(Guid.NewGuid(), "Child1", "/test/folder/child1");
            var child2 = new TestBaseItem(Guid.NewGuid(), "Child2", "/test/folder/child2");

            folder.Children = new List<BaseItem> { child1, child2 };

            // Act
            var result = folder.CallGetActualChildrenDictionary();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(child1.Id, result.Keys);
            Assert.Contains(child2.Id, result.Keys);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        [Fact]
        public void GetActualChildrenDictionary_UsesFolderNameWhenPathIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var folder = new TestFolder(loggerMock.Object)
            {
                Path = null,
                Name = "FolderName"
            };

            var duplicateId = Guid.NewGuid();

            var child1 = new TestBaseItem(duplicateId, "Child1", null);
            var child2 = new TestBaseItem(duplicateId, "Child2", null);

            folder.Children = new List<BaseItem> { child1, child2 };

            // Act
            var result = folder.CallGetActualChildrenDictionary();

            // Assert
            Assert.Single(result);
            Assert.True(result.ContainsKey(duplicateId));
            Assert.Equal(child1, result[duplicateId]);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Path: FolderName") && v.ToString().Contains("Child Name: Child1")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
