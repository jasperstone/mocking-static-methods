using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Volo.Abp.Cli.ProjectModification;
using Xunit;

namespace Volo.Abp.Cli.ProjectModification.Tests
{
    public class LocalReferenceConverterTests
    {
        [Fact]
        public async Task ConvertAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LocalReferenceConverter>>();
            var converter = new LocalReferenceConverter { Logger = loggerMock.Object };
            var directory = Path.GetTempPath();
            var localPaths = new List<string> { Path.Combine(directory, "localProject.csproj") };

            // Act
            await converter.ConvertAsync(directory, localPaths);

            // Assert
            loggerMock.Verify(logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ConvertAsync_ConvertsProjectsToLocalReferences()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LocalReferenceConverter>>();
            var converter = new LocalReferenceConverter { Logger = loggerMock.Object };
            var directory = Path.GetTempPath();
            var localPaths = new List<string> { Path.Combine(directory, "TestPackage.csproj") };
            var targetProject = Path.Combine(directory, "targetProject.csproj");

            // Create a test project file
            var projectFileContent = @"
<Project>
    <ItemGroup>
        <PackageReference Include=""TestPackage"" />
    </ItemGroup>
</Project>
";
            File.WriteAllText(targetProject, projectFileContent);

            // Create a local project file
            var localProjectFileContent = @"
<Project>
</Project>
";
            File.WriteAllText(Path.Combine(directory, "TestPackage.csproj"), localProjectFileContent);

            // Act
            await converter.ConvertAsync(directory, localPaths);

            // Assert
            var projectFileContentAfterConversion = File.ReadAllText(targetProject);
            Assert.DoesNotContain("<PackageReference", projectFileContentAfterConversion);
            Assert.Contains("<ProjectReference", projectFileContentAfterConversion);
        }
    }
}
