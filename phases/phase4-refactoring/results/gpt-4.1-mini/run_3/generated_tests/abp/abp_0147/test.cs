using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Bundling;
using Volo.Abp.Cli.Bundling.Scripts;
using Volo.Abp.Cli.Bundling.Styles;
using Volo.Abp.Cli.Configuration;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.Volo.Abp.Cli.Bundling
{
    public class BundlingServiceTests
    {
        [Fact]
        public async Task BundleAsync_LogsInformation_CallsLogInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BundlingService>>();
            var styleBundlerMock = new Mock<IStyleBundler>();
            var scriptBundlerMock = new Mock<IScriptBundler>();
            var configReaderMock = new Mock<IConfigReader>();

            var bundleConfig = new BundleConfig
            {
                Mode = BundlingMode.Bundle,
                Name = "test",
                InteractiveAuto = true,
                Parameters = new BundleParameterDictionary(),
                IsBlazorWebApp = false
            };

            var configMock = new Mock<IConfig>();
            configMock.SetupGet(c => c.Bundle).Returns(bundleConfig);

            configReaderMock.Setup(c => c.Read(It.IsAny<string>())).Returns(configMock.Object);
            styleBundlerMock.Setup(s => s.Bundle(It.IsAny<BundleOptions>(), It.IsAny<BundleContext>())).Returns("style");
            scriptBundlerMock.Setup(s => s.Bundle(It.IsAny<BundleOptions>(), It.IsAny<BundleContext>())).Returns("script");

            var bundlingService = new BundlingService
            {
                Logger = loggerMock.Object,
                StyleBundler = styleBundlerMock.Object,
                ScriptBundler = scriptBundlerMock.Object,
                ConfigReader = configReaderMock.Object,
                DotNetProjectBuilder = Mock.Of<IDotNetProjectBuilder>(),
                JsMinifier = Mock.Of<IJavascriptMinifier>(),
                CssMinifier = Mock.Of<ICssMinifier>(),
                CliVersionService = Mock.Of<CliVersionService>()
            };

            // Create a temporary directory with a dummy .csproj file to pass the file check
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var csprojPath = Path.Combine(tempDir, "TestProject.csproj");
            File.WriteAllText(csprojPath, "<Project></Project>");

            try
            {
                // Act & Assert
                await Assert.ThrowsAnyAsync<Exception>(() => bundlingService.BundleAsync(tempDir, false, BundlingConsts.WebAssembly));
            }
            finally
            {
                File.Delete(csprojPath);
                Directory.Delete(tempDir);
            }

            // Verify that Logger.LogInformation was called with expected messages
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Generating style bundle...")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);

            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Style bundle has been generated successfully.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);

            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Generating script bundle...")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);

            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Script bundle has been generated successfully.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }
    }
}
