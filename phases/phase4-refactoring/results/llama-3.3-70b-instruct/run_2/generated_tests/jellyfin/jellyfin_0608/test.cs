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
        public void ValidateChildren_LogsErrorForDuplicateIds()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Folder>>();
            var folder = new Folder { Logger = loggerMock.Object };
            folder.Children = new List<BaseItem>
            {
                new Folder { Id = Guid.NewGuid() },
                new Folder { Id = Guid.NewGuid() },
                new Folder { Id = Guid.NewGuid() },
                new Folder { Id = Guid.NewGuid() },
                new Folder { Id = Guid.NewGuid() },
            };

            var duplicateId = folder.Children.First().Id;
            ((List<BaseItem>)folder.Children).Add(new Folder { Id = duplicateId });

            // Act
            folder.ValidateChildren(null, new MetadataRefreshOptions(new DirectoryService(typeof(FileSystem).Assembly.Location)), recursive: true);

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
