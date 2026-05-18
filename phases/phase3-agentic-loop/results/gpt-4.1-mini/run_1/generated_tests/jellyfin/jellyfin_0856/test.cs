using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Providers.Manager;
using MediaBrowser.Model.Entities;

namespace MediaBrowser.Providers.Manager.Tests
{
    public class ImageSaverTests
    {
        [Fact]
        public async Task SaveImage_DeletesPreviousImage_LogsInformation()
        {
            // Arrange
            var mockConfig = new Mock<IServerConfigurationManager>(MockBehavior.Strict);
            var mockLibraryMonitor = new Mock<ILibraryMonitor>(MockBehavior.Strict);
            var mockFileSystem = new Mock<IFileSystem>(MockBehavior.Strict);
            var mockLogger = new Mock<ILogger>(MockBehavior.Strict);

            var appPaths = new ApplicationPaths
            {
                InternalMetadataPath = "internalmetadata"
            };

            mockConfig.Setup(c => c.ApplicationPaths).Returns(appPaths);
            mockConfig.Setup(c => c.GetConfiguration<XbmcMetadataOptions>("xbmcmetadata"))
                .Returns(new XbmcMetadataOptions { EnableExtraThumbsDuplication = false });

            mockLibraryMonitor.Setup(lm => lm.ReportFileSystemChangeBeginning(It.IsAny<string>()));
            mockLibraryMonitor.Setup(lm => lm.ReportFileSystemChangeComplete(It.IsAny<string>(), false));

            var deletedFilePath = "somepath/image.jpg";
            var parentMetadataPath = "parentMetadata";

            mockFileSystem.Setup(fs => fs.DeleteFile(deletedFilePath));
            mockFileSystem.Setup(fs => fs.DirectoryExists(parentMetadataPath)).Returns(true);
            mockFileSystem.Setup(fs => fs.GetFiles(parentMetadataPath)).Returns(Array.Empty<string>());

            // Setup logger expectations for the two LogInformation calls
            mockLogger.Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting previous image")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            mockLogger.Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting empty local metadata folder")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            // Setup logger for error logs (should not be called in this test)
            mockLogger.Setup(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>())).Verifiable();

            // Create an Episode item with a current image that is local file and path conditions to trigger deletion
            var episode = new Episode
            {
                ExtraType = null,
                SupportsLocalMetadata = true,
                IsSaveLocalMetadataEnabledFunc = () => true,
                IsFileProtocol = true,
            };

            // Setup current image
            var currentImage = new ImageInfo
            {
                IsLocalFile = true,
                Path = deletedFilePath
            };

            // We need to mock GetCurrentImage to return currentImage, but it's private, so we simulate by subclassing ImageSaver
            var imageSaver = new TestImageSaver(mockConfig.Object, mockLibraryMonitor.Object, mockFileSystem.Object, mockLogger.Object, currentImage);

            // Setup GetSavePaths to return a single path (simulate saving to a single location)
            imageSaver.SetSavePaths(new[] { "savedpath/image.jpg" }, new[] { "retrypath/image.jpg" });

            // Setup SaveImageToLocation to return the path it is given
            imageSaver.SetSaveImageToLocationFunc((stream, path, retryPath, token) => Task.FromResult(path));

            // Setup SetImagePath to do nothing (override)
            imageSaver.SetSetImagePathAction((item, type, index, path) => { });

            // Setup episode to have directory "metadata" for current image path
            // We simulate Path.GetDirectoryName to return "metadata" for currentImage.Path
            // We simulate Directory.GetParent to return a DirectoryInfo with FullName = parentMetadataPath
            // We will override these static calls by injecting delegates in the test subclass

            imageSaver.SetGetDirectoryNameFunc(path => "metadata");
            imageSaver.SetGetParentFunc(path => new DirectoryInfoStub(parentMetadataPath));

            // Act
            using var sourceStream = new MemoryStream(new byte[] { 1, 2, 3 });
            await imageSaver.SaveImage(episode, sourceStream, "image/jpeg", MediaBrowser.Model.Entities.ImageType.Primary, 0, CancellationToken.None);

            // Assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting previous image")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting empty local metadata folder")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            mockFileSystem.Verify(fs => fs.DeleteFile(deletedFilePath), Times.Once);
        }

        // Helper classes and subclass to override private methods and static calls
        private class TestImageSaver : ImageSaver
        {
            private readonly ImageInfo _currentImage;
            private string[] _savePaths = Array.Empty<string>();
            private string[] _retryPaths = Array.Empty<string>();
            private Func<Stream, string, string, CancellationToken, Task<string>> _saveImageToLocationFunc;
            private Action<BaseItem, MediaBrowser.Model.Entities.ImageType, int?, string> _setImagePathAction;
            private Func<string, string> _getDirectoryNameFunc;
            private Func<string, DirectoryInfo> _getParentFunc;

            public TestImageSaver(IServerConfigurationManager config, ILibraryMonitor libraryMonitor, IFileSystem fileSystem, ILogger logger, ImageInfo currentImage)
                : base(config, libraryMonitor, fileSystem, logger)
            {
                _currentImage = currentImage;
                _saveImageToLocationFunc = (s, p, r, c) => Task.FromResult(p);
                _setImagePathAction = (item, type, index, path) => { };
                _getDirectoryNameFunc = path => Path.GetDirectoryName(path);
                _getParentFunc = path => new DirectoryInfo(path).Parent;
            }

            protected override object GetCurrentImage(BaseItem item, MediaBrowser.Model.Entities.ImageType type, int index)
            {
                return _currentImage;
            }

            protected override string[] GetSavePaths(BaseItem item, MediaBrowser.Model.Entities.ImageType type, int? imageIndex, string mimeType, bool saveLocally)
            {
                return _savePaths;
            }

            protected override Task<string> SaveImageToLocation(Stream source, string path, string retryPath, CancellationToken cancellationToken)
            {
                return _saveImageToLocationFunc(source, path, retryPath, cancellationToken);
            }

            protected override void SetImagePath(BaseItem item, MediaBrowser.Model.Entities.ImageType type, int? imageIndex, string path)
            {
                _setImagePathAction(item, type, imageIndex, path);
            }

            protected override string GetDirectoryName(string path)
            {
                return _getDirectoryNameFunc(path);
            }

            protected override DirectoryInfo GetParent(string path)
            {
                return _getParentFunc(path);
            }

            public void SetSavePaths(string[] savePaths, string[] retryPaths)
            {
                _savePaths = savePaths;
                _retryPaths = retryPaths;
            }

            public void SetSaveImageToLocationFunc(Func<Stream, string, string, CancellationToken, Task<string>> func)
            {
                _saveImageToLocationFunc = func;
            }

            public void SetSetImagePathAction(Action<BaseItem, MediaBrowser.Model.Entities.ImageType, int?, string> action)
            {
                _setImagePathAction = action;
            }

            public void SetGetDirectoryNameFunc(Func<string, string> func)
            {
                _getDirectoryNameFunc = func;
            }

            public void SetGetParentFunc(Func<string, DirectoryInfo> func)
            {
                _getParentFunc = func;
            }
        }

        // Minimal ImageInfo class to simulate current image
        private class ImageInfo
        {
            public bool IsLocalFile { get; set; }
            public string Path { get; set; }
        }
    }
}
