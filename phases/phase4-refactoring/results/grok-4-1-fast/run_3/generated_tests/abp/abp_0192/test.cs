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

namespace Volo.Abp.Cli.Commands.Tests;

public class HelpCommandTests
{
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IOptions<AbpCliOptions>> _cliOptionsMock;
    private readonly Mock<ILogger<HelpCommand>> _loggerMock;
    private readonly HelpCommand _helpCommand;

    public HelpCommandTests()
    {
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _cliOptionsMock = new Mock<IOptions<AbpCliOptions>>();
        _loggerMock = new Mock<ILogger<HelpCommand>>();

        var options = new AbpCliOptions();
        _cliOptionsMock.Setup(o => o.Value).Returns(options);

        _helpCommand = new HelpCommand(_cliOptionsMock.Object, _serviceScopeFactoryMock.Object)
        {
            Logger = _loggerMock.Object
        };
    }

    [Fact]
    public async Task Should_LogInformation_When_Target_Is_NullOrWhiteSpace()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs(target: null);

        // Act
        await _helpCommand.ExecuteAsync(commandLineArgs);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Usage:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_LogWarning_And_UsageInfo_When_Command_Not_Found()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs(target: "NonExistentCommand");
        var options = new AbpCliOptions();
        _cliOptionsMock.Setup(o => o.Value).Returns(options);

        // Act
        await _helpCommand.ExecuteAsync(commandLineArgs);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("There is no command named NonExistentCommand")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Usage:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_LogInformation_With_Command_UsageInfo_When_Command_Exists()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs(target: "test");
        var options = new AbpCliOptions();
        options.Commands["test"] = typeof(MockCommand);
        _cliOptionsMock.Setup(o => o.Value).Returns(options);

        var mockCommand = new MockCommand();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(p => p.GetService(typeof(MockCommand)))
                          .Returns(mockCommand);
        serviceProviderMock.Setup(p => p.GetRequiredService(typeof(MockCommand)))
                          .Returns(mockCommand);

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
        _serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

        // Act
        await _helpCommand.ExecuteAsync(commandLineArgs);

        // Assert - Verifies the LogInformation call on line 53
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Test usage info")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private class MockCommand : IConsoleCommand
    {
        public string GetUsageInfo()
        {
            return "Test usage info";
        }

        public Task ExecuteAsync(CommandLineArgs commandLineArgs)
        {
            return Task.CompletedTask;
        }
    }
}
