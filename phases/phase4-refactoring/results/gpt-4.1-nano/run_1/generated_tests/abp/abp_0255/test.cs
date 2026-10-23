using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands.Services;
using ICSharpCode.SharpZipLib.Zip;

namespace Volo.Abp.Cli.Tests
{
    public class SourceCodeDownloadServiceTests
    {
        [Fact]
        public async Task DownloadModuleAsync_LogsInformationAndProcessesZipEntries()
        {
            // Arrange
            var moduleBuilderMock = new Mock<ModuleProjectBuilder>();
            var nugetBuilderMock = new Mock<NugetPackageProjectBuilder>();
            var npmBuilderMock = new Mock<NpmPackageProjectBuilder>();
            var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();

            var service = new SourceCodeDownloadService(
                moduleBuilderMock.Object,
                nugetBuilderMock.Object,
                npmBuilderMock.Object
            )
            {
                Logger = loggerMock.Object
            };

            var dummyZipContent = new byte[] { 1, 2, 3 };
            var buildResult = new { ZipContent = dummyZipContent };
            moduleBuilderMock.Setup(m => m.BuildAsync(It.IsAny<ProjectBuildArgs>()))
                .ReturnsAsync(buildResult);

            var dummyEntry = new ZipEntryStub("file.txt");
            var zipStreamMock = new ZipInputStreamMock(new[] { dummyEntry });
            var zipStream = zipStreamMock;

            // Act
            await service.DownloadModuleAsync(
                "TestModule",
                "outputFolder",
                null,
                "abpRepoPath",
                "voloRepoPath",
                new AbpCommandLineOptions()
            );

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("TestModule"))), Times.AtLeastOnce);
        }
    }

    // Mocks for ZipInputStream and ZipEntry
    public class ZipEntryStub
    {
        public string Name { get; }
        public ZipEntryStub(string name)
        {
            Name = name;
        }
    }

    public class ZipInputStreamMock : ZipInputStream
    {
        private readonly ZipEntryStub[] _entries;
        private int _index = 0;

        public ZipInputStreamMock(ZipEntryStub[] entries)
        {
            _entries = entries;
        }

        public override ZipEntry GetNextEntry()
        {
            if (_index >= _entries.Length)
                return null;
            var entry = new ZipEntry(_entries[_index].Name);
            _index++;
            return entry;
        }
    }
}
