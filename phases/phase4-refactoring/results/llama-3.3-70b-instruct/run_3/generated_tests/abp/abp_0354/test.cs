using Xunit;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace Volo.Abp.Cli.ProjectModification
{
    public class LocalReferenceConverterTests
    {
        [Fact]
        public async Task ConvertAsync_LogInformationCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LocalReferenceConverter>>();
            var converter = new LocalReferenceConverter { Logger = loggerMock.Object };
            var directory = Path.GetTempPath();
            var localPaths = new List<string> { Path.Combine(directory, "localProject.csproj") };

            // Act
            await converter.ConvertAsync(directory, localPaths);

            // Assert
            loggerMock.Verify(l => l.Log(
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
            var localPaths = new List<string> { Path.Combine(directory, "localProject.csproj") };
            var targetProject = Path.Combine(directory, "targetProject.csproj");

            // Create a test project file
            File.WriteAllText(targetProject, @"
<Project>
    <ItemGroup>
        <PackageReference Include=""packageName"" />
    </ItemGroup>
</Project>
");

            // Act
            await converter.ConvertAsync(directory, localPaths);

            // Assert
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(targetProject);
            var projectReferenceNodes = xmlDocument.SelectNodes("/Project/ItemGroup/ProjectReference");
            Assert.Empty(projectReferenceNodes);
        }
    }
}
