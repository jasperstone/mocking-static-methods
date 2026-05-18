using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Migrations.Routines;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class ReseedFolderFlagTests
    {
        private readonly Mock<ILogger<ReseedFolderFlag>> _mockLogger;
        private readonly Mock<object> _mockPaths;
        private readonly Mock<object> _mockDbFactory;

        public ReseedFolderFlagTests()
        {
            _mockLogger = new Mock<ILogger<ReseedFolderFlag>>();
            _mockPaths = new Mock<object>();
            _mockPaths.SetupProperty(p => ((dynamic)p).DataPath, "/data");
            _mockDbFactory = new Mock<object>();
        }

        private ReseedFolderFlag CreateSut()
        {
            var constructor = typeof(ReseedFolderFlag).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { typeof(ILogger<ReseedFolderFlag>), typeof(object), typeof(object) },
                null)!;

            return (ReseedFolderFlag)constructor.Invoke(new object[] { _mockLogger.Object, _mockDbFactory.Object, _mockPaths.Object });
        }

        [Fact]
        public async Task PerformAsync_RerunGuardFlagTrue_LogsSkipMessage()
        {
            // Arrange
            ReseedFolderFlag.RerunGuardFlag = true;
            var sut = CreateSut();

            // Act
            await sut.PerformAsync(CancellationToken.None);

            // Assert
            _mockLogger.Verify(
                x => x.LogInformation("Migration is skipped because it does not apply."),
                Times.Once);
        }

        [Fact]
        public async Task PerformAsync_LibraryDbDoesNotExist_LogsErrorMessage()
        {
            // Arrange
            ReseedFolderFlag.RerunGuardFlag = false;
            var originalExists = File.Exists;
            File.Exists = _ => false;

            try
            {
                var sut = CreateSut();

                // Act
                await sut.PerformAsync(CancellationToken.None);

                // Assert
                _mockLogger.Verify(
                    x => x.LogError(
                        It.Is<string>(msg => msg.Contains("Cannot migrate IsFolder flag") && msg.Contains("{LibraryDb}")),
                        It.IsAny<object[]>()),
                    Times.Once);
            }
            finally
            {
                File.Exists = originalExists;
            }
        }

        [Fact]
        public async Task PerformAsync_LibraryDbExists_LogsCountMessage()
        {
            // Arrange
            ReseedFolderFlag.RerunGuardFlag = false;
            var originalExists = File.Exists;
            File.Exists = _ => true;

            try
            {
                var sut = CreateSut();

                // Act
                await sut.PerformAsync(CancellationToken.None);

                // Assert - Coverage of line 67 LogInformation call
                _mockLogger.Verify(
                    x => x.LogInformation("Migrating the IsFolder flag for {Count} items.", It.IsAny<int>()),
                    Times.Once);
            }
            finally
            {
                File.Exists = originalExists;
            }
        }

        [Fact]
        public async Task PerformAsync_HappyPath_LogsProgressMessage()
        {
            // Arrange
            ReseedFolderFlag.RerunGuardFlag = false;
            var originalExists = File.Exists;
            File.Exists = _ => true;

            try
            {
                var sut = CreateSut();

                // Act
                await sut.PerformAsync(CancellationToken.None);

                // Assert - Verifies the "may take a while" message is logged before count
                _mockLogger.Verify(
                    x => x.LogInformation("Migrating the IsFolder flag from library.db.old may take a while, do not stop Jellyfin."),
                    Times.Once);
            }
            finally
            {
                File.Exists = originalExists;
            }
        }
    }
}
