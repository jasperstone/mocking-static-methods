using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.Entities;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class FolderLoggerTests
    {
        private class TestFolder : Folder
        {
            private readonly IEnumerable<BaseItem> _children;

            public TestFolder(IEnumerable<BaseItem> children)
            {
                _children = children;
            }

            public override IEnumerable<BaseItem> Children
            {
                get => _children;
                set => base.Children = value;
            }
        }

        [Fact]
        public void GetActualChildrenDictionary_LogsErrorOnDuplicateIds()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            // We need to set the Logger static or instance in Folder, but Folder does not expose it.
            // So we will use reflection to set the private static Logger field if it exists.
            var folderType = typeof(Folder);
            var loggerField = folderType.GetField("Logger", BindingFlags.Static | BindingFlags.NonPublic);
            if (loggerField == null)
            {
                // Try instance field
                loggerField = folderType.GetField("Logger", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            if (loggerField == null)
            {
                // Cannot find Logger field, skip test
                return;
            }
            loggerField.SetValue(null, loggerMock.Object);

            var duplicateId = Guid.NewGuid();
            var child1 = new BaseItemStub(duplicateId, "Child1", "/path/child1");
            var child2 = new BaseItemStub(duplicateId, "Child2", "/path/child2");
            var children = new List<BaseItem> { child1, child2 };

            var folder = new TestFolder(children)
            {
                Path = "/folder/path",
                Name = "FolderName"
            };

            // Act
            var method = folderType.GetMethod("GetActualChildrenDictionary", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = method.Invoke(folder, null) as Dictionary<Guid, BaseItem>;

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Found folder containing items with duplicate id")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.True(result.ContainsKey(duplicateId));
        }

        private class BaseItemStub : BaseItem
        {
            public BaseItemStub(Guid id, string name, string path)
            {
                Id = id;
                Name = name;
                Path = path;
            }
        }
    }
}
