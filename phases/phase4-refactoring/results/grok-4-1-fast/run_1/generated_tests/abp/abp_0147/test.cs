using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Bundling;
using Volo.Abp.Cli.Bundling;
using Volo.Abp.Cli.Build;
using Volo.Abp.Cli.Configuration;
using Volo.Abp.Cli.Version;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.Bundling.Tests;

public class BundlingServiceTests
{
    [Fact]
    public async Task BundleAsync_Should_Log_GeneratingScriptReferences_When_Mode_Is_Reference()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<BundlingService>>();
        var mockConfigReader = new Mock<IConfigReader>();
        
        // Create config that returns Reference mode (not Bundle or BundleAndMinify)
        var bundleConfig = new BundleConfig 
        { 
            Mode = (BundlingMode)0, // Assuming 0 is Reference/else branch
            InteractiveAuto = true // Skip file update section
        };
        var config = new AbpCliConfig { Bundle = bundleConfig };
        mockConfigReader.Setup(x => x.Read(It.IsAny<string>())).Returns(config);

        var mockDotNetProjectBuilder = new Mock<IDotNetProjectBuilder>();
        mockDotNetProjectBuilder.Setup(x => x.BuildProjects(It.IsAny<IList<DotNetProjectInfo>>(), It.IsAny<string>()));

        var mockScriptBundler = new Mock<IScriptBundler>();
        mockScriptBundler.Setup(x => x.Bundle(It.IsAny<BundleOptions>(), It.IsAny<BundleContext>())).Returns("mock-script");

        var mockStyleBundler = new Mock<IStyleBundler>();
        mockStyleBundler.Setup(x => x.Bundle(It.IsAny<BundleOptions>(), It.IsAny<BundleContext>())).Returns("mock-style");

        var service = new BundlingService
        {
            Logger = mockLogger.Object,
            ConfigReader = mockConfigReader.Object,
            DotNetProjectBuilder = mockDotNetProjectBuilder.Object,
            ScriptBundler = mockScriptBundler.Object,
            StyleBundler = mockStyleBundler.Object,
            JsMinifier = Mock.Of<IJavascriptMinifier>(),
            CssMinifier = Mock.Of<ICssMinifier>(),
            CliVersionService = Mock.Of<CliVersionService>()
        };

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")[..10]);
        try
        {
            Directory.CreateDirectory(tempDir);
            File.WriteAllText(Path.Combine(tempDir, "Test.csproj"), "<Project/>");

            // Mock Directory.GetFiles
            var originalGetFiles = typeof(Directory).GetMethod("GetFiles", new[] { typeof(string), typeof(string) });
            
            // Act
            await service.BundleAsync(tempDir, false, "WebAssembly");
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }

        // Assert - Verify the specific LogInformation call on line 112
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Generating script references...")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
