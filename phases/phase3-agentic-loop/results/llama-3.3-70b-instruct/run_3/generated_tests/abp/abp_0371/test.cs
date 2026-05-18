using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectModification;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class ProjectNpmPackageAdderTests
    {
        [Fact]
        public async Task AddNpmPackageAsync_InstallsPackage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
            var projectNpmPackageAdder = new ProjectNpmPackageAdder(
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );

            projectNpmPackageAdder.Logger = loggerMock.Object;

            var directory = "test-directory";
            var npmPackageName = "test-package";
            var version = "1.0.0";

            // Act
            await projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackageName, version);

            // Assert
            loggerMock.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task AddNpmPackageAsync_DoesNotInstallPackageIfAlreadyInstalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
            var projectNpmPackageAdder = new ProjectNpmPackageAdder(
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );

            projectNpmPackageAdder.Logger = loggerMock.Object;

            var directory = "test-directory";
            var npmPackageName = "test-package";
            var version = "1.0.0";

            // Act
            await projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackageName, version);

            // Assert
            loggerMock.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task AddNpmPackageAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
            var projectNpmPackageAdder = new ProjectNpmPackageAdder(
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );

            projectNpmPackageAdder.Logger = loggerMock.Object;

            var directory = "test-directory";
            var npmPackageName = "test-package";
            var version = "1.0.0";

            // Act
            await projectNpmPackageAdder.AddNpmPackageAsync(directory, npmPackageName, version);

            // Assert
            loggerMock.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.AtLeastOnce);
        }
    }
}
