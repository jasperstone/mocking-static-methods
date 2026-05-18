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
        public void GetActualChildrenDictionary_LogsError_WhenDuplicateIdFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var folder = new Folder
            {
                Path = "TestPath",
                Name = "TestName"
            };
            folder.Children = new List<BaseItem>
            {
                new TestBaseItem { Id = Guid.NewGuid(), Path = "ChildPath1", Name = "Child1" },
                new TestBaseItem { Id = folder.Children[0].Id, Path = "ChildPath2", Name = "Child2" } // Duplicate ID
            };

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

    public class TestBaseItem : BaseItem
    {
        public TestBaseItem()
        {
            // Initialize necessary properties
        }
    }
}
