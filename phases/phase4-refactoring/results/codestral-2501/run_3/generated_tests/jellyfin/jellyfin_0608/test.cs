using Xunit;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System;

namespace MediaBrowser.Controller.Tests.Entities
{
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

            var child1 = new Mock<BaseItem>();
            child1.SetupGet(c => c.Id).Returns(Guid.NewGuid());
            child1.SetupGet(c => c.Path).Returns("Child1Path");
            child1.SetupGet(c => c.Name).Returns("Child1");

            var child2 = new Mock<BaseItem>();
            child2.SetupGet(c => c.Id).Returns(child1.Object.Id);
            child2.SetupGet(c => c.Path).Returns("Child2Path");
            child2.SetupGet(c => c.Name).Returns("Child2");

            folder.Children = new List<BaseItem> { child1.Object, child2.Object };

            // Act
            await folder.ValidateChildren(null, new MetadataRefreshOptions(null), cancellationToken: CancellationToken.None);

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    "Found folder containing items with duplicate id. Path: {Path}, Child Name: {ChildName}",
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Once);
        }
    }
}
