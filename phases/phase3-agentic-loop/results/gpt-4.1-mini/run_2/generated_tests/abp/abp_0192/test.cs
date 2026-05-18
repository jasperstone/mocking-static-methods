using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class HelpCommandTests
{
    [Fact]
    public async Task ExecuteAsync_LogsUsageInfo_WhenTargetIsNullOrWhitespace()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<HelpCommand>>();
        var options = Options.Create(new AbpCliOptions());
        var mockScopeFactory = new Mock<IServiceScopeFactory>();

        var helpCommand = new HelpCommand(options, mockScopeFactory.Object)
        {
            Logger = mockLogger.Object
        };

        var args = new CommandLineArgs(target: "   ");

        // Act
        await helpCommand.ExecuteAsync(args);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Usage:")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_LogsWarningAndUsageInfo_WhenTargetNotFound()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<HelpCommand>>();
        var abpOptions = new AbpCliOptions
        {
            Commands = new Dictionary<string, Type>()
        };
        var options = Options.Create(abpOptions);
        var mockScopeFactory = new Mock<IServiceScopeFactory>();

        var helpCommand = new HelpCommand(options, mockScopeFactory.Object)
        {
            Logger = mockLogger.Object
        };

        var args = new CommandLineArgs(target: "nonexistent");

        // Act
        await helpCommand.ExecuteAsync(args);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("There is no command named nonexistent.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Usage:")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_LogsCommandUsageInfo_WhenTargetFound()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<HelpCommand>>();
        var commandType = typeof(MockCommand);
        var commands = new Dictionary<string, Type>
        {
            { "mock", commandType }
        };
        var abpOptions = new AbpCliOptions
        {
            Commands = commands
        };
        var options = Options.Create(abpOptions);

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(x => x.GetRequiredService(commandType)).Returns(new MockCommand());

        var mockScope = new Mock<IServiceScope>();
        mockScope.SetupGet(x => x.ServiceProvider).Returns(mockServiceProvider.Object);

        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        var helpCommand = new HelpCommand(options, mockScopeFactory.Object)
        {
            Logger = mockLogger.Object
        };

        var args = new CommandLineArgs(target: "mock");

        // Act
        await helpCommand.ExecuteAsync(args);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("MockCommand UsageInfo")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    private class MockCommand : IConsoleCommand
    {
        public Task ExecuteAsync(CommandLineArgs commandLineArgs)
        {
            throw new NotImplementedException();
        }

        public string GetUsageInfo()
        {
            return "MockCommand UsageInfo";
        }
    }
}
