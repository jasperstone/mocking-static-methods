using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using MediaBrowser.Controller.Entities;

public class FolderTests
{
    [Fact]
    public void GetActualChildrenDictionary_LogsError_WhenDuplicateIdFound()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<Folder>>();
        var folder = new TestFolder(loggerMock.Object);
        var childrenList = new List<BaseItem>
        {
            new TestBaseItem { Id = Guid.NewGuid() },
            new TestBaseItem { Id = Guid.NewGuid() },
            new TestBaseItem { Id = Guid.NewGuid() },
            new TestBaseItem { Id = Guid.NewGuid() },
            new TestBaseItem { Id = Guid.NewGuid() },
            new TestBaseItem { Id = Guid.NewGuid() },
            new TestBaseItem { Id = Guid.NewGuid() },
        };

        var child1 = new TestBaseItem { Id = Guid.NewGuid() };
        var child2 = new TestBaseItem { Id = child1.Id };

        childrenList.Add(child1);
        childrenList.Add(child2);

        folder.Children = childrenList;

        // Act
        folder.GetActualChildrenDictionary();

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    private class TestFolder : Folder
    {
        public TestFolder(ILogger<Folder> logger) : base()
        {
            Logger = logger;
        }

        public new IEnumerable<BaseItem> Children
        {
            get => base.Children;
            set => base.Children = value;
        }

        public Dictionary<Guid, BaseItem> GetActualChildrenDictionary()
        {
            var dictionary = new Dictionary<Guid, BaseItem>();

            foreach (var child in Children)
            {
                var id = child.Id;
                if (dictionary.ContainsKey(id))
                {
                    Logger.LogError(
                        "Found folder containing items with duplicate id. Path: {Path}, Child Name: {ChildName}",
                        Path ?? Name,
                        child.Path ?? child.Name);
                }
                else
                {
                    dictionary[id] = child;
                }
            }

            return dictionary;
        }
    }

    private class TestBaseItem : BaseItem
    {
        public Guid Id { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
    }
}
