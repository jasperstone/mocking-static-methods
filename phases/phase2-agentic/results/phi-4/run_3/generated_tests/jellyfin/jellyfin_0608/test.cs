using System;
using System.Collections.Generic;
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
                Path = "/test/path",
                Name = "TestFolder"
            };
            folder.Logger = loggerMock.Object;

            var child1 = new BaseItem { Id = Guid.NewGuid(), Path = "/test/path/child1" };
            var child2 = new BaseItem { Id = child1.Id, Path = "/test/path/child2" };

            folder.Children = new List<BaseItem> { child1, child2 };

            // Act
            folder.GetActualChildrenDictionary();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.Is<string>(s => s.Contains("Found folder containing items with duplicate id.")),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ),
                Times.Once
            );
        }
    }
}
