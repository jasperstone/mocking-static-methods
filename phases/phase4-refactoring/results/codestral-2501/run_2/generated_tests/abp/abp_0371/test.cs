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

            var npmPackageInfo = new NpmPackageInfo { Name = "test-package" };
            var findNpmPackageInfoAsyncMock = new Mock<Func<string, Task<NpmPackageInfo>>>();
            findNpmPackageInfoAsyncMock.Setup(x => x(It.IsAny<string>())).ReturnsAsync(npmPackageInfo);
            _npmPackageAdder.FindNpmPackageInfoAsync = findNpmPackageInfoAsyncMock.Object;
        }

        [Fact]
        public async Task AddNpmPackageAsync_ShouldLogError_WhenPackageJsonNotFound()
        {
            // Arrange
            var directory = Path.Combine(Directory.GetCurrentDirectory(), "TestDirectory");
            var npmPackageName = "test-package";

            // Act
            await _npmPackageAdder.AddNpmPackageAsync(directory, npmPackageName);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("package.json not found!")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task AddNpmPackageAsync_ShouldLogInformation_WhenPackageIsAlreadyInstalled()
        {
            // Arrange
            var directory = Path.Combine(Directory.GetCurrentDirectory(), "TestDirectory");
            var npmPackageName = "test-package";
            var packageJsonContent = "{\"dependencies\": {\"test-package\": \"1.0.0\"}}";
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "package.json"), packageJsonContent);

            // Act
            await _npmPackageAdder.AddNpmPackageAsync(directory, npmPackageName);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("'test-package' is already installed.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task AddNpmPackageAsync_ShouldLogInformation_WhenInstallingPackage()
        {
            // Arrange
            var directory = Path.Combine(Directory.GetCurrentDirectory(), "TestDirectory");
            var npmPackageName = "test-package";
            var packageJsonContent = "{\"dependencies\": {}}";
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "package.json"), packageJsonContent);

            // Act
            await _npmPackageAdder.AddNpmPackageAsync(directory, npmPackageName);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("yarn add test-package")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
