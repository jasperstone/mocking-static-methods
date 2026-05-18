using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class HelpCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsUsageInfo_WhenTargetIsEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var serviceScopeFactoryMock = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
            var cliOptions = new AbpCliOptions();
            var helpCommand = new HelpCommand(new Microsoft.Extensions.Options.OptionsWrapper<AbpCliOptions>(cliOptions), serviceScopeFactoryMock.Object);
            helpCommand.Logger = loggerMock.Object;
            var commandLineArgs = new CommandLineArgs();

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsUsageInfo_WhenTargetIsUnknown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var serviceScopeFactoryMock = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
            var cliOptions = new AbpCliOptions();
            var helpCommand = new HelpCommand(new Microsoft.Extensions.Options.OptionsWrapper<AbpCliOptions>(cliOptions), serviceScopeFactoryMock.Object);
            helpCommand.Logger = loggerMock.Object;
            var commandLineArgs = new CommandLineArgs("Unknown", null);

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsCommandUsageInfo_WhenTargetIsKnown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var serviceScopeFactoryMock = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
            var serviceProviderMock = new Mock<Microsoft.Extensions.DependencyInjection.IServiceProvider>();
            var scopeMock = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScope>();
            scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
            serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);
            var cliOptions = new AbpCliOptions();
            cliOptions.Commands.Add("Test", typeof(TestCommand));
            var helpCommand = new HelpCommand(new Microsoft.Extensions.Options.OptionsWrapper<AbpCliOptions>(cliOptions), serviceScopeFactoryMock.Object);
            helpCommand.Logger = loggerMock.Object;
            var commandLineArgs = new CommandLineArgs(null, "Test");
            serviceProviderMock.Setup(p => p.GetRequiredService(typeof(TestCommand))).Returns(new TestCommand());

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }

        private class TestCommand : IConsoleCommand
        {
            public string GetUsageInfo()
            {
                return "Test command usage info";
            }

            public Task ExecuteAsync(CommandLineArgs commandLineArgs)
            {
                throw new NotImplementedException();
            }
        }
    }
}
