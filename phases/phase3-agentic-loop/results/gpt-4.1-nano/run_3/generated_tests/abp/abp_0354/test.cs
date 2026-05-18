using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
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
            var testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(testDirectory);
            var csprojPath = Path.Combine(testDirectory, "TestProject.csproj");
            File.WriteAllText(csprojPath, "<Project></Project>");
            var localPaths = new List<string> { testDirectory };

            // Act
            await _converter.ConvertAsync(testDirectory, localPaths);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Converting projects to local reference.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
