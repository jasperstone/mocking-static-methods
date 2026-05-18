using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Cli;
using Volo.Abp.Cli.Args;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class HelpCommandTests
{
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IConsoleCommand> _consoleCommandMock;

    public HelpCommandTests()
    {
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _consoleCommandMock = new Mock<IConsoleCommand>();

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
        _serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

        // Mock the actual GetService method instead of the extension
        _serviceProviderMock.Setup(p => p.GetService(It.IsAny<Type>())).Returns(_consoleCommandMock.Object);
    }

    [Fact]
    public async Task Should_LogInformation_When_Target_Is_Empty()
    {
        // Arrange
        var abpCliOptions = new AbpCliOptions();
        var optionsMock = new Mock<IOptions<AbpCliOptions>>();
        optionsMock.Setup(o => o.Value).Returns(abpCliOptions);

        var loggerMock = new Mock<ILogger<HelpCommand>>();
        var helpCommand = new HelpCommand(optionsMock.Object, _serviceScopeFactoryMock.Object);
        helpCommand.Logger = loggerMock.Object;

        var commandLineArgs = new CommandLineArgs(null, null);

        // Act
        await helpCommand.ExecuteAsync(commandLineArgs);

        // Assert
        loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Should_LogWarning_And_LogInformation_For_Unknown_Command()
    {
        // Arrange
        var abpCliOptions = new AbpCliOptions();
        var optionsMock = new Mock<IOptions<AbpCliOptions>>();
        optionsMock.Setup(o => o.Value).Returns(abpCliOptions);

        var loggerMock = new Mock<ILogger<HelpCommand>>();
        var helpCommand = new HelpCommand(optionsMock.Object, _serviceScopeFactoryMock.Object);
        helpCommand.Logger = loggerMock.Object;

        var commandLineArgs = new CommandLineArgs(null, "nonexistent");

        // Act
        await helpCommand.ExecuteAsync(commandLineArgs);

        // Assert
        loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("nonexistent"))), Times.Once);
        loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Should_LogInformation_With_Command_GetUsageInfo_When_Valid_Command()
    {
        // Arrange
        var abpCliOptions = new AbpCliOptions();
        abpCliOptions.Commands.Add("validcommand", typeof(object));
        
        var optionsMock = new Mock<IOptions<AbpCliOptions>>();
        optionsMock.Setup(o => o.Value).Returns(abpCliOptions);

        var loggerMock = new Mock<ILogger<HelpCommand>>();
        var helpCommand = new HelpCommand(optionsMock.Object, _serviceScopeFactoryMock.Object);
        helpCommand.Logger = loggerMock.Object;

        _consoleCommandMock.Setup(c => c.GetUsageInfo()).Returns("Command usage info");

        var commandLineArgs = new CommandLineArgs(null, "validcommand");

        // Act
        await helpCommand.ExecuteAsync(commandLineArgs);

        // Assert - Verifies the LogInformation call on line 53 hits with command.GetUsageInfo()
        loggerMock.Verify(l => l.LogInformation("Command usage info"), Times.Once);
    }
}
