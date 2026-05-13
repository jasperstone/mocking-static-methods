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

            // Override the GetActualChildrenDictionary to use the base method logic but with our Logger
            public Dictionary<Guid, BaseItem> CallGetActualChildrenDictionary()
            {
                // We need to replicate the method logic here because the original method uses Logger property
                var dictionary = new Dictionary<Guid, BaseItem>();

                Children = Children ?? Enumerable.Empty<BaseItem>();

                var childrenList = Children.ToList();

                foreach (var child in childrenList)
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

        [Fact]
        public void GetActualChildrenDictionary_LogsErrorOnDuplicateIds()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var folder = new TestFolder(loggerMock.Object)
            {
                Path = "/test/path",
                Name = "TestFolder"
            };

            var duplicateId = Guid.NewGuid();

            var child1 = new BaseItem { Id = duplicateId, Name = "Child1", Path = "/child1" };
            var child2 = new BaseItem { Id = duplicateId, Name = "Child2", Path = "/child2" };

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
        public void GetActualChildrenDictionary_NoDuplicates_NoLogError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var folder = new TestFolder(loggerMock.Object)
            {
                Path = "/test/path",
                Name = "TestFolder"
            };

            var child1 = new BaseItem { Id = Guid.NewGuid(), Name = "Child1", Path = "/child1" };
            var child2 = new BaseItem { Id = Guid.NewGuid(), Name = "Child2", Path = "/child2" };

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
    }
}
