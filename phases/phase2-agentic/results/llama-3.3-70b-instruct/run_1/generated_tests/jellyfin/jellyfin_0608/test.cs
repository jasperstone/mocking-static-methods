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
            var child1 = new BaseItem { Id = Guid.NewGuid() };
            var child2 = new BaseItem { Id = child1.Id };
            folder.Children = new List<BaseItem> { child1, child2 };

            // Act
            await folder.ValidateChildren(null, new MetadataRefreshOptions(new DirectoryService(null)), recursive: true, allowRemoveRoot: false, default);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
