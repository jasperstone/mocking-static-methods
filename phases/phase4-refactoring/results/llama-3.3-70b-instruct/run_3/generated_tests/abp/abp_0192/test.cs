using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Core;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class HelpCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsUsageInfo_WhenTargetIsEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var cliOptions = new AbpCliOptions();
            var serviceScopeFactoryMock = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
            var helpCommand = new HelpCommand(new Microsoft.Extensions.Options.OptionsWrapper<AbpCliOptions>(cliOptions), serviceScopeFactoryMock.Object);
            helpCommand.Logger = loggerMock.Object;

            var commandLineArgs = new CommandLineArgs(string.Empty, string.Empty);

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsUsageInfo_WhenTargetIsUnknown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var cliOptions = new AbpCliOptions();
            var serviceScopeFactoryMock = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
            var helpCommand = new HelpCommand(new Microsoft.Extensions.Options.OptionsWrapper<AbpCliOptions>(cliOptions), serviceScopeFactoryMock.Object);
            helpCommand.Logger = loggerMock.Object;

            var commandLineArgs = new CommandLineArgs("unknown", string.Empty);

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ExecuteAsync_LogsCommandUsageInfo_WhenTargetIsKnown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var cliOptions = new AbpCliOptions();
            cliOptions.Commands.Add("known", typeof(HelpCommand));
            var serviceScopeFactoryMock = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
            var helpCommand = new HelpCommand(new Microsoft.Extensions.Options.OptionsWrapper<AbpCliOptions>(cliOptions), serviceScopeFactoryMock.Object);
            helpCommand.Logger = loggerMock.Object;

            var commandLineArgs = new CommandLineArgs("known", string.Empty);

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);
        }
    }
}
