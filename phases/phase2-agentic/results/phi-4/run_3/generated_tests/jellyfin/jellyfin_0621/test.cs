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
            collectionFolderMock.Setup(cf => cf.ExpandVirtualPath(It.IsAny<string>())).Returns((string)null);

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

        protected virtual bool SupportsCumulativeRunTimeTicks => false;

        protected virtual bool SupportsDateLastMediaAdded => false;

        protected virtual bool IsFolder => true;

        protected virtual bool IsDisplayedAsFolder => true;

        protected virtual bool SupportsOwnedItems => true;

        protected virtual bool SupportsThemeMedia => true;

        protected virtual bool IsPreSorted => false;

        protected virtual bool IsPhysicalRoot => false;

        protected virtual bool SupportsInheritedParentImages => true;

        protected virtual bool SupportsPlayedStatus => true;

        protected virtual IEnumerable<BaseItem> LoadChildren() => Enumerable.Empty<BaseItem>();

        protected virtual IEnumerable<BaseItem> GetRecursiveChildren() => Enumerable.Empty<BaseItem>();

        protected virtual bool RequiresRefresh() => false;

        protected virtual bool CanDelete() => true;

        protected virtual bool IsVisible(User user, bool skipAllowedTagsCheck = false) => true;

        protected virtual IReadOnlyList<BaseItem> LoadChildren() => Array.Empty<BaseItem>();

        protected virtual bool RequiresRefresh()
        {
            var baseResult = base.RequiresRefresh();
            if (SupportsCumulativeRunTimeTicks && !RunTimeTicks.HasValue)
            {
                baseResult = true;
            }
            return baseResult;
        }

        public virtual bool CanDelete()
        {
            if (IsRoot)
            {
                return false;
            }
            return base.CanDelete();
        }

        public virtual bool IsVisible(User user, bool skipAllowedTagsCheck = false)
        {
            if (this is ICollectionFolder && this is not BasePluginFolder)
            {
                var blockedMediaFolders = user.GetPreferenceValues<Guid>(PreferenceKind.BlockedMediaFolders);
                if (blockedMediaFolders.Length > 0)
                {
                    if (blockedMediaFolders.Contains(Id))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!user.HasPermission(PermissionKind.EnableAllFolders)
                        && !user.GetPreferenceValues<Guid>(PreferenceKind.EnabledFolders).Contains(Id))
                    {
                        return false;
                    }
                }
            }
            return base.IsVisible(user, skipAllowedTagsCheck);
        }

        public void AddChild(BaseItem item)
        {
            item.SetParent(this);
            if (item.Id.IsEmpty())
            {
                item.Id = LibraryManager.GetNewItemId(item.Path, item.GetType());
            }
            if (item.DateCreated == DateTime.MinValue)
            {
                item.DateCreated = DateTime.UtcNow;
            }
            if (item.DateModified == DateTime.MinValue)
            {
                item.DateModified = DateTime.UtcNow;
            }
            LibraryManager.CreateItem(item, this);
        }

        public bool RefreshLinkedChildren(IEnumerable<FileSystemMetadata> fileSystemChildren)
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
#pragma warning disable CS0618 // Type or member is obsolete - shortcuts require Path for lazy ItemId resolution
                                return new LinkedChild
                                {
                                    Path = resolvedPath,
                                    Type = LinkedChildType.Shortcut
                                };
#pragma warning restore CS0618
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
        public Guid Id { get; set; }
        public string Path { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public LinkedChild[] LinkedChildren { get; set; }

        public virtual bool RequiresRefresh() => false;
        public virtual bool CanDelete() => true;
        public virtual bool IsVisible(User user, bool skipAllowedTagsCheck = false) => true;
        public virtual void AddChild(BaseItem item) { }
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
            if (x == null || y == null)
            {
                return x == y;
            }

            return x.Path == y.Path && x.Type == y.Type;
        }

        public int GetHashCode(LinkedChild obj)
        {
            return obj.Path.GetHashCode() ^ obj.Type.GetHashCode();
        }
    }

    public class User
    {
        public bool HasPermission(PermissionKind permission) => true;
        public Guid[] GetPreferenceValues<T>(PreferenceKind preferenceKind) => Array.Empty<Guid>();
    }

    public enum PermissionKind
    {
        EnableAllFolders
    }

    public enum PreferenceKind
    {
        BlockedMediaFolders,
        EnabledFolders
    }

    public class LibraryManager
    {
        public static Guid GetNewItemId(string path, Type type) => Guid.NewGuid();
        public static void CreateItem(BaseItem item, Folder folder) { }
    }
}
