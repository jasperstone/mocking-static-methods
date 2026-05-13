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
        private readonly Mock<INpmPackageInfoProvider> _npmPackageInfoProviderMock;
        private readonly ProjectNpmPackageAdder _projectNpmPackageAdder;

        public ProjectNpmPackageAdderTests()
        {
            _loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
            _cmdHelperMock = new Mock<ICmdHelper>();
            _npmPackageInfoProviderMock = new Mock<INpmPackageInfoProvider>();

            _projectNpmPackageAdder = new ProjectNpmPackageAdder(
                null,
                null,
                null,
                null,
                null,
                _cmdHelperMock.Object,
                null,
                _npmPackageInfoProviderMock.Object)
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
            var packageJsonContent = "{}";

            var packageJsonFilePath = Path.Combine(directory, "package.json");
            File.WriteAllText(packageJsonFilePath, packageJsonContent);

            _npmPackageInfoProviderMock.Setup(x => x.FindNpmPackageInfoAsync(npmPackageName))
                .ReturnsAsync(npmPackage);

            // Act
            await _projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackageName);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Installing '{npmPackageName}' package to the project '{packageJsonFilePath}'...")),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("yarn add " + npmPackageName)),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Clean up
            File.Delete(packageJsonFilePath);
        }

        [Fact]
        public async Task AddNpmPackageAsync_ShouldLogError_WhenPackageJsonDoesNotExist()
        {
            // Arrange
            var directory = "testDirectory";
            var npmPackageName = "testPackage";
            var npmPackage = new NpmPackageInfo { Name = npmPackageName };

            _npmPackageInfoProviderMock.Setup(x => x.FindNpmPackageInfoAsync(npmPackageName))
                .ReturnsAsync(npmPackage);

            // Act
            await _projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackageName);

            // Assert
            _loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("package.json not found!")),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
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
            var packageJsonContent = $"{{\"dependencies\": {{\"{npmPackageName}\": \"1.0.0\"}}}}";

            var packageJsonFilePath = Path.Combine(directory, "package.json");
            File.WriteAllText(packageJsonFilePath, packageJsonContent);

            _npmPackageInfoProviderMock.Setup(x => x.FindNpmPackageInfoAsync(npmPackageName))
                .ReturnsAsync(npmPackage);

            // Act
            await _projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackageName);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"'{npmPackageName}' is already installed.")),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Clean up
            File.Delete(packageJsonFilePath);
        }
    }
}
