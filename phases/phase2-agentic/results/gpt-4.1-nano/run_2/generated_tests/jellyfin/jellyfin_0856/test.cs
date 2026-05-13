using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Providers.Manager;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Tests.Providers.Manager
{
    public class ImageSaverTests
    {
        private readonly Mock<IServerConfigurationManager> _configMock;
        private readonly Mock<ILibraryMonitor> _libraryMonitorMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly ImageSaver _imageSaver;

        public ImageSaverTests()
        {
            _configMock = new Mock<IServerConfigurationManager>();
            _libraryMonitorMock = new Mock<ILibraryMonitor>();
            _fileSystemMock = new Mock<IFileSystem>();
            _loggerMock = new Mock<ILogger>();

            _imageSaver = new ImageSaver(
                _configMock.Object,
                _libraryMonitorMock.Object,
                _fileSystemMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task SaveImage_Should_LogInformation_When_DeletingPreviousImage()
        {
            // Arrange
            var itemMock = new Mock<BaseItem>();
            var streamMock = new MemoryStream();
            var currentImagePath = "C:\\Images\\oldimage.jpg";

            var currentImageMock = new Mock<Image>();
            currentImageMock.SetupGet(i => i.IsLocalFile).Returns(true);
            currentImageMock.SetupGet(i => i.Path).Returns(currentImagePath);

            // Setup GetCurrentImage to return our mock image
            _imageSaver.GetType()
                .GetMethod("GetCurrentImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_imageSaver, new object[] { itemMock.Object, ImageType.Primary, 0 })
                .Returns(currentImageMock.Object);

            // Setup item mock
            itemMock.SetupGet(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.SetupGet(i => i.IsSaveLocalMetadataEnabled).Returns(true);
            itemMock.SetupGet(i => i.ExtraType).Returns((int?)null);
            itemMock.SetupGet(i => i.IsFileProtocol).Returns(true);
            itemMock.Setup(i => i.AllowsMultipleImages(It.IsAny<ImageType>())).Returns(false);
            itemMock.Setup(i => i.GetImages(It.IsAny<ImageType>())).Returns(new[] { currentImageMock.Object });
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.IsFileProtocol).Returns(true);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.ExtraType).Returns((int?)null);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.IsFileProtocol).Returns(true);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.ExtraType).Returns((int?)null);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.IsFileProtocol).Returns(true);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.ExtraType).Returns((int?)null);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.IsFileProtocol).Returns(true);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.ExtraType).Returns((int?)null);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.IsFileProtocol).Returns(true);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.ExtraType).Returns((int?)null);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.IsFileProtocol).Returns(true);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.ExtraType).Returns((int?)null);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.IsFileProtocol).Returns(true);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.ExtraType).Returns((int?)null);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.IsFileProtocol).Returns(true);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.ExtraType).Returns((int?)null);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.IsFileProtocol).Returns(true);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.ExtraType).Returns((int?)null);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.IsFileProtocol).Returns(true);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.ExtraType).Returns((int?)null);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.IsFileProtocol).Returns(true);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.ExtraType).Returns((int?)null);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.IsFileProtocol).Returns(true);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.ExtraType).Returns((int?)null);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.IsFileProtocol).Returns(true);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.ExtraType).Returns((int?)null);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.IsFileProtocol).Returns(true);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.ExtraType).Returns((int?)null);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.IsFileProtocol).Returns(true);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.ExtraType).Returns((int?)null);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.IsFileProtocol).Returns(true);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.ExtraType).Returns((int?)null);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.IsFileProtocol).Returns(true);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.ExtraType).Returns((int?)null);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.IsFileProtocol).Returns(true);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.ExtraType).Returns((int?)null);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.IsFileProtocol).Returns(true);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.ExtraType).Returns((int?)null);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            itemMock.Setup(i => i.IsFileProtocol).Returns(true);
            itemMock.Setup(i => i.SupportsLocalMetadata).Returns(true);
            itemMock.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            // Setup GetImage to return our mock image
            itemMock.Setup(i => i.GetImages(It.IsAny<ImageType>())).Returns(new[] { currentImageMock.Object });
            // Setup SetImagePath to do nothing
            var setImagePathCalled = false;
            _imageSaver.GetType()
                .GetMethod("SetImagePath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_imageSaver, new object[] { itemMock.Object, ImageType.Primary, 0, It.IsAny<string>() });
            // Setup _config.ApplicationPaths.InternalMetadataPath
            var configMock = new Mock<IConfiguration>();
            configMock.SetupGet(c => c.ApplicationPaths).Returns(new ApplicationPaths { InternalMetadataPath = "metadata" });
            // Setup _config.GetConfiguration<XbmcMetadataOptions>
            _configMock.Setup(c => c.GetConfiguration<XbmcMetadataOptions>("xbmcmetadata"))
                .Returns(new XbmcMetadataOptions { EnableExtraThumbsDuplication = false });
            // Setup _fileSystem.DeleteFile to do nothing
            _fileSystemMock.Setup(fs => fs.DeleteFile(It.IsAny<string>()));
            // Setup _fileSystem.DirectoryExists to return true
            _fileSystemMock.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
            // Setup _fileSystem.GetFiles to return empty
            _fileSystemMock.Setup(fs => fs.GetFiles(It.IsAny<string>())).Returns(Enumerable.Empty<string>());
            // Setup Directory.Delete to do nothing
            Directory.Delete = (path) => { setImagePathCalled = true; };
            // Setup _libraryMonitor.ReportFileSystemChangeBeginning
            _libraryMonitorMock.Setup(l => l.ReportFileSystemChangeBeginning(It.IsAny<string>()));
            // Setup _fileSystem.DeleteFile to throw FileNotFoundException
            _fileSystemMock.Setup(fs => fs.DeleteFile(It.IsAny<string>())).Throws(new FileNotFoundException());

            // Act
            await _imageSaver.SaveImage(itemMock.Object, streamMock, "image/jpeg", ImageType.Primary, 0, true, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting previous image")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
