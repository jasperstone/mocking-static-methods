using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Users;

namespace MediaBrowser.Tests.Entities
{
    public class FolderTests
    {
        private class TestFolder : Folder
        {
            public override Task ValidateChildrenInternal(
                IProgress<double> progress,
                bool recursive,
                bool refreshChildMetadata,
                bool allowRemoveRoot,
                MetadataRefreshOptions refreshOptions,
                IDirectoryService directoryService,
                CancellationToken cancellationToken)
            {
                // Simulate validation logic
                return Task.CompletedTask;
            }

            public override IReadOnlyList<BaseItem> LoadChildren()
            {
                return new List<BaseItem>();
            }
        }

        [Fact]
        public void GetActualChildrenDictionary_Should_LogError_On_DuplicateIds()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var folder = new TestFolder();
            folder.Logger = mockLogger.Object;

            var child1 = new BaseItem { Id = Guid.NewGuid(), Path = "path1", Name = "Child1" };
            var child2 = new BaseItem { Id = child1.Id, Path = "path2", Name = "Child2" }; // duplicate ID

            folder.Children = new List<BaseItem> { child1, child2 };

            // Act
            var result = folder.GetActualChildrenDictionary();

            // Assert
            mockLogger.Verify(
                x => x.LogError(
                    It.Is<string>(s => s.Contains("Found folder containing items with duplicate id")),
                    It.IsAny<object[]>()
                ),
                Times.Once);
        }
    }
}
