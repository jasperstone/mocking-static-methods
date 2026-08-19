using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectModification;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.ProjectModification
{
    public class ProjectNpmPackageAdderTests
    {
        private readonly Mock<ILogger<ProjectNpmPackageAdder>> _loggerMock;
        private readonly Mock<ICmdHelper> _cmdHelperMock;
        private readonly ProjectNpmPackageAdder _npmPackageAdder;

        public ProjectNpmPackageAdderTests()
        {
            _loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
            _cmdHelperMock = new Mock<ICmdHelper>();

            _npmPackageAdder = new ProjectNpmPackageAdder(
                null,
                null,
                null,
                null,
                null,
                _cmdHelperMock.Object,
                null,
                null)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task AddNpmPackageAsync_ShouldLogInformation_WhenPackageJsonExists()
        {
            // Arrange
            var directory = "testDirectory";
            var npmPackageName = "testPackage";
            var npmPackage = new NpmPackageInfo { Name = npmPackageName };
            var packageJsonFilePath = Path.Combine(directory, "package.json");

            // Create a temporary package.json file
            Directory.CreateDirectory(directory);
            File.WriteAllText(packageJsonFilePath, "{}");

            // Act
            await _npmPackageAdder.AddNpmPackageAsync(directory, npmPackage);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogInformation(
                    It.IsAny<string>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));

            // Clean up
            File.Delete(packageJsonFilePath);
            Directory.Delete(directory);
        }

        [Fact]
        public async Task AddNpmPackageAsync_ShouldLogError_WhenPackageJsonDoesNotExist()
        {
            // Arrange
            var directory = "testDirectory";
            var npmPackageName = "testPackage";
            var npmPackage = new NpmPackageInfo { Name = npmPackageName };

            // Act
            await _npmPackageAdder.AddNpmPackageAsync(directory, npmPackage);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<string>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task AddNpmPackageAsync_ShouldLogInformation_WhenPackageIsAlreadyInstalled()
        {
            // Arrange
            var directory = "testDirectory";
            var npmPackageName = "testPackage";
            var npmPackage = new NpmPackageInfo { Name = npmPackageName };
            var packageJsonFilePath = Path.Combine(directory, "package.json");

            // Create a temporary package.json file with the package already installed
            Directory.CreateDirectory(directory);
            File.WriteAllText(packageJsonFilePath, "{\"dependencies\": {\"testPackage\": \"1.0.0\"}}");

            // Act
            await _npmPackageAdder.AddNpmPackageAsync(directory, npmPackage);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogInformation(
                    It.IsAny<string>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Clean up
            File.Delete(packageJsonFilePath);
            Directory.Delete(directory);
        }

        [Fact]
        public async Task AddNpmPackageAsync_ShouldRunCmd_WhenPackageIsNotInstalled()
        {
            // Arrange
            var directory = "testDirectory";
            var npmPackageName = "testPackage";
            var npmPackage = new NpmPackageInfo { Name = npmPackageName };
            var packageJsonFilePath = Path.Combine(directory, "package.json");

            // Create a temporary package.json file
            Directory.CreateDirectory(directory);
            File.WriteAllText(packageJsonFilePath, "{}");

            // Act
            await _npmPackageAdder.AddNpmPackageAsync(directory, npmPackage);

            // Assert
            _cmdHelperMock.Verify(
                cmdHelper => cmdHelper.RunCmd(It.IsAny<string>()),
                Times.Once);

            // Clean up
            File.Delete(packageJsonFilePath);
            Directory.Delete(directory);
        }
    }
}
