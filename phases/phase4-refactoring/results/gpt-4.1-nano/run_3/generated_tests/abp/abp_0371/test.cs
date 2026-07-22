using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.ProjectModification;

namespace Tests
{
    public class ProjectNpmPackageAdderTests
    {
        [Fact]
        public async Task AddNpmPackageAsync_Should_LogInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
            var adder = new ProjectNpmPackageAdder(
                null, null, null, null, null, null, null, null);
            adder.Logger = loggerMock.Object;

            var directory = Path.GetTempPath();
            var npmPackageName = "test-package";

            // Create a dummy package.json file
            var packageJsonPath = Path.Combine(directory, "package.json");
            File.WriteAllText(packageJsonPath, "{}");

            // We need to mock FindNpmPackageInfoAsync to return a dummy NpmPackageInfo
            // Since it's an instance method, we can create a derived class with an override
            var adderWithMock = new TestProjectNpmPackageAdder(
                loggerMock.Object);
            adderWithMock.SetupFindNpmPackageInfo(npmPackageName, new NpmPackageInfo { Name = npmPackageName });

            // Act
            await adderWithMock.AddNpmPackageAsync(directory, npmPackageName);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Installing '{npmPackageName}' package to the project")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        private class TestProjectNpmPackageAdder : ProjectNpmPackageAdder
        {
            private readonly NpmPackageInfo _mockedPackageInfo;

            public TestProjectNpmPackageAdder(ILogger<ProjectNpmPackageAdder> logger) : base(
                null, null, null, null, null, null, null, null)
            {
                this.Logger = logger;
            }

            public void SetupFindNpmPackageInfo(string packageName, NpmPackageInfo info)
            {
                _mockedPackageInfo = info;
            }

            public override async Task<NpmPackageInfo> FindNpmPackageInfoAsync(string packageName)
            {
                return await Task.FromResult(_mockedPackageInfo);
            }
        }
    }
}
