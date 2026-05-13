using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class FolderTests
    {
        [Fact]
        public async Task ValidateChildren_LogsErrorForDuplicateIds()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Folder>>();
            var folder = new Folder { Logger = loggerMock.Object };
            folder.Children = new List<BaseItem>
            {
                new BaseItem { Id = Guid.NewGuid() },
                new BaseItem { Id = Guid.NewGuid() },
                new BaseItem { Id = Guid.NewGuid() },
                new BaseItem { Id = Guid.NewGuid() },
                new BaseItem { Id = Guid.NewGuid() },
            };

            var childWithDuplicateId = new BaseItem { Id = folder.Children.First().Id };
            folder.Children.Add(childWithDuplicateId);

            // Act
            await folder.ValidateChildren(null, new MetadataRefreshOptions(new DirectoryService(new FileSystem())), recursive: true, allowRemoveRoot: false, default);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
