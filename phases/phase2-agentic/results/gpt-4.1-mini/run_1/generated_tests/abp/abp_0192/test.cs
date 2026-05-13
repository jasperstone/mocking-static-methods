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
    private class TestCommand : IConsoleCommand
    {
        public Task ExecuteAsync(CommandLineArgs commandLineArgs) => Task.CompletedTask;
        public string GetUsageInfo() => "TestCommand Usage Info";
        public static string GetShortDescription() => "Test command short description";
    }

    private class HideCommand : IConsoleCommand
    {
        public Task ExecuteAsync(CommandLineArgs commandLineArgs) => Task.CompletedTask;
        public string GetUsageInfo() => "Hidden Command Usage Info";
        public static string GetShortDescription() => "Hidden command short description";
    }

    [Fact]
    public async Task ExecuteAsync_TargetIsNullOrWhitespace_LogsUsageInfo()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<HelpCommand>>();
        var optionsMock = new Mock<IOptions<AbpCliOptions>>();
        var abpCliOptions = new AbpCliOptions
        {
            Commands = new Dictionary<string, Type>()
        };
        optionsMock.Setup(o => o.Value).Returns(abpCliOptions);

        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

        var helpCommand = new HelpCommand(optionsMock.Object, serviceScopeFactoryMock.Object)
        {
            Logger = loggerMock.Object
        };

        var args = new CommandLineArgs { Target = " " };

        // Act
        await helpCommand.ExecuteAsync(args);

        // Assert
        loggerMock.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Usage:")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_TargetNotFound_LogsWarningAndUsageInfo()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<HelpCommand>>();
        var optionsMock = new Mock<IOptions<AbpCliOptions>>();
        var abpCliOptions = new AbpCliOptions
        {
            Commands = new Dictionary<string, Type>()
        };
        optionsMock.Setup(o => o.Value).Returns(abpCliOptions);

        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

        var helpCommand = new HelpCommand(optionsMock.Object, serviceScopeFactoryMock.Object)
        {
            Logger = loggerMock.Object
        };

        var args = new CommandLineArgs { Target = "nonexistent" };

        // Act
        await helpCommand.ExecuteAsync(args);

        // Assert
        loggerMock.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("There is no command named nonexistent.")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        loggerMock.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Usage:")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_TargetFound_LogsCommandUsageInfo()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<HelpCommand>>();
        var optionsMock = new Mock<IOptions<AbpCliOptions>>();
        var abpCliOptions = new AbpCliOptions
        {
            Commands = new Dictionary<string, Type>
            {
                { "test", typeof(TestCommand) }
            }
        };
        optionsMock.Setup(o => o.Value).Returns(abpCliOptions);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(TestCommand))).Returns(null);
        serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(TestCommand))).Returns(new TestCommand());

        var serviceScopeMock = new Mock<IServiceScope>();
        serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);

        var helpCommand = new HelpCommand(optionsMock.Object, serviceScopeFactoryMock.Object)
        {
            Logger = loggerMock.Object
        };

        var args = new CommandLineArgs { Target = "test" };

        // Act
        await helpCommand.ExecuteAsync(args);

        // Assert
        loggerMock.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("TestCommand Usage Info")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
