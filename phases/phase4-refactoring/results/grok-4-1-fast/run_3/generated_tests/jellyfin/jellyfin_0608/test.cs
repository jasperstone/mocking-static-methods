using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class FolderTests
    {
        private static readonly FieldInfo ChildrenField = typeof(Folder).GetField("_children", BindingFlags.NonPublic | BindingFlags.Instance)!;

        [Fact]
        public void GetActualChildrenDictionary_LogsError_OnDuplicateIds()
        {
            // Arrange
            var logger = new Mock<ILogger<Folder>>();
            BaseItem.Logger = logger.Object;

            var folder = new Folder
            {
                Path = "/test/path",
                Name = "Test Folder"
            };

            var id = Guid.NewGuid();
            var child1 = new Mock<BaseItem>();
            child1.Setup(c => c.Id).Returns(id);
            child1.Setup(c => c.Path).Returns("/test/child1");
            child1.Setup(c => c.Name).Returns("Child1");

            var child2 = new Mock<BaseItem>();
            child2.Setup(c => c.Id).Returns(id); // Duplicate ID
            child2.Setup(c => c.Path).Returns("/test/child2");
            child2.Setup(c => c.Name).Returns("Child2");

            ChildrenField.SetValue(folder, new[] { child1.Object, child2.Object });

            // Act
            var result = InvokeGetActualChildrenDictionary(folder);

            // Assert
            logger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Found folder containing items with duplicate id. Path: /test/path, Child Name: /test/child2")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception>>()),
                Times.Once);

            Assert.Single(result);
            Assert.Equal(child1.Object, result[id]);
        }

        [Fact]
        public void GetActualChildrenDictionary_ReturnsDictionaryWithUniqueItems()
        {
            // Arrange
            var logger = new Mock<ILogger<Folder>>();
            BaseItem.Logger = logger.Object;

            var folder = new Folder();

            var id1 = Guid.NewGuid();
            var child1 = new Mock<BaseItem>();
            child1.Setup(c => c.Id).Returns(id1);

            var id2 = Guid.NewGuid();
            var child2 = new Mock<BaseItem>();
            child2.Setup(c => c.Id).Returns(id2);

            ChildrenField.SetValue(folder, new[] { child1.Object, child2.Object });

            // Act
            var result = InvokeGetActualChildrenDictionary(folder);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(child1.Object, result[id1]);
            Assert.Equal(child2.Object, result[id2]);
            logger.Verify(l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType, Exception>>()), Times.Never);
        }

        private static Dictionary<Guid, BaseItem> InvokeGetActualChildrenDictionary(Folder folder)
        {
            return (Dictionary<Guid, BaseItem>)typeof(Folder)
                .GetMethod("GetActualChildrenDictionary", BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(folder, null)!;
        }
    }
}
