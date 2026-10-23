using Xunit;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Volo.Abp.Cli.ProjectModification
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
            loggerMock.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ConvertAsync_ConvertsProjectsToLocalReferences()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LocalReferenceConverter>>();
            var converter = new LocalReferenceConverter { Logger = loggerMock.Object };
            var directory = Path.GetTempPath();
            var localPaths = new List<string> { Path.Combine(directory, "localProject.csproj") };
            var targetProject = Path.Combine(directory, "targetProject.csproj");

            // Create a test project file
            File.WriteAllText(targetProject, "<Project><ItemGroup><PackageReference Include=\"TestPackage\" /></ItemGroup></Project>");

            // Act
            await converter.ConvertAsync(directory, localPaths);

            // Assert
            var projectFileContent = File.ReadAllText(targetProject);
            Assert.Contains("<ProjectReference", projectFileContent);
        }
    }
}
