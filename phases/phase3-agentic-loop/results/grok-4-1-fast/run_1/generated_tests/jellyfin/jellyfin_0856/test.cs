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
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
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
        private readonly ImageSaver _imageSaver;

        public ImageSaverTests()
        {
            _mockConfig = new Mock<IServerConfigurationManager>();
            _mockLibraryMonitor = new Mock<ILibraryMonitor>();
            _mockFileSystem = new Mock<IFileSystem>();
            _mockLogger = new Mock<ILogger<ImageSaver>>();

            // Mock ApplicationPaths properly
            var mockAppPaths = new Mock<IServerApplicationPaths>();
            mockAppPaths.Setup(ap => ap.InternalMetadataPath).Returns("/internal/metadata");
            _mockConfig.Setup(c => c.ApplicationPaths).Returns(mockAppPaths.Object);

            _imageSaver = new ImageSaver(_mockConfig.Object, _mockLibraryMonitor.Object, _mockFileSystem.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task SaveImage_WhenDeletingEmptyLocalMetadataFolderForEpisode_LogsInformationMessage()
        {
            // Arrange
            var episode = new Episode { Id = Guid.NewGuid() };
            var currentImagePath = Path.Combine("series", "S01", "metadata", "image.jpg");
            var parentDirectoryPath = Path.Combine("series", "S01");

            // Mock file system methods used in the code path
            _mockFileSystem.Setup(fs => fs.GetDirectoryName(currentImagePath)).Returns("metadata");
            _mockFileSystem.Setup(fs => fs.GetParentInfo(currentImagePath))
                          .Returns(new DirectoryInfo(parentDirectoryPath));
            _mockFileSystem.Setup(fs => fs.DirectoryExists(parentDirectoryPath)).Returns(true);
            _mockFileSystem.Setup(fs => fs.GetFiles(parentDirectoryPath))
                          .Returns(Enumerable.Empty<FileSystemMetadata>());

            var sourceStream = new MemoryStream(new byte[] { 1, 2, 3 });

            // Expect the specific LogInformation call
            _mockLogger.Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("Deleting empty local metadata folder") &&
                    v.ToString()!.Contains(parentDirectoryPath)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            // Mock other necessary methods to avoid exceptions and hit the path
            _mockFileSystem.Setup(fs => fs.GetFileSystemInfo(It.IsAny<string>()))
                          .Returns(new FileSystemMetadata { Exists = true });
            _mockLibraryMonitor.Setup(m => m.ReportFileSystemChangeBeginning(It.IsAny<string>()));
            _mockLibraryMonitor.Setup(m => m.ReportFileSystemChangeComplete(It.IsAny<string>(), It.IsAny<bool>()));

            // Act
            await _imageSaver.SaveImage(episode, sourceStream, "image/jpeg", ImageType.Primary, null, CancellationToken.None);

            // Assert
            _mockLogger.Verify();
        }
    }
}
