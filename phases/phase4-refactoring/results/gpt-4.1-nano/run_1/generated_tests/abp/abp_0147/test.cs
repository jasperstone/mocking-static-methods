using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System;

namespace BundlingServiceTests
{
    public class BundleAsyncTests
    {
        [Fact]
        public async Task BundleAsync_Should_Log_GenerateScriptReferences_When_Mode_Is_Not_Bundle()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BundlingService>>();
            var bundlingService = new TestBundlingService(loggerMock.Object);

            // Setup minimal dependencies and state
            var bundleConfig = new BundleConfig
            {
                Mode = BundlingMode.None,
                InteractiveAuto = true,
                IsBlazorWebApp = false,
                Name = "TestBundle"
            };

            // Override methods to avoid dependencies
            var bundleDefinitions = new List<BundleTypeDefinition>();
            var styleContext = new BundleContext();
            var scriptContext = new BundleContext();

            // Act
            await bundlingService.BundleAsync("dummyDir", false);

            // Assert
            loggerMock.Verify(x => x.LogInformation("Generating style references..."), Times.Once);
            loggerMock.Verify(x => x.LogInformation("Generating script references..."), Times.Once);
        }

        private class TestBundlingService : BundlingService
        {
            private readonly ILogger<BundlingService> _logger;
            public TestBundlingService(ILogger<BundlingService> logger)
            {
                _logger = logger;
                this.Logger = logger;
            }

            public override async Task BundleAsync(string directory, bool forceBuild, string projectType = BundlingConsts.WebAssembly)
            {
                // Call base method
                await base.BundleAsync(directory, forceBuild, projectType);
            }

            protected override string GenerateStyleDefinitions(BundleContext styleContext) => "styleDefs";
            protected override string GenerateScriptDefinitions(BundleContext scriptContext) => "scriptDefs";

            protected override BundleContext GetStyleContext(List<BundleTypeDefinition> bundleDefinitions, BundleConfig bundleConfig)
            {
                return new BundleContext();
            }

            protected override BundleContext GetScriptContext(List<BundleTypeDefinition> bundleDefinitions, BundleConfig bundleConfig, string projectType)
            {
                return new BundleContext();
            }

            protected override async Task UpdateDependenciesInBlazorFileAsync(string fileName, string styleDefinitions, string scriptDefinitions)
            {
                await Task.CompletedTask;
            }
        }
    }
}
