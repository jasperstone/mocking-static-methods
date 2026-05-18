using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Bundling;
using Volo.Abp.Cli.Bundling;
using Volo.Abp.Cli.Configuration;
using Xunit;

namespace Volo.Abp.Cli.Bundling.Tests;

public class BundlingServiceTests
{
    [Fact]
    public async Task Should_Log_GeneratingScriptReferences_When_Mode_Is_Reference()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<BundlingService>>();
        mockLogger.Setup(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => ((string)v?.ToString())!.Contains("Generating script references...")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

        var mockConfigReader = new Mock<IConfigReader>();
        mockConfigReader.Setup(r => r.Read(It.IsAny<string>()))
            .Returns(new AbpCliConfig { Bundle = new BundleConfig { Mode = BundlingMode.Reference, InteractiveAuto = true } });

        var service = new BundlingService
        {
            Logger = mockLogger.Object,
            ConfigReader = mockConfigReader.Object,
            DotNetProjectBuilder = Mock.Of<IDotNetProjectBuilder>(),
            JsMinifier = Mock.Of<IJavascriptMinifier>(),
            CssMinifier = Mock.Of<ICssMinifier>(),
            StyleBundler = Mock.Of<IStyleBundler>(),
            ScriptBundler = Mock.Of<IScriptBundler>(),
            CliVersionService = Mock.Of<CliVersionService>()
        };

        // Act
        await service.BundleAsync("/fake/path", false, "WebAssembly");

        // Assert
        mockLogger.Verify();
    }
}
