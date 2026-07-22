using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Volo.Abp.Bundling;
using Volo.Abp.Cli.Bundling;
using Volo.Abp.Cli.Configuration;
using Xunit;

namespace Volo.Abp.Cli.Bundling.Tests;

public class BundlingServiceTests
{
    private readonly Mock<ILogger<BundlingService>> _mockLogger;

    public BundlingServiceTests()
    {
        _mockLogger = new Mock<ILogger<BundlingService>>();
        _mockLogger.SetupAllProperties();
    }

    [Fact]
    public async void BundleAsync_ReferencesMode_LogsGeneratingScriptReferences()
    {
        // Arrange
        var mockConfigReader = new Mock<IConfigReader>();
        mockConfigReader.Setup(r => r.Read(It.IsAny<string>()))
            .Returns(new AbpCliConfig { Bundle = new BundleConfig { Mode = BundlingMode.References, InteractiveAuto = true } });

        var service = new BundlingService
        {
            Logger = _mockLogger.Object,
            DotNetProjectBuilder = Mock.Of<Volo.Abp.Cli.Build.IDotNetProjectBuilder>(),
            JsMinifier = Mock.Of<Volo.Abp.Minify.Scripts.IJavascriptMinifier>(),
            CssMinifier = Mock.Of<Volo.Abp.Minify.Styles.ICssMinifier>(),
            ScriptBundler = Mock.Of<Volo.Abp.Cli.Bundling.Scripts.IScriptBundler>(),
            StyleBundler = Mock.Of<Volo.Abp.Cli.Bundling.Styles.IStyleBundler>(),
            ConfigReader = mockConfigReader.Object,
            CliVersionService = Mock.Of<Volo.Abp.Cli.Version.CliVersionService>()
        };

        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            File.WriteAllText(Path.Combine(directory, "test.csproj"), "<Project></Project>");

            // Act
            await service.BundleAsync(directory, false, Volo.Abp.Cli.Bundling.BundlingConsts.WebAssembly);

            // Assert - verifies line 112 Logger.LogInformation("Generating script references...")
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v?.ToString()?.Contains("Generating script references...") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
    }

    [Fact]
    public async void BundleAsync_BundleMode_LogsGeneratingScriptBundle_NotReferences()
    {
        // Arrange
        var mockConfigReader = new Mock<IConfigReader>();
        mockConfigReader.Setup(r => r.Read(It.IsAny<string>()))
            .Returns(new AbpCliConfig { Bundle = new BundleConfig { Mode = BundlingMode.Bundle, InteractiveAuto = true } });

        var service = new BundlingService
        {
            Logger = _mockLogger.Object,
            DotNetProjectBuilder = Mock.Of<Volo.Abp.Cli.Build.IDotNetProjectBuilder>(),
            JsMinifier = Mock.Of<Volo.Abp.Minify.Scripts.IJavascriptMinifier>(),
            CssMinifier = Mock.Of<Volo.Abp.Minify.Styles.ICssMinifier>(),
            ScriptBundler = Mock.Of<Volo.Abp.Cli.Bundling.Scripts.IScriptBundler>(),
            StyleBundler = Mock.Of<Volo.Abp.Cli.Bundling.Styles.IStyleBundler>(),
            ConfigReader = mockConfigReader.Object,
            CliVersionService = Mock.Of<Volo.Abp.Cli.Version.CliVersionService>()
        };

        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            File.WriteAllText(Path.Combine(directory, "test.csproj"), "<Project></Project>");

            // Act
            await service.BundleAsync(directory, false, Volo.Abp.Cli.Bundling.BundlingConsts.WebAssembly);

            // Assert - logs bundle message, NOT references message (line 112)
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v?.ToString()?.Contains("Generating script bundle...") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v?.ToString()?.Contains("Generating script references...") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
    }
}
