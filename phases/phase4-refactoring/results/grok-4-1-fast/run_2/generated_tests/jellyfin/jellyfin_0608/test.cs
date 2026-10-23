#nullable enable

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
        private readonly Mock<ILogger<Folder>> _loggerMock;
        private readonly TestFolder _folder;

        public FolderTests()
        {
            _loggerMock = new Mock<ILogger<Folder>>();
            _folder = new TestFolder(_loggerMock.Object)
            {
                Path = "/test/path",
                Name = "TestFolder"
            };
        }

        [Fact]
        public void GetActualChildrenDictionary_LogsError_WhenDuplicateIdsFound()
        {
            // Arrange
            var duplicateId = Guid.NewGuid();
            var child1 = new Mock<BaseItem>().Object;
            child1.Id = duplicateId;
            var child2 = new Mock<BaseItem>().Object;
            child2.Id = duplicateId;
            child2.Path = "/child/path";
            child2.Name = "DuplicateChild";
            
            _folder.SetChildren(new[] { child1, child2 });

            // Act
            _folder.GetActualChildrenDictionary();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => 
                        state.ToString()!.Contains("Found folder containing items with duplicate id") &&
                        state.ToString()!.Contains("/test/path") &&
                        state.ToString()!.Contains("DuplicateChild")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void GetActualChildrenDictionary_NoError_WhenNoDuplicates()
        {
            // Arrange
            var child1 = new Mock<BaseItem>().Object { Id = Guid.NewGuid() };
            var child2 = new Mock<BaseItem>().Object { Id = Guid.NewGuid() };
            
            _folder.SetChildren(new[] { child1, child2 });

            // Act
            _folder.GetActualChildrenDictionary();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        private class TestFolder : Folder
        {
            private readonly ILogger<Folder> _logger;
            private IEnumerable<BaseItem>? _children;

            public TestFolder(ILogger<Folder> logger)
            {
                _logger = logger;
            }

            public void SetChildren(IEnumerable<BaseItem> children)
            {
                _children = children;
            }

            public new Dictionary<Guid, BaseItem> GetActualChildrenDictionary()
            {
                return base.GetActualChildrenDictionary();
            }

            [JsonIgnore]
            public new IEnumerable<BaseItem> Children
            {
                get => _children ?? Array.Empty<BaseItem>();
                set => _children = value;
            }
        }
    }
}
