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
        public void ValidateChildren_LogsErrorForDuplicateIds()
        {
            // Arrange
            var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger>();
            var folder = new Folder();
            folder.Logger = loggerMock.Object;

            var children = new List<BaseItem>
            {
                new BaseItem { Id = Guid.NewGuid() },
                new BaseItem { Id = Guid.NewGuid() },
                new BaseItem { Id = Guid.NewGuid() },
                new BaseItem { Id = Guid.NewGuid() },
                new BaseItem { Id = Guid.NewGuid() },
            };

            var firstChild = children.First();
            children.Add(firstChild);

            folder.Children = children;

            // Act
            folder.ValidateChildren(null, null); // Assuming ValidateChildren calls GetActualChildrenDictionary internally

            // Assert
            loggerMock.Verify(logger => logger.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
