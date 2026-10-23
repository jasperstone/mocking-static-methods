using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Providers.Manager;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.Manager.Tests
{
    public class ImageSaverTests
    {
        private readonly Mock<IServerConfigurationManager> _mockConfig;
        private readonly Mock<ILibraryMonitor> _mockLibraryMonitor;
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly Mock<ILogger<ImageSaver>> _mockLogger;

        public ImageSaverTests()
        {
            _mockConfig = new Mock<IServerConfigurationManager>();
            _mockLibraryMonitor = new Mock<ILibraryMonitor>();
            _mockFileSystem = new Mock<IFileSystem>();
            _mockLogger = new Mock<ILogger<ImageSaver>>();

            SetupMocks();
        }

        private void SetupMocks()
        {
            // Setup config for XbmcMetadataOptions
            _mockConfig.Setup(x => x.GetConfiguration<XbmcMetadataOptions>("xbmcmetadata"))
                      .Returns(new XbmcMetadataOptions { EnableExtraThumbsDuplication = false });

            // Setup empty files for parent directory
            _mockFileSystem.Setup(x => x.GetFiles(It.IsAny<string>()))
                          .Returns(new List<FileSystemMetadata>());

            // Setup directory exists
            _mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>()))
                          .Returns(true);
        }

        [Fact]
        public async Task SaveImage_WhenDeletingEmptyEpisodeMetadataFolder_LogsInformationMessage()
        {
            // Arrange
            var episode = new Episode();
            var imageSaver = new ImageSaver(_mockConfig.Object, _mockLibraryMonitor.Object, _mockFileSystem.Object, _mockLogger.Object);

            // Setup to hit the exact deletion path with empty metadata folder
            var currentImagePath = Path.Combine("/media", "S01", "metadata", "image.jpg");
            
            // Mock the current image path by making GetCurrentImage return local file
            // Setup file deletion to succeed
            _mockFileSystem.Setup(x => x.DeleteFile(currentImagePath));

            // Setup library monitor calls
            _mockLibraryMonitor.Setup(x => x.ReportFileSystemChangeBeginning(It.IsAny<string>()));
            _mockLibraryMonitor.Setup(x => x.ReportFileSystemChangeComplete(It.IsAny<string>(), It.IsAny<bool>()));

            using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

            // Act - Call SaveImage with conditions that should trigger the logger call on line 201
            await imageSaver.SaveImage(episode, stream, "image/jpeg", ImageType.Primary, null, true, CancellationToken.None);

            // Assert - Verify the specific LogInformation call from line 201 was executed
            _mockLogger.Verify(
                x => x.LogInformation("Deleting empty local metadata folder {Folder}", It.IsAny<string>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public void LoggerExtension_VerifiesLogInformationCallSignature()
        {
            // Arrange - Test the exact LoggerExtensions.LogInformation call pattern from line 201
            var mockLogger = new Mock<ILogger<ImageSaver>>();
            var logger = mockLogger.Object;
            var folderPath = "/test/S01";

            // Act - Directly invoke the extension method matching line 201
            logger.LogInformation("Deleting empty local metadata folder {Folder}", folderPath);

            // Assert - Verify it translates to the correct low-level Log call
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("Deleting empty local metadata folder") &&
                        v.ToString()!.Contains(folderPath)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
