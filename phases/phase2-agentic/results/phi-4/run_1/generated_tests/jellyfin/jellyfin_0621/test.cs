using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class FolderTests
    {
        [Fact]
        public void RefreshLinkedChildren_LogsError_WhenShortcutCannotBeResolved()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var fileSystemMock = new Mock<IFileSystem>();
            var collectionFolderMock = new Mock<ICollectionFolder>();

            var folder = new Folder
            {
                Logger = loggerMock.Object,
                FileSystem = fileSystemMock.Object,
                CollectionFolder = collectionFolderMock.Object
            };

            var fileSystemChildren = new List<FileSystemMetadata>
            {
                new FileSystemMetadata
                {
                    FullName = "path/to/shortcut.lnk",
                    IsDirectory = false
                }
            };

            fileSystemMock.Setup(fs => fs.IsShortcut(It.IsAny<string>())).Returns(true);
            fileSystemMock.Setup(fs => fs.ResolveShortcut(It.IsAny<string>())).Returns((string)null);

            // Act
            folder.RefreshLinkedChildren(fileSystemChildren);

            // Assert
            loggerMock.Verify(
                log => log.LogError(It.IsAny<Exception>(), "Error resolving shortcut {0}", "path/to/shortcut.lnk"),
                Times.Once);
        }
    }

    // Mock interfaces and classes for testing
    public interface IFileSystem
    {
        bool IsShortcut(string path);
        string ResolveShortcut(string path);
    }

    public interface ICollectionFolder
    {
        string ExpandVirtualPath(string path);
    }

    public class FileSystemMetadata
    {
        public string FullName { get; set; }
        public bool IsDirectory { get; set; }
    }

    public class Folder : BaseItem
    {
        public ILogger Logger { get; set; }
        public IFileSystem FileSystem { get; set; }
        public ICollectionFolder CollectionFolder { get; set; }

        protected virtual bool SupportsShortcutChildren => true;

        protected virtual bool FilterLinkedChildrenPerUser => false;

        protected virtual IEnumerable<BaseItem> LoadChildren => Enumerable.Empty<BaseItem>();

        protected virtual IEnumerable<BaseItem> GetRecursiveChildren() => Enumerable.Empty<BaseItem>();

        protected virtual bool RequiresRefresh() => false;

        protected virtual bool CanDelete() => true;

        protected virtual bool IsVisible(User user, bool skipAllowedTagsCheck = false) => true;

        protected virtual IReadOnlyList<BaseItem> LoadChildren() => Array.Empty<BaseItem>();

        protected virtual bool IsVisible(User user, bool skipAllowedTagsCheck = false) => true;

        protected virtual bool RequiresRefresh() => false;

        protected virtual bool CanDelete() => true;

        protected virtual IEnumerable<BaseItem> LoadChildren() => Array.Empty<BaseItem>();

        protected virtual IEnumerable<BaseItem> GetRecursiveChildren() => Enumerable.Empty<BaseItem>();

        protected virtual bool FilterLinkedChildrenPerUser => false;

        protected virtual bool SupportsShortcutChildren => true;

        protected virtual bool SupportsCumulativeRunTimeTicks => false;

        protected virtual bool SupportsDateLastMediaAdded => false;

        protected virtual bool SupportsUserDataFromChildren => true;

        protected virtual bool SupportsInheritedParentImages => true;

        protected virtual bool SupportsPlayedStatus => true;

        protected virtual bool IsFolder => true;

        protected virtual bool IsDisplayedAsFolder => true;

        protected virtual bool SupportsThemeMedia => true;

        protected virtual bool IsPreSorted => false;

        protected virtual bool IsPhysicalRoot => false;

        protected virtual bool SupportsOwnedItems => true;

        protected virtual string FileNameWithoutExtension => null;

        protected virtual bool IsRoot => false;

        protected virtual LinkedChild[] LinkedChildren => Array.Empty<LinkedChild>();

        protected virtual DateTime? DateLastMediaAdded => null;

        protected virtual IEnumerable<BaseItem> Children => Enumerable.Empty<BaseItem>();

        protected virtual IEnumerable<BaseItem> RecursiveChildren => Enumerable.Empty<BaseItem>();

        /// <summary>
        /// Refreshes the linked children.
        /// </summary>
        /// <param name="fileSystemChildren">The enumerable of file system metadata.</param>
        /// <returns><c>true</c> if the linked children were updated, <c>false</c> otherwise.</returns>
        protected virtual bool RefreshLinkedChildren(IEnumerable<FileSystemMetadata> fileSystemChildren)
        {
            if (SupportsShortcutChildren)
            {
                var newShortcutLinks = fileSystemChildren
                    .Where(i => !i.IsDirectory && FileSystem.IsShortcut(i.FullName))
                    .Select(i =>
                    {
                        try
                        {
                            Logger.LogDebug("Found shortcut at {0}", i.FullName);

                            var resolvedPath = CollectionFolder.ExpandVirtualPath(FileSystem.ResolveShortcut(i.FullName));

                            if (!string.IsNullOrEmpty(resolvedPath))
                            {
                                return new LinkedChild
                                {
                                    Path = resolvedPath,
                                    Type = LinkedChildType.Shortcut
                                };
                            }

                            Logger.LogError("Error resolving shortcut {0}", i.FullName);

                            return null;
                        }
                        catch (IOException ex)
                        {
                            Logger.LogError(ex, "Error resolving shortcut {0}", i.FullName);
                            return null;
                        }
                    })
                    .Where(i => i is not null)
                    .ToList();

                var currentShortcutLinks = LinkedChildren.Where(i => i.Type == LinkedChildType.Shortcut).ToList();

                if (!newShortcutLinks.SequenceEqual(currentShortcutLinks, new LinkedChildComparer(FileSystem)))
                {
                    Logger.LogInformation("Shortcut links have changed for {0}", Path);

                    newShortcutLinks.AddRange(LinkedChildren.Where(i => i.Type == LinkedChildType.Manual));
                    LinkedChildren = newShortcutLinks.ToArray();
                    return true;
                }
            }

            return false;
        }
    }

    public class BaseItem
    {
    }

    public class User
    {
    }

    public class LinkedChild
    {
        public string Path { get; set; }
        public LinkedChildType Type { get; set; }
    }

    public enum LinkedChildType
    {
        Shortcut,
        Manual
    }

    public class LinkedChildComparer : IEqualityComparer<LinkedChild>
    {
        private readonly IFileSystem _fileSystem;

        public LinkedChildComparer(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public bool Equals(LinkedChild x, LinkedChild y)
        {
            return x.Path == y.Path && x.Type == y.Type;
        }

        public int GetHashCode(LinkedChild obj)
        {
            return obj.Path.GetHashCode() ^ obj.Type.GetHashCode();
        }
    }
}
