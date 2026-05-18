using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Xunit;
using Volo.Abp.Cli.Bundling;
using Volo.Abp.Cli.Bundling.Styles;
using Volo.Abp.Cli.Bundling.Scripts;
using Volo.Abp.Cli.Configuration;
using Volo.Abp.Cli.Build;
using Volo.Abp.Minify.Styles;
using Volo.Abp.Minify.Scripts;
using Volo.Abp.Cli.Version;

public class BundlingServiceTests
{
    private readonly Mock<ILogger<BundlingService>> _loggerMock;
    private readonly Mock<IStyleBundler> _styleBundlerMock;
    private readonly Mock<IScriptBundler> _scriptBundlerMock;
    private readonly Mock<IConfigReader> _configReaderMock;
    private readonly Mock<IDotNetProjectBuilder> _dotNetProjectBuilderMock;
    private readonly Mock<ICssMinifier> _cssMinifierMock;
    private readonly Mock<IJavascriptMinifier> _jsMinifierMock;
    private readonly Mock<ICliVersionService> _cliVersionServiceMock;

    public BundlingServiceTests()
    {
        _loggerMock = new Mock<ILogger<BundlingService>>();
        _styleBundlerMock = new Mock<IStyleBundler>();
        _scriptBundlerMock = new Mock<IScriptBundler>();
        _configReaderMock = new Mock<IConfigReader>();
        _dotNetProjectBuilderMock = new Mock<IDotNetProjectBuilder>();
        _cssMinifierMock = new Mock<ICssMinifier>();
        _jsMinifierMock = new Mock<IJavascriptMinifier>();
        _cliVersionServiceMock = new Mock<ICliVersionService>();
    }

    [Fact]
    public async Task BundleAsync_LogsGeneratingScriptReferences()
    {
        // Arrange
        var bundlingService = new BundlingService
        {
            Logger = _loggerMock.Object,
            StyleBundler = _styleBundlerMock.Object,
            ScriptBundler = _scriptBundlerMock.Object,
            ConfigReader = _configReaderMock.Object,
            DotNetProjectBuilder = _dotNetProjectBuilderMock.Object,
            CssMinifier = _cssMinifierMock.Object,
            JsMinifier = _jsMinifierMock.Object,
            CliVersionService = _cliVersionServiceMock.Object
        };

        var directory = "test_directory";
        var forceBuild = false;
        var projectType = "WebAssembly";

        // Act
        await bundlingService.BundleAsync(directory, forceBuild, projectType);

        // Assert
        _loggerMock.Verify(
            logger => logger.LogInformation("Generating script references..."),
            Times.Once);
    }
}
