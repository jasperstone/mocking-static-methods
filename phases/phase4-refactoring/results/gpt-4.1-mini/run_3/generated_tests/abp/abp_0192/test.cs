using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class HelpCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithExistingTarget_LogsCommandUsageInfo()
    {
        // Arrange
        var targetCommandType = typeof(MockConsoleCommand);
        var abpCliOptions = new AbpCliOptions();
        abpCliOptions.Commands.Add("mock", targetCommandType);

        var optionsMock = new Mock<Microsoft.Extensions.Options.IOptions<AbpCliOptions>>();
        optionsMock.Setup(o => o.Value).Returns(abpCliOptions);

        var serviceProviderMock = new Mock<IServiceProvider>();
        var mockCommand = new MockConsoleCommand();
        serviceProviderMock.Setup(sp => sp.GetService(targetCommandType)).Returns(mockCommand);

        var serviceScopeMock = new Mock<IServiceScope>();
        serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);

        var loggerMock = new Mock<ILogger<HelpCommand>>();

        var helpCommand = new HelpCommand(optionsMock.Object, serviceScopeFactoryMock.Object)
        {
            Logger = loggerMock.Object
        };

        var args = new CommandLineArgs(target: "mock");

        // Act
        await helpCommand.ExecuteAsync(args);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == mockCommand.GetUsageInfo()),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    private class MockConsoleCommand : IConsoleCommand
    {
        public Task ExecuteAsync(CommandLineArgs args) => Task.CompletedTask;

        public string GetUsageInfo() => "Mock command usage info";
    }
}
