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
        [Fact]
        public async Task AddNpmPackageAsync_LogsInformation_WhenPackageJsonExistsAndPackageNotInstalled()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var packageJsonPath = Path.Combine(tempDir, "package.json");
            var npmPackageName = "test-package";
            var version = "1.2.3";

            // Write a package.json file that does NOT contain the package name
            File.WriteAllText(packageJsonPath, "{ \"dependencies\": { } }");

            var loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var npmPackageInfoProviderMock = new Mock<INpmPackageInfoProvider>();
            var angularSourceCodeAdderMock = new Mock<AngularSourceCodeAdder>();
            var sourceCodeDownloadServiceMock = new Mock<SourceCodeDownloadService>();
            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            var installLibsServiceMock = new Mock<IInstallLibsService>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            var jsonSerializerMock = new Mock<IJsonSerializer>();

            var npmPackage = new NpmPackageInfo { Name = npmPackageName };

            var adder = new ProjectNpmPackageAdder(
                jsonSerializerMock.Object,
                sourceCodeDownloadServiceMock.Object,
                angularSourceCodeAdderMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                installLibsServiceMock.Object,
                cmdHelperMock.Object,
                cliHttpClientFactoryMock.Object,
                npmPackageInfoProviderMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await adder.AddNpmPackageAsync(tempDir, npmPackage, version);

            // Assert
            loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Installing '{npmPackageName}' package to the project '{packageJsonPath}'...")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("yarn add " + npmPackageName + "@" + version)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            cmdHelperMock.Verify(x => x.RunCmd(It.Is<string>(s => s.Contains("npx yarn add " + npmPackageName + "@" + version))), Times.Once);

            // Cleanup
            try
            {
                File.Delete(packageJsonPath);
                Directory.Delete(tempDir);
            }
            catch { }
        }
    }

    // Minimal interface and class stubs to allow compilation
    public interface ICmdHelper
    {
        void RunCmd(string command);
    }

    public interface INpmPackageInfoProvider
    {
        Task<System.Collections.Generic.List<NpmPackageInfo>> GetPackageListAsync();
    }

    public class AngularSourceCodeAdder
    {
        public virtual Task AddAsync(string directory, NpmPackageInfo npmPackage) => Task.CompletedTask;
    }

    public class SourceCodeDownloadService
    {
        public virtual Task DownloadNpmPackageAsync(string packageName, string targetFolder, string version) => Task.CompletedTask;
    }

    public interface IRemoteServiceExceptionHandler { }

    public interface IInstallLibsService
    {
        Task InstallLibsAsync(string directory);
    }

    public class CliHttpClientFactory { }

    public interface IJsonSerializer { }

    public class NpmPackageInfo
    {
        public string Name { get; set; }
    }
}
