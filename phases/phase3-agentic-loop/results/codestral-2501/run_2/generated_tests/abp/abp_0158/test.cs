using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Versioning;
using Volo.Abp.Cli;
using Xunit;

public class CliServiceTests
{
    [Fact]
    public void LogNewVersionInfo_StableChannel_LogsCorrectMessage()
    {
        // Arrange
        var logger = new TestLogger<CliService>();
        var cliService = new CliService(
            null, null, null, null, null, null, null, null)
        {
            Logger = logger
        };

        var updateChannel = GetUpdateChannel(cliService, "Stable");
        var latestVersion = new SemanticVersion(2, 0, 0);
        var toolPath = @"C:\path\to\tool";

        // Act
        InvokeLogNewVersionInfo(cliService, updateChannel, latestVersion, toolPath);

        // Assert
        Assert.Contains("A newer stable version of the ABP CLI is available: 2.0.0.", logger.LoggedMessages);
        Assert.Contains("dotnet tool update --tool-path C:\\path\\to\\tool Volo.Abp.Cli", logger.LoggedMessages);
    }

    [Fact]
    public void LogNewVersionInfo_PrereleaseChannel_LogsCorrectMessage()
    {
        // Arrange
        var logger = new TestLogger<CliService>();
        var cliService = new CliService(
            null, null, null, null, null, null, null, null)
        {
            Logger = logger
        };

        var updateChannel = GetUpdateChannel(cliService, "Prerelease");
        var latestVersion = new SemanticVersion(2, 0, 0, "beta");
        var toolPath = @"C:\path\to\tool";

        // Act
        InvokeLogNewVersionInfo(cliService, updateChannel, latestVersion, toolPath);

        // Assert
        Assert.Contains("A newer prerelease version of the ABP CLI is available: 2.0.0-beta.", logger.LoggedMessages);
        Assert.Contains("dotnet tool update --tool-path C:\\path\\to\\tool Volo.Abp.Cli --version 2.0.0-beta", logger.LoggedMessages);
    }

    [Fact]
    public void LogNewVersionInfo_NightlyChannel_LogsCorrectMessage()
    {
        // Arrange
        var logger = new TestLogger<CliService>();
        var cliService = new CliService(
            null, null, null, null, null, null, null, null)
        {
            Logger = logger
        };

        var updateChannel = GetUpdateChannel(cliService, "Nightly");
        var latestVersion = new SemanticVersion(2, 0, 0, "nightly");
        var toolPath = @"C:\path\to\tool";

        // Act
        InvokeLogNewVersionInfo(cliService, updateChannel, latestVersion, toolPath);

        // Assert
        Assert.Contains("A newer nightly version of the ABP CLI is available: 2.0.0-nightly.", logger.LoggedMessages);
        Assert.Contains("dotnet tool uninstall --tool-path C:\\path\\to\\tool Volo.Abp.Cli", logger.LoggedMessages);
        Assert.Contains("dotnet tool install --tool-path C:\\path\\to\\tool Volo.Abp.Cli --add-source https://www.myget.org/F/abp-nightly/api/v3/index.json --version 2.0.0-nightly", logger.LoggedMessages);
    }

    [Fact]
    public void LogNewVersionInfo_DevelopmentChannel_LogsCorrectMessage()
    {
        // Arrange
        var logger = new TestLogger<CliService>();
        var cliService = new CliService(
            null, null, null, null, null, null, null, null)
        {
            Logger = logger
        };

        var updateChannel = GetUpdateChannel(cliService, "Development");
        var latestVersion = new SemanticVersion(2, 0, 0, "dev");
        var toolPath = @"C:\path\to\tool";

        // Act
        InvokeLogNewVersionInfo(cliService, updateChannel, latestVersion, toolPath);

        // Assert
        Assert.Contains("A newer development version of the ABP CLI is available: 2.0.0-dev.", logger.LoggedMessages);
        Assert.Contains("dotnet tool uninstall --tool-path C:\\path\\to\\tool Volo.Abp.Cli", logger.LoggedMessages);
        Assert.Contains("dotnet tool install --tool-path C:\\path\\to\\tool Volo.Abp.Cli --add-source https://www.myget.org/F/abp-nightly/api/v3/index.json --version 2.0.0-dev", logger.LoggedMessages);
    }

    private object GetUpdateChannel(CliService cliService, string channelName)
    {
        var updateChannelType = cliService.GetType().GetNestedTypes(BindingFlags.NonPublic).First(t => t.Name == "UpdateChannel");
        return Enum.Parse(updateChannelType, channelName);
    }

    private void InvokeLogNewVersionInfo(CliService cliService, object updateChannel, SemanticVersion latestVersion, string toolPath)
    {
        var method = cliService.GetType().GetMethod("LogNewVersionInfo", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(cliService, new object[] { updateChannel, latestVersion, toolPath, null });
    }

    public class TestLogger<T> : ILogger<T>
    {
        public List<string> LoggedMessages { get; } = new List<string>();

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel == LogLevel.Warning)
            {
                LoggedMessages.Add(formatter(state, exception));
            }
        }
    }
}
