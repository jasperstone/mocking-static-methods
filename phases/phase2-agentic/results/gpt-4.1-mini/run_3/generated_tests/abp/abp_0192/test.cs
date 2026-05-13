using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Cli;
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
        var optionsMock = new Mock<IOptions<AbpCliOptions>>();
        var abpCliOptions = new AbpCliOptions();
        optionsMock.Setup(o => o.Value).Returns(abpCliOptions);

        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

        var loggerMock = new Mock<ILogger<HelpCommand>>();

        var helpCommand = new HelpCommand(optionsMock.Object, serviceScopeFactoryMock.Object)
        {
            Logger = loggerMock.Object
        };

        var args = new CommandLineArgs { Target = " " };

        // Act
        await helpCommand.ExecuteAsync(args);

        // Assert
        loggerMock.Verify(
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
        var optionsMock = new Mock<IOptions<AbpCliOptions>>();
        var abpCliOptions = new AbpCliOptions();
        optionsMock.Setup(o => o.Value).Returns(abpCliOptions);

        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

        var loggerMock = new Mock<ILogger<HelpCommand>>();

        var helpCommand = new HelpCommand(optionsMock.Object, serviceScopeFactoryMock.Object)
        {
            Logger = loggerMock.Object
        };

        var args = new CommandLineArgs { Target = "nonexistent" };

        // Act
        await helpCommand.ExecuteAsync(args);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("There is no command named nonexistent.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        loggerMock.Verify(
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
        var commandType = typeof(MockCommand);

        var abpCliOptions = new AbpCliOptions();
        abpCliOptions.Commands.Add("mock", commandType);

        var optionsMock = new Mock<IOptions<AbpCliOptions>>();
        optionsMock.Setup(o => o.Value).Returns(abpCliOptions);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(commandType)).Returns(null);
        serviceProviderMock.Setup(sp => sp.GetRequiredService(commandType)).Returns(new MockCommand());

        var serviceScopeMock = new Mock<IServiceScope>();
        serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);

        var loggerMock = new Mock<ILogger<HelpCommand>>();

        var helpCommand = new HelpCommand(optionsMock.Object, serviceScopeFactoryMock.Object)
        {
            Logger = loggerMock.Object
        };

        var args = new CommandLineArgs { Target = "mock" };

        // Act
        await helpCommand.ExecuteAsync(args);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Mock command usage info")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    private class MockCommand : IConsoleCommand
    {
        public Task ExecuteAsync(CommandLineArgs args) => Task.CompletedTask;

        public string GetUsageInfo() => "Mock command usage info";
    }
}
