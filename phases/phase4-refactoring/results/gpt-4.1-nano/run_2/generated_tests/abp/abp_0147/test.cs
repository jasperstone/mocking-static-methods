using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.IO;
using System;

namespace Volo.Abp.Cli.Bundling.Tests
{
    public class BundlingServiceTests
    {
        [Fact]
        public async Task BundleAsync_Should_Log_GenerateScriptReferences_When_Mode_Is_None()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<BundlingService>>();
            var mockScriptBundler = new Mock<IScriptBundler>();
            var mockStyleBundler = new Mock<IStyleBundler>();
            var mockConfigReader = new Mock<IConfigReader>();

            var service = new BundlingService
            {
                Logger = mockLogger.Object,
                ScriptBundler = mockScriptBundler.Object,
                StyleBundler = mockStyleBundler.Object,
                ConfigReader = mockConfigReader.Object
            };

            // Setup config to trigger the else branch
            var bundleConfig = new BundleConfig
            {
                Mode = BundlingMode.None,
                InteractiveAuto = true,
                IsBlazorWebApp = false
            };

            var mockConfig = new { Bundle = bundleConfig };
            mockConfigReader.Setup(r => r.Read(It.IsAny<string>())).Returns(mockConfig);

            // Create a temporary directory with a dummy project file
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var csprojPath = Path.Combine(tempDir, "Test.csproj");
            File.WriteAllText(csprojPath, "<Project></Project>");

            // Act
            await service.BundleAsync(tempDir, false);

            // Assert
            mockLogger.Verify(l => l.LogInformation("Generating style references..."), Times.Once);
            mockLogger.Verify(l => l.LogInformation("Generating script references..."), Times.Once);

            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }
}
