using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Providers.Manager;

namespace MediaBrowser.Providers.Manager.Tests
{
    public class ImageSaverTests
    {
        private class TestBaseItem : BaseItem
        {
            public override bool SupportsLocalMetadata => true;
            public override bool IsSaveLocalMetadataEnabled() => true;
            public override bool IsFileProtocol => true;
            public override bool AllowsMultipleImages(ImageType type) => false;
            public override System.Collections.Generic.IEnumerable<ImageInfo> GetImages(ImageType type) => Array.Empty<ImageInfo>();
            public override ExtraType? ExtraType => null;
        }

        [Fact]
        public async Task SaveImage_LogsInformationWhenDeletingPreviousImage()
        {
            // Arrange
            var mockConfig = new Mock<IServerConfigurationManager>();
            var mockLibraryMonitor = new Mock<ILibraryMonitor>();
            var mockFileSystem = new Mock<IFileSystem>();
            var mockLogger = new Mock<ILogger>();

            var internalMetadataPath = "internalMetadataPath";

            var mockAppPaths = new Mock<IApplicationPaths>();
            mockAppPaths.SetupGet(p => p.InternalMetadataPath).Returns(internalMetadataPath);
            mockConfig.SetupGet(c => c.ApplicationPaths).Returns(mockAppPaths.Object);

            mockConfig.Setup(c => c.GetConfiguration<XbmcMetadataOptions>("xbmcmetadata"))
                .Returns(new XbmcMetadataOptions { EnableExtraThumbsDuplication = false });

            var item = new TestBaseItem();

            // Setup file system and library monitor expectations
            mockLibraryMonitor.Setup(lm => lm.ReportFileSystemChangeBeginning(It.IsAny<string>()));
            mockLibraryMonitor.Setup(lm => lm.ReportFileSystemChangeComplete(It.IsAny<string>(), false));

            mockFileSystem.Setup(fs => fs.DeleteFile(It.IsAny<string>()));

            // Setup logger to verify LogInformation call
            mockLogger.Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting previous image")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            var imageSaver = new ImageSaver(mockConfig.Object, mockLibraryMonitor.Object, mockFileSystem.Object, mockLogger.Object);

            // Act
            using var sourceStream = new MemoryStream(new byte[] { 1, 2, 3 });
            await imageSaver.SaveImage(item, sourceStream, "image/jpeg", ImageType.Primary, 0, CancellationToken.None);

            // Assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting previous image")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }

        // Dummy classes to satisfy dependencies
        private class XbmcMetadataOptions
        {
            public bool EnableExtraThumbsDuplication { get; set; }
        }
    }
}
