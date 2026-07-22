using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class FolderTests
    {
        private class TestBaseItem : BaseItem
        {
            public TestBaseItem(Guid id, string name, string path)
            {
                Id = id;
                Name = name;
                Path = path;
            }
        }

        private class TestFolder : Folder
        {
            public TestFolder()
            {
                // Set the TypeName or SourceType to avoid enum parse errors if needed
                SourceType = MediaBrowser.Model.Entities.SourceType.Library;
            }

            public Dictionary<Guid, BaseItem> CallGetActualChildrenDictionary()
            {
                var method = typeof(Folder).GetMethod("GetActualChildrenDictionary", BindingFlags.NonPublic | BindingFlags.Instance);
                return (Dictionary<Guid, BaseItem>)method.Invoke(this, null);
            }
        }

        [Fact]
        public void GetActualChildrenDictionary_WithDuplicateIds_ReturnsDictionaryWithOneEntry()
        {
            // Arrange
            var folder = new TestFolder();
            var duplicateId = Guid.NewGuid();

            var child1 = new TestBaseItem(duplicateId, "Child1", "/path/to/child1");
            var child2 = new TestBaseItem(duplicateId, "Child2", "/path/to/child2");

            folder.Children = new List<BaseItem> { child1, child2 };

            // Act
            var result = folder.CallGetActualChildrenDictionary();

            // Assert
            Assert.Single(result);
            Assert.True(result.ContainsKey(duplicateId));
        }
    }
}
