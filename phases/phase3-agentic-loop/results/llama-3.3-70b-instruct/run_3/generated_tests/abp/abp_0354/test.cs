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
            var targetProject = Path.Combine(directory, "targetProject.csproj");

            // Create a test project file
            File.WriteAllText(targetProject, @"
<Project>
    <ItemGroup>
        <PackageReference Include=""TestPackage"" />
    </ItemGroup>
</Project>
");

            // Act
            await converter.ConvertAsync(directory, new List<string> { targetProject });

            // Assert
            loggerMock.Verify(logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception, string>>()
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
            File.WriteAllText(targetProject, @"
<Project>
    <ItemGroup>
        <PackageReference Include=""TestPackage"" />
    </ItemGroup>
</Project>
");

            // Create a test local project file
            File.WriteAllText(Path.Combine(directory, "TestPackage.csproj"), @"
<Project>
</Project>
");

            // Act
            await converter.ConvertAsync(directory, localPaths);

            // Assert
            var projectFileContent = File.ReadAllText(targetProject);
            Assert.DoesNotContain("<PackageReference", projectFileContent);
            Assert.Contains("<ProjectReference", projectFileContent);
        }
    }
}
