using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectModification;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class ProjectNpmPackageAdderTests
    {
        private readonly Mock<ILogger<ProjectNpmPackageAdder>> _loggerMock;
        private readonly Mock<INpmPackageInfoProvider> _npmPackageInfoProviderMock;
        private readonly Mock<ICmdHelper> _cmdHelperMock;
        private readonly Mock<AngularSourceCodeAdder> _angularSourceCodeAdderMock;

        public ProjectNpmPackageAdderTests()
        {
            _loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
            _npmPackageInfoProviderMock = new Mock<INpmPackageInfoProvider>();
            _cmdHelperMock = new Mock<ICmdHelper>();
            _angularSourceCodeAdderMock = new Mock<AngularSourceCodeAdder>();
        }

        [Fact]
        public async Task AddNpmPackageAsync_LogsInformation_WhenPackageIsInstalled()
        {
            // Arrange
            var packageInfo = new NpmPackageInfo { Name = "test-package" };
            var directory = "test-directory";
            var packageJsonFilePath = Path.Combine(directory, "package.json");
            File.Create(packageJsonFilePath).Dispose();

            var projectNpmPackageAdder = new ProjectNpmPackageAdder(
                _loggerMock.Object,
                _npmPackageInfoProviderMock.Object,
                _cmdHelperMock.Object,
                _angularSourceCodeAdderMock.Object
            );

            // Act
            await projectNpmPackageAdder.AddNpmPackageAsync(directory, packageInfo);

            // Assert
            _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task AddNpmPackageAsync_LogsInformation_WhenPackageIsAlreadyInstalled()
        {
            // Arrange
            var packageInfo = new NpmPackageInfo { Name = "test-package" };
            var directory = "test-directory";
            var packageJsonFilePath = Path.Combine(directory, "package.json");
            File.Create(packageJsonFilePath).Dispose();
            File.WriteAllText(packageJsonFilePath, $"{{\"dependencies\":{{\"{packageInfo.Name}\":\"1.0.0\"}}}}");

            var projectNpmPackageAdder = new ProjectNpmPackageAdder(
                _loggerMock.Object,
                _npmPackageInfoProviderMock.Object,
                _cmdHelperMock.Object,
                _angularSourceCodeAdderMock.Object
            );

            // Act
            await projectNpmPackageAdder.AddNpmPackageAsync(directory, packageInfo);

            // Assert
            _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task AddNpmPackageAsync_CallsCmdHelper_RunCmd_WhenPackageIsNotInstalled()
        {
            // Arrange
            var packageInfo = new NpmPackageInfo { Name = "test-package" };
            var directory = "test-directory";
            var packageJsonFilePath = Path.Combine(directory, "package.json");
            File.Create(packageJsonFilePath).Dispose();

            var projectNpmPackageAdder = new ProjectNpmPackageAdder(
                _loggerMock.Object,
                _npmPackageInfoProviderMock.Object,
                _cmdHelperMock.Object,
                _angularSourceCodeAdderMock.Object
            );

            // Act
            await projectNpmPackageAdder.AddNpmPackageAsync(directory, packageInfo);

            // Assert
            _cmdHelperMock.Verify(c => c.RunCmd(It.IsAny<string>()), Times.Once);
        }
    }
}
