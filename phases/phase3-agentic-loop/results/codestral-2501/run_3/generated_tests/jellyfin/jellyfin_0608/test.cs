using Xunit;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class FolderTests
{
    [Fact]
    public async Task ValidateChildren_ShouldLogError_WhenDuplicateChildIdFound()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<Folder>>();
        var folder = new Folder
        {
            Logger = mockLogger.Object,
            Path = "TestPath",
            Name = "TestFolder"
        };

        var child1 = new BaseItem { Id = Guid.NewGuid(), Path = "Child1Path", Name = "Child1" };
        var child2 = new BaseItem { Id = child1.Id, Path = "Child2Path", Name = "Child2" }; // Duplicate ID

        folder.Children = new List<BaseItem> { child1, child2 };

        // Act
        await folder.ValidateChildren(null, new MetadataRefreshOptions(new Mock<IDirectoryService>().Object), cancellationToken: CancellationToken.None);

        // Assert
        mockLogger.Verify(
            logger => logger.LogError(
                "Found folder containing items with duplicate id. Path: {Path}, Child Name: {ChildName}",
                "TestPath",
                "Child2"),
            Times.Once);
    }
}
