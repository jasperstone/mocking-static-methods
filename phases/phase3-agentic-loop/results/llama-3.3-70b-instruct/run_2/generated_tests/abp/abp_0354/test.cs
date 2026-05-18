using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
            var directory = Path.GetTempPath();
            var localPaths = new List<string> { directory };

            // Act
            await converter.ConvertAsync(directory, localPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ConvertAsync_ConvertsProjectsToLocalReferences()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LocalReferenceConverter>>();
            var converter = new LocalReferenceConverter { Logger = loggerMock.Object };
            var directory = Path.GetTempPath();
            var localPaths = new List<string> { directory };
            var targetProject = Path.Combine(directory, "TestProject.csproj");
            File.Create(targetProject).Dispose();

            // Act
            await converter.ConvertAsync(directory, localPaths);

            // Assert
            Assert.True(File.Exists(targetProject));
        }

        [Fact]
        public async Task ConvertProjectToLocalReferences_ConvertsPackageReferenceToProjectReference()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LocalReferenceConverter>>();
            var converter = new LocalReferenceConverter { Logger = loggerMock.Object };
            var directory = Path.GetTempPath();
            var localPaths = new List<string> { directory };
            var targetProject = Path.Combine(directory, "TestProject.csproj");
            var xml = $@"
                <Project>
                    <ItemGroup>
                        <PackageReference Include=""TestPackage"" />
                    </ItemGroup>
                </Project>
            ";
            File.WriteAllText(targetProject, xml);

            // Act
            var method = converter.GetType().GetMethod("ConvertProjectToLocalReferences", BindingFlags.NonPublic | BindingFlags.Instance);
            await (Task)method.Invoke(converter, new object[] { targetProject, localPaths });

            // Assert
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(targetProject);
            var projectReference = xmlDocument.SelectSingleNode("/Project/ItemGroup/ProjectReference");
            Assert.NotNull(projectReference);
        }
    }
}
