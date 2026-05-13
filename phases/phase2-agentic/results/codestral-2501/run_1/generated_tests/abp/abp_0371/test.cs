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

            // Act
            await _npmPackageAdder.AddNpmPackageAsync(directory, npmPackage);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Installing '{npmPackageName}' package to the project '{packageJsonFilePath}'...")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            // Clean up
            File.Delete(packageJsonFilePath);
        }

        [Fact]
        public async Task AddNpmPackageAsync_ShouldLogInformation_WhenPackageIsAlreadyInstalled()
        {
            // Arrange
            var directory = "testDirectory";
            var npmPackageName = "testPackage";
            var npmPackage = new NpmPackageInfo { Name = npmPackageName };
            var packageJsonFilePath = Path.Combine(directory, "package.json");
            File.WriteAllText(packageJsonFilePath, $"{{\"dependencies\": {{\"{npmPackageName}\": \"1.0.0\"}}}}");

            // Act
            await _npmPackageAdder.AddNpmPackageAsync(directory, npmPackage);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"'{npmPackageName}' is already installed.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            // Clean up
            File.Delete(packageJsonFilePath);
        }

        [Fact]
        public async Task AddNpmPackageAsync_ShouldLogInformation_WhenAddingPackage()
        {
            // Arrange
            var directory = "testDirectory";
            var npmPackageName = "testPackage";
            var npmPackage = new NpmPackageInfo { Name = npmPackageName };
            var packageJsonFilePath = Path.Combine(directory, "package.json");
            File.WriteAllText(packageJsonFilePath, "{}");

            // Act
            await _npmPackageAdder.AddNpmPackageAsync(directory, npmPackage);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"yarn add {npmPackageName}")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            // Clean up
            File.Delete(packageJsonFilePath);
        }
    }
}
