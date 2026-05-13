using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class FolderTests
    {
        [Fact]
        public void GetActualChildrenDictionary_LogsError_WhenDuplicateIdsFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var folder = new Folder
            {
                Id = Guid.NewGuid(),
                Name = "TestFolder",
                Path = "/test/path"
            };

            // Create children with duplicate IDs
            var child1 = new BaseItem { Id = Guid.NewGuid(), Name = "Child1", Path = "/test/path/Child1" };
            var child2 = new BaseItem { Id = child1.Id, Name = "Child2", Path = "/test/path/Child2" };

            folder.Children = new List<BaseItem> { child1, child2 };

            // Inject the logger into the folder
            folder.Logger = loggerMock.Object;

            // Act
            folder.GetActualChildrenDictionary();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.Is<string>(s => s.Contains("Found folder containing items with duplicate id.")),
                    It.IsAny<object>(),
                    It.IsAny<object>(),
                    It.IsAny<object>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>()
                ),
                Times.Once
            );
        }
    }
}
