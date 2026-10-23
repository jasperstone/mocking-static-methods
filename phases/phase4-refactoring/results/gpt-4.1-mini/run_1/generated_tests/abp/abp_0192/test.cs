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
    public async Task ExecuteAsync_TargetIsNullOrWhitespace_LogsUsageInfo()
    {
        // Arrange
        var abpCliOptions = new AbpCliOptions();
        var optionsMock = new Mock<IOptions<AbpCliOptions>>();
        optionsMock.Setup(o => o.Value).Returns(abpCliOptions);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();

        var loggerMock = new Mock<ILogger<HelpCommand>>();

        var helpCommand = new HelpCommand(optionsMock.Object, scopeFactoryMock.Object)
        {
            Logger = loggerMock.Object
        };

        var args = new CommandLineArgs(null, " ");

        // Act
        await helpCommand.ExecuteAsync(args);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == helpCommand.GetUsageInfo()),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_TargetNotFound_LogsWarningAndUsageInfo()
    {
        // Arrange
        var abpCliOptions = new AbpCliOptions();
        abpCliOptions.Commands.Add("existing", typeof(object));
        var optionsMock = new Mock<IOptions<AbpCliOptions>>();
        optionsMock.Setup(o => o.Value).Returns(abpCliOptions);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();

        var loggerMock = new Mock<ILogger<HelpCommand>>();

        var helpCommand = new HelpCommand(optionsMock.Object, scopeFactoryMock.Object)
        {
            Logger = loggerMock.Object
        };

        var args = new CommandLineArgs(null, "notfound");

        // Act
        await helpCommand.ExecuteAsync(args);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "There is no command named notfound."),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == helpCommand.GetUsageInfo()),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_TargetFound_LogsCommandUsageInfo()
    {
        // Arrange
        var commandUsageInfo = "Command usage info";

        var commandMock = new Mock<IConsoleCommand>();
        commandMock.Setup(c => c.GetUsageInfo()).Returns(commandUsageInfo);

        var abpCliOptions = new AbpCliOptions();
        abpCliOptions.Commands.Add("mycmd", typeof(IConsoleCommand));
        var optionsMock = new Mock<IOptions<AbpCliOptions>>();
        optionsMock.Setup(o => o.Value).Returns(abpCliOptions);

        var services = new ServiceCollection();
        services.AddSingleton(commandMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.SetupGet(s => s.ServiceProvider).Returns(serviceProvider);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(sf => sf.CreateScope()).Returns(scopeMock.Object);

        var loggerMock = new Mock<ILogger<HelpCommand>>();

        var helpCommand = new HelpCommand(optionsMock.Object, scopeFactoryMock.Object)
        {
            Logger = loggerMock.Object
        };

        var args = new CommandLineArgs(null, "mycmd");

        // Act
        await helpCommand.ExecuteAsync(args);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == commandUsageInfo),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
