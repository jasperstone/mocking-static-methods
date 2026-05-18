using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using MediaBrowser.Controller.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaBrowser.Tests.Entities
{
    public class FolderTests
    {
        private class DummyFolder : Folder
        {
            public bool SupportShortcutChildrenCalled { get; set; } = false;
            public override bool SupportsShortcutChildren => SupportShortcutChildrenCalled;

            public List<LinkedChild> LinkedChildrenList { get; } = new List<LinkedChild>();

            public override IEnumerable<BaseItem> LoadChildren()
            {
                return new List<BaseItem>();
            }

            public DummyFolder(ILogger logger)
            {
                Logger = logger;
            }

            public ILogger Logger { get; }

            public override bool SupportsCumulativeRunTimeTicks => true;

            public override bool SupportsDateLastMediaAdded => true;

            public override IEnumerable<BaseItem> GetRecursiveChildren()
            {
                return Enumerable.Empty<BaseItem>();
            }
        }

        [Fact]
        public void RefreshLinkedChildren_Should_LogError_When_ShortcutResolutionFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var folder = new DummyFolder(loggerMock.Object)
            {
                SupportShortcutChildrenCalled = true,
                LinkedChildren = new LinkedChild[]
                {
                    new LinkedChild { ItemId = Guid.NewGuid(), Type = LinkedChildType.Shortcut, Path = "shortcut1" }
                }
            };

            var fileSystemChildren = new List<FileSystemMetadata>
            {
                new FileSystemMetadata { FullName = "shortcut1", IsDirectory = false }
            };

            // Act
            var result = folder.RefreshLinkedChildren(fileSystemChildren);

            // Assert
            loggerMock.Verify(
                x => x.LogError(It.IsAny<string>(), It.IsAny<object>()),
                Times.Once);
        }
    }
}
