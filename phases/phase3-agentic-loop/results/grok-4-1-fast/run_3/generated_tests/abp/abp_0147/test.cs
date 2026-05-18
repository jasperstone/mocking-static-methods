using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Volo.Abp.Bundling;
using Volo.Abp.Cli.Bundling;
using Volo.Abp.Cli.Bundling.Scripts;
using Volo.Abp.Cli.Bundling.Styles;
using Volo.Abp.Cli.Build;
using Volo.Abp.Cli.Configuration;
using Xunit;

namespace Volo.Abp.Cli.Bundling.Tests;

public class BundlingServiceTests
{
    [Fact]
    public async Task BundleAsync_Should_Log_GeneratingScriptReferences_When_Mode_Is_Not_Bundle_Or_BundleAndMinify()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BundlingService>>();
        loggerMock.Setup(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Generating script references...")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

        var configReaderMock = new Mock<IConfigReader>();
        var bundleConfig = new BundleConfig { Mode = BundlingMode.None, InteractiveAuto = true };
        var abpCliConfig = new AbpCliConfig { Bundle = bundleConfig };
        configReaderMock.Setup(x => x.Read(It.IsAny<string>())).Returns(abpCliConfig);

        var dotNetProjectBuilderMock = new Mock<IDotNetProjectBuilder>();
        dotNetProjectBuilderMock.Setup(x => x.BuildProjects(It.IsAny<List<DotNetProjectInfo>>(), It.IsAny<string>()));

        var styleBundlerMock = new Mock<IStyleBundler>();
        var scriptBundlerMock = new Mock<IScriptBundler>();

        var service = new BundlingService
        {
            Logger = loggerMock.Object,
            ConfigReader = configReaderMock.Object,
            DotNetProjectBuilder = dotNetProjectBuilderMock.Object,
            StyleBundler = styleBundlerMock.Object,
            ScriptBundler = scriptBundlerMock.Object
        };

        // Mock other dependencies and methods to avoid exceptions
        service.JsMinifier = Mock.Of<IJavascriptMinifier>();
        service.CssMinifier = Mock.Of<ICssMinifier>();
        service.CliVersionService = Mock.Of<CliVersionService>();
        service.DotNetProjectBuilder = dotNetProjectBuilderMock.Object;

        // Create minimal temp directory structure
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);
        
        var projectFile = Path.Combine(tempDir, "test.csproj");
        File.WriteAllText(projectFile, "<Project></Project>");

        // Mock assembly file and startup module
        var assemblyPath = Path.Combine(tempDir, "test.dll");
        File.WriteAllBytes(assemblyPath, new byte[1000]);

        // Setup mocks for methods that get called
        dotNetProjectBuilderMock.Setup(x => x.BuildProjects(It.Is<List<DotNetProjectInfo>>(p => p.Any(pi => pi.CsProjPath == projectFile)), It.IsAny<string>()));
        
        // Mock private methods using reflection or just let them be called since we control the flow
        Mock.Get(service.StyleBundler!).Setup(x => x.Bundle(It.IsAny<BundleOptions>(), It.IsAny<BundleContext>())).Returns("styles");
        Mock.Get(service.ScriptBundler!).Setup(x => x.Bundle(It.IsAny<BundleOptions>(), It.IsAny<BundleContext>())).Returns("scripts");

        try
        {
            // Act
            await service.BundleAsync(tempDir, false, "test");

            // Assert - Verify the specific LogInformation call on line 112
            loggerMock.Verify();
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, true); } catch { }
            }
        }
    }
}
