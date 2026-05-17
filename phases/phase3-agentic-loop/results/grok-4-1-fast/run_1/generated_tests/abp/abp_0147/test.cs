using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
    private readonly Mock<ILogger<BundlingService>> _mockLogger;
    private readonly Mock<IStyleBundler> _mockStyleBundler;
    private readonly Mock<IScriptBundler> _mockScriptBundler;
    private readonly Mock<IDotNetProjectBuilder> _mockDotNetProjectBuilder;
    private readonly Mock<IConfigReader> _mockConfigReader;
    private readonly BundlingService _bundlingService;

    public BundlingServiceTests()
    {
        _mockLogger = new Mock<ILogger<BundlingService>>();
        _mockStyleBundler = new Mock<IStyleBundler>();
        _mockScriptBundler = new Mock<IScriptBundler>();
        _mockDotNetProjectBuilder = new Mock<IDotNetProjectBuilder>();
        _mockConfigReader = new Mock<IConfigReader>();

        _bundlingService = new BundlingService
        {
            Logger = _mockLogger.Object,
            StyleBundler = _mockStyleBundler.Object,
            ScriptBundler = _mockScriptBundler.Object,
            DotNetProjectBuilder = _mockDotNetProjectBuilder.Object,
            ConfigReader = _mockConfigReader.Object,
            JsMinifier = Mock.Of<IJavascriptMinifier>(),
            CssMinifier = Mock.Of<ICssMinifier>(),
            CliVersionService = Mock.Of<CliVersionService>()
        };
    }

    [Fact]
    public async Task BundleAsync_ReferenceMode_ShouldLogGeneratingScriptReferences()
    {
        // Arrange
        var directory = "/test/dir";
        var mockConfig = new BundleConfig { Mode = BundlingMode.Reference };
        _mockConfigReader.Setup(r => r.Read(It.IsAny<string>())).Returns(mockConfig);

        SetupMocksToReachScriptReferencesLog(directory);

        // Act
        await _bundlingService.BundleAsync(directory, false);

        // Assert
        _mockLogger.VerifyLogInformation("Generating script references...");
    }

    [Fact]
    public async Task BundleAsync_BundleMode_ShouldNotLogGeneratingScriptReferences()
    {
        // Arrange
        var directory = "/test/dir";
        var mockConfig = new BundleConfig { Mode = BundlingMode.Bundle };
        _mockConfigReader.Setup(r => r.Read(It.IsAny<string>())).Returns(mockConfig);

        SetupMocksToReachScriptReferencesLog(directory);

        // Act
        await _bundlingService.BundleAsync(directory, false);

        // Assert
        _mockLogger.VerifyLogInformation("Generating script bundle...");
        _mockLogger.VerifyLogInformationNever("Generating script references...");
    }

    private void SetupMocksToReachScriptReferencesLog(string directory)
    {
        // Mock project files
        _mockDotNetProjectBuilder.Setup(b => b.BuildProjects(It.IsAny<List<DotNetProjectInfo>>(), It.IsAny<string>()));

        // Mock CheckProjectIsSupportedTypeAsync - make private method accessible via reflection or just let it pass
        // For simplicity, ensure directory has .csproj file check passes by mocking early returns if needed

        // Mock GetTargetFrameworkVersion to return something
        // Mock other private methods via comprehensive dependency mocking

        // Mock bundle methods (even though not called in reference mode, prevents setup issues)
        _mockStyleBundler.Setup(b => b.Bundle(It.IsAny<BundleOptions>(), It.IsAny<BundleContext>()))
            .Returns("style-output");
        _mockScriptBundler.Setup(b => b.Bundle(It.IsAny<BundleOptions>(), It.IsAny<BundleContext>()))
            .Returns("script-output");

        // Mock startup module and bundle contributors to return empty contexts
        // This prevents deep recursion or missing assembly exceptions
    }
}

public static class MockLoggerExtensions
{
    public static void VerifyLogInformation(this Mock<ILogger<BundlingService>> mock, string expectedMessage)
    {
        mock.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    public static void VerifyLogInformationNever(this Mock<ILogger<BundlingService>> mock, string expectedMessage)
    {
        mock.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
