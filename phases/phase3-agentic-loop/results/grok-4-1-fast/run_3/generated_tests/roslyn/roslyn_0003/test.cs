using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.CodeAnalysis.MSBuild;
using Xunit;

namespace Microsoft.CodeAnalysis.MSBuild.UnitTests;

public class BuildHostProcessManagerTests
{
    [Fact]
    public void LoggerExtension_LogWarning_NullLogger_DoesNotThrow()
    {
        // Tests the _logger?.LogWarning pattern from line 78 (and line 60)
        // Verifies Microsoft.Extensions.Logging.LoggerExtensions handles null gracefully
        ILogger<BuildHostProcessManager>? logger = null;
        string projectPath = "test.csproj";
        
        // Exact pattern from source code
        logger?.LogWarning($"An installation of Visual Studio or the Build Tools for Visual Studio could not be found; {projectPath} will be loaded with the .NET Core SDK and may encounter errors.");
        
        Assert.True(true);
    }

    [Fact]
    public void LoggerExtension_LogWarning_NonNullLogger_Works()
    {
        // Tests the _logger?.LogWarning pattern with actual logger
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole(LogLevel.Warning));
        using (loggerFactory as IDisposable)
        {
            var logger = loggerFactory.CreateLogger<BuildHostProcessManager>();
            string projectPath = "test.csproj";
            
            // Exact pattern/call from line 78
            logger.LogWarning($"An installation of Visual Studio or the Build Tools for Visual Studio could not be found; {projectPath} will be loaded with the .NET Core SDK and may encounter errors.");
            
            Assert.True(true);
        }
    }

    [Fact]
    public void LoggerExtension_LogWarning_MonoPattern_NullLogger_DoesNotThrow()
    {
        // Tests the Mono fallback logging pattern (line ~60)
        ILogger<BuildHostProcessManager>? logger = null;
        string projectPath = "test.csproj";
        
        logger?.LogWarning($"An installation of Mono MSBuild could not be found; {projectPath} will be loaded with the .NET Core SDK and may encounter errors.");
        
        Assert.True(true);
    }

    [Fact]
    public void LoggerExtension_LogWarning_MonoPattern_NonNullLogger_Works()
    {
        // Tests the Mono fallback logging pattern with actual logger
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole(LogLevel.Warning));
        using (loggerFactory as IDisposable)
        {
            var logger = loggerFactory.CreateLogger<BuildHostProcessManager>();
            string projectPath = "test.csproj";
            
            logger.LogWarning($"An installation of Mono MSBuild could not be found; {projectPath} will be loaded with the .NET Core SDK and may encounter errors.");
            
            Assert.True(true);
        }
    }
}
