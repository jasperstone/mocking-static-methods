using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.Entities;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class FolderTests
    {
        private class TestFolder : Folder
        {
            public bool CallRefreshLinkedChildren(IEnumerable<object> fileSystemChildren)
            {
                // Call the protected method via reflection
                var method = typeof(Folder).GetMethod("RefreshLinkedChildren", BindingFlags.Instance | BindingFlags.NonPublic);
                if (method == null) throw new InvalidOperationException("RefreshLinkedChildren method not found");
                return (bool)method.Invoke(this, new object[] { fileSystemChildren });
            }
        }

        private interface IFileSystem
        {
            bool IsShortcut(string path);
            string ResolveShortcut(string path);
        }

        private interface ICollectionFolder
        {
            IApplicationHost ApplicationHost { get; }
        }

        private interface IApplicationHost
        {
            string ExpandVirtualPath(string path);
        }

        private class FileSystemMetadata
        {
            public bool IsDirectory { get; set; }
            public string FullName { get; set; }
        }

        private class LinkedChild
        {
            public string Path { get; set; }
            public int Type { get; set; }
        }

        [Fact]
        public void RefreshLinkedChildren_LogsError_WhenResolvedPathIsNullOrEmpty()
        {
            // Arrange
            var folder = new TestFolder();

            var loggerMock = new Mock<ILogger>();
            var fileSystemMock = new Mock<IFileSystem>();
            var appHostMock = new Mock<IApplicationHost>();
            var collectionFolderMock = new Mock<ICollectionFolder>();

            // Set private fields or properties via reflection
            SetPrivateFieldOrProperty(folder, "Logger", loggerMock.Object);
            SetPrivateFieldOrProperty(folder, "FileSystem", fileSystemMock.Object);
            SetPrivateFieldOrProperty(folder, "CollectionFolder", collectionFolderMock.Object);

            collectionFolderMock.Setup(cf => cf.ApplicationHost).Returns(appHostMock.Object);
            appHostMock.Setup(ah => ah.ExpandVirtualPath(It.IsAny<string>())).Returns(string.Empty);

            var shortcutPath = "shortcut.lnk";
            var fileSystemChildren = new List<FileSystemMetadata>
            {
                new FileSystemMetadata { IsDirectory = false, FullName = shortcutPath }
            };

            fileSystemMock.Setup(fs => fs.IsShortcut(shortcutPath)).Returns(true);
            fileSystemMock.Setup(fs => fs.ResolveShortcut(shortcutPath)).Returns("somepath");

            // Act
            var result = folder.CallRefreshLinkedChildren(fileSystemChildren.Cast<object>());

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error resolving shortcut")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.False(result);
        }

        [Fact]
        public void RefreshLinkedChildren_LogsError_WhenIOExceptionThrown()
        {
            // Arrange
            var folder = new TestFolder();

            var loggerMock = new Mock<ILogger>();
            var fileSystemMock = new Mock<IFileSystem>();
            var appHostMock = new Mock<IApplicationHost>();
            var collectionFolderMock = new Mock<ICollectionFolder>();

            SetPrivateFieldOrProperty(folder, "Logger", loggerMock.Object);
            SetPrivateFieldOrProperty(folder, "FileSystem", fileSystemMock.Object);
            SetPrivateFieldOrProperty(folder, "CollectionFolder", collectionFolderMock.Object);

            collectionFolderMock.Setup(cf => cf.ApplicationHost).Returns(appHostMock.Object);
            appHostMock.Setup(ah => ah.ExpandVirtualPath(It.IsAny<string>())).Returns("resolvedPath");

            var shortcutPath = "shortcut.lnk";
            var fileSystemChildren = new List<FileSystemMetadata>
            {
                new FileSystemMetadata { IsDirectory = false, FullName = shortcutPath }
            };

            fileSystemMock.Setup(fs => fs.IsShortcut(shortcutPath)).Returns(true);
            fileSystemMock.Setup(fs => fs.ResolveShortcut(shortcutPath)).Throws(new IOException("Test IO Exception"));

            // Act
            var result = folder.CallRefreshLinkedChildren(fileSystemChildren.Cast<object>());

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error resolving shortcut")),
                    It.IsAny<IOException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.False(result);
        }

        private static void SetPrivateFieldOrProperty(object obj, string name, object value)
        {
            var type = obj.GetType();
            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(obj, value);
                return;
            }
            var prop = type.GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(obj, value);
                return;
            }
            throw new InvalidOperationException($"Field or property '{name}' not found on type {type.FullName}");
        }
    }
}
