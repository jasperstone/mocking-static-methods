using Xunit;
using Moq;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class FolderTests
    {
        [Fact]
        public void GetActualChildrenDictionary_LogsErrorWhenDuplicateIdFound()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var loggerMock = new Mock<ILogger<Folder>>();
            var folder = new Folder(loggerFactory.CreateLogger<Folder>());
            folder.Children = new BaseItem[]
            {
                new BaseItem { Id = Guid.NewGuid() },
                new BaseItem { Id = Guid.NewGuid() },
                new BaseItem { Id = Guid.NewGuid() },
                new BaseItem { Id = Guid.NewGuid() },
                new BaseItem { Id = Guid.NewGuid() },
                new BaseItem { Id = ((BaseItem[])folder.Children)[0].Id },
            };

            // Act
            folder.GetActualChildrenDictionary();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void GetActualChildrenDictionary_DoesNotLogErrorWhenNoDuplicateIdFound()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var loggerMock = new Mock<ILogger<Folder>>();
            var folder = new Folder(loggerFactory.CreateLogger<Folder>());
            folder.Children = new BaseItem[]
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
    }
}
