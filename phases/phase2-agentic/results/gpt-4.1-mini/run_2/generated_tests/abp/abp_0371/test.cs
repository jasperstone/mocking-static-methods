using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectModification;
using Xunit;

namespace Volo.Abp.Cli.Tests.ProjectModification
{
    public class ProjectNpmPackageAdderTests
    {
        private readonly Mock<ILogger<ProjectNpmPackageAdder>> _loggerMock;
        private readonly Mock<ICmdHelper> _cmdHelperMock;
        private readonly ProjectNpmPackageAdder _packageAdder;

        public ProjectNpmPackageAdderTests()
        {
            _loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
            _cmdHelperMock = new Mock<ICmdHelper>();

            // We only need to test AddNpmPackageAsync method, so we can mock dependencies minimally
            _packageAdder = new ProjectNpmPackageAdder(
                jsonSerializer: null!,
                sourceCodeDownloadService: null!,
                angularSourceCodeAdder: null!,
                remoteServiceExceptionHandler: null!,
                installLibsService: null!,
                cmdHelper: _cmdHelperMock.Object,
                cliHttpClientFactory: null!,
                npmPackageInfoProvider: null!)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task AddNpmPackageAsync_LogsInformation_WhenPackageJsonExists_AndPackageNotInstalled()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var packageJsonPath = Path.Combine(tempDir, "package.json");
            var npmPackageName = "test-package";
            var npmPackage = new NpmPackageInfo { Name = npmPackageName };

            // Write package.json without the package name
            await File.WriteAllTextAsync(packageJsonPath, "{}");

            // Act
            await _packageAdder.AddNpmPackageAsync(tempDir, npmPackage);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Installing '{npmPackageName}' package")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("yarn add " + npmPackageName)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _cmdHelperMock.Verify(x => x.RunCmd(It.Is<string>(s => s.Contains("npx yarn add " + npmPackageName))), Times.Once);

            // Cleanup
            Directory.Delete(tempDir, true);
        }

        [Fact]
        public async Task AddNpmPackageAsync_LogsInformation_WhenPackageJsonExists_AndPackageAlreadyInstalled()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var packageJsonPath = Path.Combine(tempDir, "package.json");
            var npmPackageName = "test-package";
            var npmPackage = new NpmPackageInfo { Name = npmPackageName };

            // Write package.json containing the package name
            await File.WriteAllTextAsync(packageJsonPath, $"{{ \"{npmPackageName}\": \"1.0.0\" }}");

            // Act
            await _packageAdder.AddNpmPackageAsync(tempDir, npmPackage);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"'{npmPackageName}' is already installed.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _cmdHelperMock.Verify(x => x.RunCmd(It.IsAny<string>()), Times.Never);

            // Cleanup
            Directory.Delete(tempDir, true);
        }

        [Fact]
        public async Task AddNpmPackageAsync_LogsError_WhenPackageJsonDoesNotExist()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var npmPackageName = "test-package";
            var npmPackage = new NpmPackageInfo { Name = npmPackageName };

            // No package.json file created

            // Act
            await _packageAdder.AddNpmPackageAsync(tempDir, npmPackage);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("package.json not found!")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _cmdHelperMock.Verify(x => x.RunCmd(It.IsAny<string>()), Times.Never);

            // Cleanup
            Directory.Delete(tempDir, true);
        }

        // Minimal NpmPackageInfo class for testing
        private class NpmPackageInfo
        {
            public string Name { get; set; } = string.Empty;
        }
    }
}
