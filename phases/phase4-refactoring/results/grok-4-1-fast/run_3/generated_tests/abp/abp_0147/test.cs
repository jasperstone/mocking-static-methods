using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Bundling;
using Volo.Abp.Cli.Bundling;
using Xunit;

namespace Volo.Abp.Cli.Bundling.Tests;

public class BundlingServiceTests
{
    [Fact]
    public async Task BundleAsync_ReferencesMode_ShouldLogGeneratingScriptReferences()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<BundlingService>>();
        var service = new BundlingService
        {
            Logger = mockLogger.Object,
            ConfigReader = Mock.Of<IConfigReader>(r => r.Read(It.IsAny<string>()) == 
                new BundleConfig { Mode = BundlingMode.References, InteractiveAuto = true }),
            ScriptBundler = Mock.Of<IScriptBundler>(),
            StyleBundler = Mock.Of<IStyleBundler>()
        };

        // Create minimal test environment to pass initial checks
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);
        File.WriteAllText(Path.Combine(tempDir, "Test.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup /></Project>");

        try
        {
            // Act
            await service.BundleAsync(tempDir, false, "WebAssembly");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }

        // Assert - Verifies line 112 Logger.LogInformation("Generating script references...");
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Generating script references...")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task BundleAsync_BundleMode_ShouldNotLogGeneratingScriptReferences()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<BundlingService>>();
        var service = new BundlingService
        {
            Logger = mockLogger.Object,
            ConfigReader = Mock.Of<IConfigReader>(r => r.Read(It.IsAny<string>()) == 
                new BundleConfig { Mode = BundlingMode.Bundle, InteractiveAuto = true }),
            ScriptBundler = Mock.Of<IScriptBundler>(b => b.Bundle(It.IsAny<BundleOptions>(), It.IsAny<BundleContext>()) == ""),
            StyleBundler = Mock.Of<IStyleBundler>(b => b.Bundle(It.IsAny<BundleOptions>(), It.IsAny<BundleContext>()) == "")
        };

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);
        File.WriteAllText(Path.Combine(tempDir, "Test.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup /></Project>");

        try
        {
            // Act
            await service.BundleAsync(tempDir, false, "WebAssembly");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }

        // Assert - Should NOT log "Generating script references..." (line 112)
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Generating script references...")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
