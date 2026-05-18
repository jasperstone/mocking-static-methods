using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class HelpCommandTests
{
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IConsoleCommand> _consoleCommandMock;
    private readonly Mock<ILogger<HelpCommand>> _loggerMock;
    private readonly HelpCommand _helpCommand;

    public HelpCommandTests()
    {
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _consoleCommandMock = new Mock<IConsoleCommand>();
        _loggerMock = new Mock<ILogger<HelpCommand>>();

        _serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(_serviceScopeMock.Object);
        _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
        _serviceProviderMock.Setup(p => p.GetService(It.IsAny<Type>())).Returns(_consoleCommandMock.Object);

        var abpCliOptions = new AbpCliOptions();
        abpCliOptions.Commands["test"] = typeof(MockIConsoleCommand);

        var optionsMock = new Mock<IOptions<AbpCliOptions>>();
        optionsMock.Setup(o => o.Value).Returns(abpCliOptions);

        _helpCommand = new HelpCommand(optionsMock.Object, _serviceScopeFactoryMock.Object);
        _helpCommand.Logger = _loggerMock.Object;
    }

    [Fact]
    public async Task Should_LogInformation_When_Target_Is_Empty()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs(target: null);

        // Act
        await _helpCommand.ExecuteAsync(commandLineArgs);

        // Assert
        _loggerMock.Verify(x => x.LogInformation(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Should_LogWarning_And_LogInformation_For_Unknown_Command()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs(target: "unknown");

        // Act
        await _helpCommand.ExecuteAsync(commandLineArgs);

        // Assert
        _loggerMock.Verify(x => x.LogWarning(It.Is<string>(s => s.Contains("unknown"))), Times.Once);
        _loggerMock.Verify(x => x.LogInformation(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Should_LogInformation_With_Command_GetUsageInfo()
    {
        // Arrange - Tests coverage of line 53: Logger.LogInformation(command.GetUsageInfo());
        _consoleCommandMock.Setup(c => c.GetUsageInfo()).Returns("Command usage info");
        var commandLineArgs = new CommandLineArgs(target: "test");

        // Act
        await _helpCommand.ExecuteAsync(commandLineArgs);

        // Assert
        _loggerMock.Verify(x => x.LogInformation("Command usage info"), Times.Once);
    }
}

public class MockIConsoleCommand : IConsoleCommand
{
    public string GetUsageInfo() => "Command usage info";
    public Task ExecuteAsync(CommandLineArgs args) => Task.CompletedTask;
}
