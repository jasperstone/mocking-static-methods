using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.Cli.Bundling;
using System.Collections.Generic;
using System.IO;
using System;

namespace Volo.Abp.Cli.Tests
{
    public class BundlingServiceTests
    {
        [Fact]
        public async Task BundleAsync_Should_LogInformation_ForStyleAndScriptReferences()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BundlingService>>();
            var styleBundlerMock = new Mock<IStyleBundler>();
            var scriptBundlerMock = new Mock<IScriptBundler>();
            var configReaderMock = new Mock<IConfigReader>();
            var projectBuilderMock = new Mock<IDotNetProjectBuilder>();
            var bundlingService = new BundlingService
            {
                Logger = loggerMock.Object,
                StyleBundler = styleBundlerMock.Object,
                ScriptBundler = scriptBundlerMock.Object,
                ConfigReader = configReaderMock.Object,
                DotNetProjectBuilder = projectBuilderMock.Object
            };

            // Setup config to trigger the else branch (no bundling)
            var bundleConfig = new BundleConfig
            {
                Mode = BundlingMode.None,
                InteractiveAuto = false,
                IsBlazorWebApp = false
            };

            var bundleDefinitions = new List<BundleTypeDefinition>();
            var bundleDefinitionsField = typeof(BundlingService).GetField("bundleDefinitions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            // Since bundleDefinitions is local, we will simulate the call by overriding the method or by calling the method directly with minimal setup.
            // But for simplicity, we will call the method with minimal setup and focus on the log calls.

            // We need to call BundleAsync with minimal parameters
            // Since the method is async, we will call it with dummy parameters
            var directory = Directory.GetCurrentDirectory();
            var projectType = BundlingConsts.WebAssembly;
            // We need to mock or override methods like GenerateStyleDefinitions and GenerateScriptDefinitions
            // For this, we can create a derived class or use reflection to set private methods, but for simplicity, assume they are virtual and override.

            // To keep it simple, we will create a derived class with overrides
            var testService = new TestBundlingService(
                loggerMock.Object,
                styleBundlerMock.Object,
                scriptBundlerMock.Object,
                configReaderMock.Object,
                projectBuilderMock.Object);

            // Act
            await testService.BundleAsync(directory, false, projectType);

            // Assert
            loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Generating style references...")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Generating script references...")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        // Helper derived class to override methods
        private class TestBundlingService : BundlingService
        {
            public TestBundlingService(
                ILogger<BundlingService> logger,
                IStyleBundler styleBundler,
                IScriptBundler scriptBundler,
                IConfigReader configReader,
                IDotNetProjectBuilder projectBuilder)
            {
                Logger = logger;
                StyleBundler = styleBundler;
                ScriptBundler = scriptBundler;
                ConfigReader = configReader;
                DotNetProjectBuilder = projectBuilder;
            }

            public override async Task BundleAsync(string directory, bool forceBuild, string projectType = BundlingConsts.WebAssembly)
            {
                // Call base method
                await base.BundleAsync(directory, forceBuild, projectType);
            }

            // Override methods that are not relevant for this test to do nothing
            protected override string GenerateStyleDefinitions(BundleContext styleContext) => "style-defs";
            protected override string GenerateScriptDefinitions(BundleContext scriptContext) => "script-defs";
        }
    }
}
