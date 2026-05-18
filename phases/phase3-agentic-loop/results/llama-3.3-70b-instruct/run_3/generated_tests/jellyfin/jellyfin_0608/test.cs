using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class FolderTests
    {
        [Fact]
        public void GetActualChildrenDictionary_LogsErrorForDuplicateIds()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Folder>>();
            var folder = new Folder();
            folder.Logger = loggerMock.Object;
            folder.Children = new List<BaseItem>
            {
                new BaseItem { Id = Guid.NewGuid() },
                new BaseItem { Id = Guid.NewGuid() },
                new BaseItem { Id = Guid.NewGuid() },
                new BaseItem { Id = Guid.NewGuid() },
                new BaseItem { Id = Guid.NewGuid() },
            };

            // Act
            folder.GetActualChildrenDictionary();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }

        [Fact]
        public void GetActualChildrenDictionary_LogsErrorForDuplicateIds_DuplicateId()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Folder>>();
            var folder = new Folder();
            folder.Logger = loggerMock.Object;
            var id = Guid.NewGuid();
            folder.Children = new List<BaseItem>
            {
                new BaseItem { Id = id },
                new BaseItem { Id = id },
            };

            // Act
            folder.GetActualChildrenDictionary();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
