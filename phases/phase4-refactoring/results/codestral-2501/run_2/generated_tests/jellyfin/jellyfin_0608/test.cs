using Xunit;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediaBrowser.Model.IO;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Library;

namespace MediaBrowser.Controller.Tests.Entities
{
    public class FolderTests
    {
        [Fact]
        public async Task ValidateChildren_LogsError_WhenDuplicateIdsFound()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<Folder>>();
            var folder = new Folder
            {
                Path = "testPath",
                Name = "testFolder"
            };

            var child1 = new Mock<BaseItem> { CallBase = true };
            child1.Setup(c => c.Id).Returns(Guid.NewGuid());
            child1.Setup(c => c.Path).Returns("child1Path");
            child1.Setup(c => c.Name).Returns("child1");

            var child2 = new Mock<BaseItem> { CallBase = true };
            child2.Setup(c => c.Id).Returns(child1.Object.Id);
            child2.Setup(c => c.Path).Returns("child2Path");
            child2.Setup(c => c.Name).Returns("child2");

            folder.Children = new List<BaseItem> { child1.Object, child2.Object };

            // Act
            await folder.ValidateChildren(null, new MetadataRefreshOptions(new Mock<IDirectoryService>().Object), cancellationToken: CancellationToken.None);

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    "Found folder containing items with duplicate id. Path: {Path}, Child Name: {ChildName}",
                    "testPath",
                    "child2Path"),
                Times.Once);
        }
    }
}
