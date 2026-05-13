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

namespace Volo.Abp.Cli.Core.Tests
{
    public class LocalReferenceConverterTests
    {
        [Fact]
        public async Task ConvertAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LocalReferenceConverter>>();
            var converter = new LocalReferenceConverter { Logger = loggerMock.Object };
            var directory = Path.Combine(Directory.GetCurrentDirectory(), "TestDirectory");
            var localPaths = new List<string> { Path.Combine(directory, "LocalProject.csproj") };

            // Act
            await converter.ConvertAsync(directory, localPaths);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ConvertAsync_ConvertsProjectsToLocalReferences()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LocalReferenceConverter>>();
            var converter = new LocalReferenceConverter { Logger = loggerMock.Object };
            var directory = Path.Combine(Directory.GetCurrentDirectory(), "TestDirectory");
            var localPaths = new List<string> { Path.Combine(directory, "LocalProject.csproj") };
            var targetProject = Path.Combine(directory, "TargetProject.csproj");

            // Create test files
            Directory.CreateDirectory(directory);
            File.WriteAllText(targetProject, "<Project><ItemGroup><PackageReference Include=\"LocalProject\" /></ItemGroup></Project>");

            // Act
            await converter.ConvertAsync(directory, localPaths);

            // Assert
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(targetProject);
            var projectReferenceNodes = xmlDocument.SelectNodes("/Project/ItemGroup/ProjectReference");
            Assert.NotNull(projectReferenceNodes);
            Assert.Single(projectReferenceNodes);

            // Clean up
            Directory.Delete(directory, true);
        }
    }
}
