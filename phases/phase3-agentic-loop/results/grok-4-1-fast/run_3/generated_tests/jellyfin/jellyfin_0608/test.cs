using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class FolderTests
    {
        [Fact]
        public void GetActualChildrenDictionary_LogsErrorOnDuplicateId()
        {
            // Arrange
            var logger = new Mock<ILogger<Folder>>();
            var folder = new TestFolder(logger.Object);
            var child1 = new TestBaseItem { Id = Guid.NewGuid() };
            var child2 = new TestBaseItem { Id = child1.Id }; // Duplicate ID
            folder.SetChildren(new[] { child1, child2 });

            // Act
            folder.CallGetActualChildrenDictionary();

            // Assert
            logger.Verify(
                x => x.LogError(
                    "Found folder containing items with duplicate id. Path: {Path}, Child Name: {ChildName}",
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public void GetActualChildrenDictionary_NoErrorWhenNoDuplicates()
        {
            // Arrange
            var logger = new Mock<ILogger<Folder>>();
            var folder = new TestFolder(logger.Object);
            var child1 = new TestBaseItem { Id = Guid.NewGuid() };
            var child2 = new TestBaseItem { Id = Guid.NewGuid() }; // Unique ID
            folder.SetChildren(new[] { child1, child2 });

            // Act
            folder.CallGetActualChildrenDictionary();

            // Assert
            logger.Verify(
                x => x.LogError(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Never);
        }

        private class TestFolder : Folder
        {
            private readonly ILogger<Folder> _logger;
            private IEnumerable<BaseItem> _children;

            public TestFolder(ILogger<Folder> logger)
            {
                _logger = logger;
            }

            public ILogger<Folder> Logger => _logger;

            public void SetChildren(IEnumerable<BaseItem> children)
            {
                _children = children;
            }

            public override IEnumerable<BaseItem> Children
            {
                get => _children ?? Array.Empty<BaseItem>();
                set => _children = value;
            }

            public Dictionary<Guid, BaseItem> CallGetActualChildrenDictionary()
            {
                return base.GetActualChildrenDictionary();
            }
        }

        private class TestBaseItem : BaseItem
        {
            public TestBaseItem()
            {
                Id = Guid.NewGuid();
            }
        }
    }
}
