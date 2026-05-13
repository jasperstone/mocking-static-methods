using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class FolderTests
    {
        private class TestBaseItem : BaseItem
        {
            public TestBaseItem(Guid id, string name, string path = null)
            {
                Id = id;
                Name = name;
                Path = path;
            }
        }

        private class TestFolder : Folder
        {
            public TestFolder(ILogger logger)
            {
                Logger = logger;
            }

            public override IEnumerable<BaseItem> Children
            {
                get => _children;
                set => _children = value;
            }

            public ILogger Logger { get; }

            // Expose the protected method for testing
            public Dictionary<Guid, BaseItem> CallGetActualChildrenDictionary()
            {
                return GetActualChildrenDictionary();
            }

            // Override to use the injected logger
            protected override ILogger Logger => Logger;
        }

        [Fact]
        public void GetActualChildrenDictionary_LogsErrorOnDuplicateIds()
        {
            // Arrange
            var loggerMock = new Moq.Mock<ILogger>();
            var folder = new TestFolder(loggerMock.Object)
            {
                Path = "/test/path",
                Name = "TestFolder"
            };

            var duplicateId = Guid.NewGuid();
            var child1 = new TestBaseItem(duplicateId, "Child1", "/child1/path");
            var child2 = new TestBaseItem(duplicateId, "Child2", "/child2/path");

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
                Moq.Times.Once);
        }
    }
}
