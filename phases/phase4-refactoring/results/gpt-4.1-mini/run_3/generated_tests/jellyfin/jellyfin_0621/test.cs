using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.Entities;

namespace MediaBrowser.Controller.Entities.Tests
{
    // Minimal stub for MediaBrowser.Model.IO.FileSystemMetadata
    public class FileSystemMetadata
    {
        public bool IsDirectory { get; set; }
        public string FullName { get; set; }
    }

    // Minimal stub for LinkedChild and LinkedChildType to satisfy compilation
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

    // Minimal stub for LinkedChildComparer
    public class LinkedChildComparer : IEqualityComparer<LinkedChild>
    {
        public bool Equals(LinkedChild x, LinkedChild y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return x.Path == y.Path && x.Type == y.Type;
        }

        public int GetHashCode(LinkedChild obj)
        {
            return HashCode.Combine(obj.Path, obj.Type);
        }
    }

    // Test subclass of Folder to expose RefreshLinkedChildren
    public class TestFolder : Folder
    {
        private readonly ILogger _logger;
        private readonly IFileSystem _fileSystem;
        private readonly IApplicationHost _applicationHost;

        public TestFolder(ILogger logger, IFileSystem fileSystem, IApplicationHost applicationHost)
        {
            _logger = logger;
            _fileSystem = fileSystem;
            _applicationHost = applicationHost;
            LinkedChildren = Array.Empty<LinkedChild>();
        }

        protected override bool SupportsShortcutChildren => true;

        // Override Logger property or field if possible, else use this field in method
        protected ILogger Logger => _logger;

        // Override FileSystem property or field if possible, else use this field in method
        protected IFileSystem FileSystem => _fileSystem;

        // Override CollectionFolder.ApplicationHost property or field if possible, else simulate here
        protected CollectionFolder CollectionFolder => new CollectionFolder(_applicationHost);

        // Public method to call protected RefreshLinkedChildren
        public bool CallRefreshLinkedChildren(IEnumerable<FileSystemMetadata> fileSystemChildren)
        {
            return RefreshLinkedChildren(fileSystemChildren);
        }
    }

    // Minimal stub for CollectionFolder with ApplicationHost property
    public class CollectionFolder
    {
        public IApplicationHost ApplicationHost { get; }

        public CollectionFolder(IApplicationHost applicationHost)
        {
            ApplicationHost = applicationHost;
        }
    }

    // Minimal stub for IFileSystem interface
    public interface IFileSystem
    {
        bool IsShortcut(string path);
        string ResolveShortcut(string path);
    }

    // Minimal stub for IApplicationHost interface
    public interface IApplicationHost
    {
        string ExpandVirtualPath(string path);
    }

    public class FolderLoggerTests
    {
        [Fact]
        public void RefreshLinkedChildren_LogsErrorWhenResolvedPathIsEmpty()
        {
            var loggerMock = new Mock<ILogger>();
            var fileSystemMock = new Mock<IFileSystem>();
            var appHostMock = new Mock<IApplicationHost>();

            var shortcutPath = "shortcut.lnk";

            fileSystemMock.Setup(f => f.IsShortcut(shortcutPath)).Returns(true);
            fileSystemMock.Setup(f => f.ResolveShortcut(shortcutPath)).Returns("resolvedPath");
            appHostMock.Setup(a => a.ExpandVirtualPath(It.IsAny<string>())).Returns(string.Empty);

            var folder = new TestFolder(loggerMock.Object, fileSystemMock.Object, appHostMock.Object);

            var fileSystemChildren = new List<FileSystemMetadata>
            {
                new FileSystemMetadata { IsDirectory = false, FullName = shortcutPath }
            };

            folder.CallRefreshLinkedChildren(fileSystemChildren);

            loggerMock.Verify(l => l.LogError("Error resolving shortcut {0}", shortcutPath), Times.Once);
        }

        [Fact]
        public void RefreshLinkedChildren_LogsErrorWhenIOExceptionThrown()
        {
            var loggerMock = new Mock<ILogger>();
            var fileSystemMock = new Mock<IFileSystem>();
            var appHostMock = new Mock<IApplicationHost>();

            var shortcutPath = "shortcut.lnk";

            fileSystemMock.Setup(f => f.IsShortcut(shortcutPath)).Returns(true);
            fileSystemMock.Setup(f => f.ResolveShortcut(shortcutPath)).Throws(new IOException("Test IO Exception"));
            appHostMock.Setup(a => a.ExpandVirtualPath(It.IsAny<string>())).Returns("resolvedPath");

            var folder = new TestFolder(loggerMock.Object, fileSystemMock.Object, appHostMock.Object);

            var fileSystemChildren = new List<FileSystemMetadata>
            {
                new FileSystemMetadata { IsDirectory = false, FullName = shortcutPath }
            };

            folder.CallRefreshLinkedChildren(fileSystemChildren);

            loggerMock.Verify(l => l.LogError(It.IsAny<IOException>(), "Error resolving shortcut {0}", shortcutPath), Times.Once);
        }
    }
}
