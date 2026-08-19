using Xunit;
using Moq;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Controller.Entities;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class FolderTests
    {
        [Fact]
        public void GetActualChildrenDictionary_LogsErrorForDuplicateIds()
        {
            // Arrange
            var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger>();
            var folder = new Folder(); 

            folder.Logger = loggerMock.Object;

            folder.Children = new List<BaseItem>
            {
                new Folder { Id = Guid.NewGuid() }, 
                new Folder { Id = Guid.NewGuid() },
                new Folder { Id = Guid.NewGuid() },
                new Folder { Id = Guid.NewGuid() },
                new Folder { Id = Guid.NewGuid() },
            };

            var firstChild = folder.Children.First();
            var lastChild = folder.Children.Last();
            lastChild.Id = firstChild.Id; 

            // Act
            folder.GetActualChildrenDictionary();

            // Assert
            loggerMock.Verify(
                logger => logger.LogError(
                    "Found folder containing items with duplicate id. Path: {Path}, Child Name: {ChildName}",
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Once);
        }
    }
}
