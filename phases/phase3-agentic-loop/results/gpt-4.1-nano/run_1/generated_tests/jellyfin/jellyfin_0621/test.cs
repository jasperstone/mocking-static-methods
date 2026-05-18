using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using MediaBrowser.Controller.Entities;
using System.Collections.Generic;
using System.Linq;

namespace MediaBrowser.Tests.Entities
{
    public class FolderTests
    {
        private class TestFolder : Folder
        {
            public bool SupportShortcutChildrenOverride { get; set; } = false;
            public override bool SupportsShortcutChildren => SupportShortcutChildrenOverride;

            public List<BaseItem> LoadedChildren { get; set; } = new List<BaseItem>();
            protected override IReadOnlyList<BaseItem> LoadChildren() => LoadedChildren;
        }

        [Fact]
        public void RefreshLinkedChildren_Should_LogError_When_ExceptionThrown()
        {
            // Arrange
            var folder = new TestFolder();
            var loggerMock = new Mock<ILogger>();
            folder.Logger = loggerMock.Object;

            var shortcutItem = new FileSystemMetadata { FullName = "shortcut.lnk", IsDirectory = false };
            var fileSystemChildren = new List<FileSystemMetadata> { shortcutItem };

            // Force the method to support shortcut children
            folder.SupportShortcutChildrenOverride = true;

            // Mock the static dependencies
            var expandVirtualPathCalled = false;
            var expandVirtualPathResult = "resolvedPath";

            // Patch CollectionFolder.ApplicationHost.ExpandVirtualPath
            var originalExpandVirtualPath = CollectionFolder.ApplicationHost.ExpandVirtualPath;
            CollectionFolder.ApplicationHost.ExpandVirtualPath = (path) =>
            {
                expandVirtualPathCalled = true;
                return expandVirtualPathResult;
            };

            // Act
            var result = folder.RefreshLinkedChildren(fileSystemChildren);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Once);
            Assert.True(expandVirtualPathCalled);

            // Cleanup
            CollectionFolder.ApplicationHost.ExpandVirtualPath = originalExpandVirtualPath;
        }
    }
}
