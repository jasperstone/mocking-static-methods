using Xunit;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Providers;

public class FolderTests
{
    [Fact]
    public async Task ValidateChildren_LogsError_WhenDuplicateChildIdFound()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<Folder>>();
        var folder = new Mock<Folder> { CallBase = true };
        folder.Setup(f => f.Logger).Returns(mockLogger.Object);
        folder.Setup(f => f.Path).Returns("test/path");
        folder.Setup(f => f.Name).Returns("test folder");

        var child1 = new Mock<BaseItem> { CallBase = true };
        child1.Setup(c => c.Id).Returns(Guid.NewGuid());
        child1.Setup(c => c.Path).Returns("child1/path");
        child1.Setup(c => c.Name).Returns("child1");

        var child2 = new Mock<BaseItem> { CallBase = true };
        child2.Setup(c => c.Id).Returns(child1.Object.Id);
        child2.Setup(c => c.Path).Returns("child2/path");
        child2.Setup(c => c.Name).Returns("child2");

        folder.Object.Children = new List<BaseItem> { child1.Object, child2.Object };

        // Act
        await folder.Object.ValidateChildren(null, new MetadataRefreshOptions(new Mock<IDirectoryService>().Object), cancellationToken: CancellationToken.None);

        // Assert
        mockLogger.Verify(
            logger => logger.LogError(
                "Found folder containing items with duplicate id. Path: {Path}, Child Name: {ChildName}",
                "test/path",
                "child2/path"),
            Times.Once);
    }
}
