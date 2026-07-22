using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Volo.Abp.Bundling;
using Volo.Abp.Cli.Build;
using Volo.Abp.Cli.Bundling.Scripts;
using Volo.Abp.Cli.Bundling.Styles;
using Volo.Abp.Cli.Configuration;
using Volo.Abp.Cli.Version;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Minify.Scripts;
using Volo.Abp.Minify.Styles;
using Volo.Abp.Modularity;
using Xunit;

namespace Volo.Abp.Cli.Bundling;

public class BundlingServiceTests
{
    [Fact]
    public void BundleAsync_StyleBundleGeneratedSuccessfully_LogInformationCalled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BundlingService>>();
        var dotNetProjectBuilderMock = new Mock<IDotNetProjectBuilder>();
        var jsMinifierMock = new Mock<IJavascriptMinifier>();
        var cssMinifierMock = new Mock<ICssMinifier>();
        var scriptBundlerMock = new Mock<IScriptBundler>();
        var styleBundlerMock = new Mock<IStyleBundler>();
        var configReaderMock = new Mock<IConfigReader>();
        var cliVersionServiceMock = new Mock<CliVersionService>();

        var bundlingService = new BundlingService(
            loggerMock.Object,
            dotNetProjectBuilderMock.Object,
            jsMinifierMock.Object,
            cssMinifierMock.Object,
            scriptBundlerMock.Object,
            styleBundlerMock.Object,
            configReaderMock.Object,
            cliVersionServiceMock.Object
        );

        // Act
        bundlingService.BundleAsync("directory", false, "WebAssembly");

        // Assert
        loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
    }

    [Fact]
    public void BundleAsync_ScriptBundleGeneratedSuccessfully_LogInformationCalled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BundlingService>>();
        var dotNetProjectBuilderMock = new Mock<IDotNetProjectBuilder>();
        var jsMinifierMock = new Mock<IJavascriptMinifier>();
        var cssMinifierMock = new Mock<ICssMinifier>();
        var scriptBundlerMock = new Mock<IScriptBundler>();
        var styleBundlerMock = new Mock<IStyleBundler>();
        var configReaderMock = new Mock<IConfigReader>();
        var cliVersionServiceMock = new Mock<CliVersionService>();

        var bundlingService = new BundlingService(
            loggerMock.Object,
            dotNetProjectBuilderMock.Object,
            jsMinifierMock.Object,
            cssMinifierMock.Object,
            scriptBundlerMock.Object,
            styleBundlerMock.Object,
            configReaderMock.Object,
            cliVersionServiceMock.Object
        );

        // Act
        bundlingService.BundleAsync("directory", false, "WebAssembly");

        // Assert
        loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
    }

    [Fact]
    public void BundleAsync_StyleReferencesGenerated_LogInformationCalled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BundlingService>>();
        var dotNetProjectBuilderMock = new Mock<IDotNetProjectBuilder>();
        var jsMinifierMock = new Mock<IJavascriptMinifier>();
        var cssMinifierMock = new Mock<ICssMinifier>();
        var scriptBundlerMock = new Mock<IScriptBundler>();
        var styleBundlerMock = new Mock<IStyleBundler>();
        var configReaderMock = new Mock<IConfigReader>();
        var cliVersionServiceMock = new Mock<CliVersionService>();

        var bundlingService = new BundlingService(
            loggerMock.Object,
            dotNetProjectBuilderMock.Object,
            jsMinifierMock.Object,
            cssMinifierMock.Object,
            scriptBundlerMock.Object,
            styleBundlerMock.Object,
            configReaderMock.Object,
            cliVersionServiceMock.Object
        );

        // Act
        bundlingService.BundleAsync("directory", false, "WebAssembly");

        // Assert
        loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
    }

    [Fact]
    public void BundleAsync_ScriptReferencesGenerated_LogInformationCalled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BundlingService>>();
        var dotNetProjectBuilderMock = new Mock<IDotNetProjectBuilder>();
        var jsMinifierMock = new Mock<IJavascriptMinifier>();
        var cssMinifierMock = new Mock<ICssMinifier>();
        var scriptBundlerMock = new Mock<IScriptBundler>();
        var styleBundlerMock = new Mock<IStyleBundler>();
        var configReaderMock = new Mock<IConfigReader>();
        var cliVersionServiceMock = new Mock<CliVersionService>();

        var bundlingService = new BundlingService(
            loggerMock.Object,
            dotNetProjectBuilderMock.Object,
            jsMinifierMock.Object,
            cssMinifierMock.Object,
            scriptBundlerMock.Object,
            styleBundlerMock.Object,
            configReaderMock.Object,
            cliVersionServiceMock.Object
        );

        // Act
        bundlingService.BundleAsync("directory", false, "WebAssembly");

        // Assert
        loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
    }
}
