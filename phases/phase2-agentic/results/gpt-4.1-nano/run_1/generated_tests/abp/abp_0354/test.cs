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
        public async Task ConvertAsync_ShouldLogInformation_WhenCalled()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var csprojPath = Path.Combine(tempDir, "TestProject.csproj");
            File.WriteAllText(csprojPath, "<Project></Project>");
            var localPaths = new List<string> { tempDir };

            // Act
            await _converter.ConvertAsync(tempDir, localPaths);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Converting projects to local reference.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
            Directory.Delete(tempDir, true);
        }
    }
}
