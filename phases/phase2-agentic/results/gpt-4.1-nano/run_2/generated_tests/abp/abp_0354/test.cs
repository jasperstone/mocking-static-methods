using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectModification;

namespace Volo.Abp.Cli.Tests
{
    public class LocalReferenceConverterTests
    {
        private readonly Mock<ILogger<LocalReferenceConverter>> _loggerMock;
        private readonly LocalReferenceConverter _converter;

        public LocalReferenceConverterTests()
        {
            _loggerMock = new Mock<ILogger<LocalReferenceConverter>>();
            _converter = new LocalReferenceConverter
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task ConvertAsync_ShouldLogInformationAndProcessProjects()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var projectFilePath = Path.Combine(tempDir, "TestProject.csproj");
            File.WriteAllText(projectFilePath, "<Project></Project>");
            var localPaths = new List<string> { tempDir };

            // Act
            await _converter.ConvertAsync(tempDir, localPaths);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(It.Is<string>(s => s.Contains("Converting projects to local reference"))),
                Times.AtLeastOnce);
            _loggerMock.Verify(
                x => x.LogInformation(It.Is<string>(s => s.Contains("Converting to local reference"))),
                Times.AtLeastOnce);
            _loggerMock.Verify(
                x => x.LogInformation(It.Is<string>(s => s.Contains("Converted"))),
                Times.AtLeastOnce);

            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }
}
