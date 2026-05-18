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
    private readonly Mock<IConsoleCommand> _consoleCommandMock;

    public HelpCommandTests()
    {
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _consoleCommandMock = new Mock<IConsoleCommand>();

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
        _serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

        _serviceProviderMock.Setup(p => p.GetRequiredService(It.IsAny<Type>())).Returns(_consoleCommandMock.Object);
    }

    [Fact]
    public async Task Should_LogInformation_When_Target_Is_Empty()
    {
        // Arrange
        var optionsMock = new Mock<IOptions<AbpCliOptions>>();
        optionsMock.Setup(o => o.Value).Returns(new AbpCliOptions());
        var command = new HelpCommand(optionsMock.Object, _serviceScopeFactoryMock.Object);
        var mockLogger = new Mock<ILogger<HelpCommand>>();
        command.Logger = mockLogger.Object;

        var args = new CommandLineArgs(target: null);

        // Act
        await command.ExecuteAsync(args);

        // Assert
        mockLogger.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Should_LogWarning_And_LogInformation_When_Target_Not_Found()
    {
        // Arrange
        var optionsMock = new Mock<IOptions<AbpCliOptions>>();
        optionsMock.Setup(o => o.Value).Returns(new AbpCliOptions());
        var command = new HelpCommand(optionsMock.Object, _serviceScopeFactoryMock.Object);
        var mockLogger = new Mock<ILogger<HelpCommand>>();
        command.Logger = mockLogger.Object;

        var args = new CommandLineArgs(target: "nonexistent");

        // Act
        await command.ExecuteAsync(args);

        // Assert
        mockLogger.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Once);
        mockLogger.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Should_LogInformation_With_Command_UsageInfo_When_Valid_Target()
    {
        // Arrange
        _consoleCommandMock.Setup(c => c.GetUsageInfo()).Returns("Command usage info");

        var abpOptions = new AbpCliOptions();
        var commandsField = typeof(AbpCliOptions).GetField("Commands", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        commandsField?.SetValue(abpOptions, new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            ["test"] = typeof(MockCommand)
        });

        var optionsMock = new Mock<IOptions<AbpCliOptions>>();
        optionsMock.Setup(o => o.Value).Returns(abpOptions);
        var command = new HelpCommand(optionsMock.Object, _serviceScopeFactoryMock.Object);
        var mockLogger = new Mock<ILogger<HelpCommand>>();
        command.Logger = mockLogger.Object;

        _serviceProviderMock.Setup(p => p.GetRequiredService(typeof(MockCommand))).Returns(_consoleCommandMock.Object);

        var args = new CommandLineArgs(target: "test");

        // Act
        await command.ExecuteAsync(args);

        // Assert
        mockLogger.Verify(l => l.LogInformation("Command usage info"), Times.Once);
    }
}

public class MockCommand : IConsoleCommand
{
    public string GetUsageInfo() => "Command usage info";
    public Task ExecuteAsync(CommandLineArgs args) => Task.CompletedTask;
}
