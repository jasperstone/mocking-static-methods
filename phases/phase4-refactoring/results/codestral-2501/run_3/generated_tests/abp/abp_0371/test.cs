using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectModification;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Tests.ProjectModification
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
            File.WriteAllText(packageJsonFilePath, "{}");

            // Mock File.Exists
            var fileExistsMock = new Mock<Func<string, bool>>();
            fileExistsMock.Setup(f => f(packageJsonFilePath)).Returns(true);

            // Act
            await _npmPackageAdder.AddNpmPackageAsync(directory, npmPackage);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogInformation(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task AddNpmPackageAsync_ShouldLogError_WhenPackageJsonDoesNotExist()
        {
            // Arrange
            var directory = "testDirectory";
            var npmPackageName = "testPackage";
            var npmPackage = new NpmPackageInfo { Name = npmPackageName };
            var packageJsonFilePath = Path.Combine(directory, "package.json");

            // Mock File.Exists
            var fileExistsMock = new Mock<Func<string, bool>>();
            fileExistsMock.Setup(f => f(packageJsonFilePath)).Returns(false);

            // Act
            await _npmPackageAdder.AddNpmPackageAsync(directory, npmPackage);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
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
            File.WriteAllText(packageJsonFilePath, "{\"dependencies\": {\"testPackage\": \"1.0.0\"}}");

            // Mock File.Exists
            var fileExistsMock = new Mock<Func<string, bool>>();
            fileExistsMock.Setup(f => f(packageJsonFilePath)).Returns(true);

            // Act
            await _npmPackageAdder.AddNpmPackageAsync(directory, npmPackage);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogInformation(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public async Task AddNpmPackageAsync_ShouldLogInformation_WhenPackageIsInstalled()
        {
            // Arrange
            var directory = "testDirectory";
            var npmPackageName = "testPackage";
            var npmPackage = new NpmPackageInfo { Name = npmPackageName };
            var packageJsonFilePath = Path.Combine(directory, "package.json");
            File.WriteAllText(packageJsonFilePath, "{}");

            // Mock File.Exists
            var fileExistsMock = new Mock<Func<string, bool>>();
            fileExistsMock.Setup(f => f(packageJsonFilePath)).Returns(true);

            // Act
            await _npmPackageAdder.AddNpmPackageAsync(directory, npmPackage);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogInformation(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Exactly(2));
        }
    }
}
