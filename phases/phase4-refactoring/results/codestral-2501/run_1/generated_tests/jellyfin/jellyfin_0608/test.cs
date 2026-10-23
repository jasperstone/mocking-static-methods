using Xunit;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class FolderTests
{
    [Fact]
    public async Task ValidateChildren_LogsError_WhenDuplicateChildIdFound()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<Folder>>();
        var folder = new Folder
        {
            Logger = mockLogger.Object,
            Path = "testPath",
            Name = "testName",
            Children = new List<BaseItem>
            {
                new BaseItem { Id = Guid.NewGuid(), Path = "childPath1", Name = "childName1" },
                new BaseItem { Id = Guid.NewGuid(), Path = "childPath2", Name = "childName2" },
                new BaseItem { Id = Guid.NewGuid(), Path = "childPath3", Name = "childName3" },
                new BaseItem { Id = Guid.NewGuid(), Path = "childPath4", Name = "childName4" }
            }
        };

        // Act
        await folder.ValidateChildren(null, new MetadataRefreshOptions(new DirectoryService(null)), true, false, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            logger => logger.LogError(
                "Found folder containing items with duplicate id. Path: {Path}, Child Name: {ChildName}",
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Once);
    }
}
