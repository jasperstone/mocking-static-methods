using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Controller.Tests
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
            folder.Children.Add(folder.Children.First());

            // Act
            await folder.ValidateChildren(null, new MetadataRefreshOptions(new DirectoryService(new FileSystem())), true, false, default);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
