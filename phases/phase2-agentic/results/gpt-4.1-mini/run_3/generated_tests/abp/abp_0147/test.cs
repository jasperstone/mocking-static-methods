using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Bundling;
using Volo.Abp.Cli.Bundling.Scripts;
using Volo.Abp.Cli.Bundling.Styles;
using Volo.Abp.Cli.Configuration;
using Xunit;

namespace Volo.Abp.Cli.Bundling.Tests
{
    public class BundlingServiceTests
    {
        [Fact]
        public async Task BundleAsync_LogsInformation_WhenGeneratingScriptReferences()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BundlingService>>();
            var styleBundlerMock = new Mock<IStyleBundler>();
            var scriptBundlerMock = new Mock<IScriptBundler>();
            var configReaderMock = new Mock<IConfigReader>();
            var dotNetProjectBuilderMock = new Mock<IDotNetProjectBuilder>();

            var bundleConfig = new BundleConfig
            {
                Mode = BundlingMode.None,
                Name = null,
                InteractiveAuto = true,
                IsBlazorWebApp = false,
                Parameters = new Dictionary<string, string>()
            };

            var config = new CliConfiguration
            {
                Bundle = bundleConfig
            };

            configReaderMock.Setup(c => c.Read(It.IsAny<string>())).Returns(config);

            var bundlingService = new BundlingService
            {
                Logger = loggerMock.Object,
                StyleBundler = styleBundlerMock.Object,
                ScriptBundler = scriptBundlerMock.Object,
                ConfigReader = configReaderMock.Object,
                DotNetProjectBuilder = dotNetProjectBuilderMock.Object
            };

            // Setup StyleBundler and ScriptBundler to return dummy strings
            styleBundlerMock.Setup(s => s.Bundle(It.IsAny<BundleOptions>(), It.IsAny<BundleContext>())).Returns("style bundle");
            scriptBundlerMock.Setup(s => s.Bundle(It.IsAny<BundleOptions>(), It.IsAny<BundleContext>())).Returns("script bundle");

            // Setup GenerateStyleDefinitions and GenerateScriptDefinitions via reflection since they are private
            var styleDefinitions = "style definitions";
            var scriptDefinitions = "script definitions";

            // We will mock these private methods by creating a derived class that overrides them
            var testService = new TestBundlingService(bundlingService)
            {
                GenerateStyleDefinitionsReturn = styleDefinitions,
                GenerateScriptDefinitionsReturn = scriptDefinitions
            };

            // Act
            // We need to call BundleAsync with a directory that contains a .csproj file.
            // Since the method reads files from disk, we will mock Directory.GetFiles via a shim or we can override the method.
            // But since we cannot mock static methods easily here, we will simulate by calling the private method that contains the logging.
            // Instead, we will test the private method that contains the logging for script references.
            // But since it's private, we will test the public method with a directory that has a .csproj file.
            // To avoid file system dependency, we will test the logging calls by calling the private method via reflection.

            // Instead, we will test the logging calls by invoking the BundleAsync method with a directory that has a .csproj file.
            // We will create a temporary directory and a dummy .csproj file.

            // For simplicity, we will just verify that the logger was called with the expected message "Generating script references..."

            // Assert
            await testService.InvokeBundleAsyncWithModeNone();

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Generating script references...")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class TestBundlingService : BundlingService
        {
            private readonly BundlingService _inner;

            public string GenerateStyleDefinitionsReturn { get; set; }
            public string GenerateScriptDefinitionsReturn { get; set; }

            public TestBundlingService(BundlingService inner)
            {
                _inner = inner;
                Logger = inner.Logger;
                StyleBundler = inner.StyleBundler;
                ScriptBundler = inner.ScriptBundler;
                ConfigReader = inner.ConfigReader;
                DotNetProjectBuilder = inner.DotNetProjectBuilder;
            }

            public async Task InvokeBundleAsyncWithModeNone()
            {
                // We will call the private method BundleAsync with Mode None by reflection
                // But since BundleAsync is public, we can call it directly with a directory that has a .csproj file.
                // To avoid file system dependency, we will override the ConfigReader to return a config with Mode None.

                // We will create a temporary directory and a dummy .csproj file.
                var tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
                System.IO.Directory.CreateDirectory(tempDir);
                var csprojPath = System.IO.Path.Combine(tempDir, "TestProject.csproj");
                System.IO.File.WriteAllText(csprojPath, "<Project></Project>");

                try
                {
                    await _inner.BundleAsync(tempDir, false, BundlingConsts.WebAssembly);
                }
                finally
                {
                    try
                    {
                        System.IO.File.Delete(csprojPath);
                        System.IO.Directory.Delete(tempDir);
                    }
                    catch { }
                }
            }
        }
    }
}
