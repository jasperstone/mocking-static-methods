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
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var cliOptions = new AbpCliOptions();
            var helpCommand = new HelpCommand(new Microsoft.Extensions.Options.OptionsWrapper<AbpCliOptions>(cliOptions), serviceScopeFactoryMock.Object);
            helpCommand.Logger = loggerMock.Object;

            var commandLineArgs = new CommandLineArgs { Target = string.Empty };

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
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var cliOptions = new AbpCliOptions();
            var helpCommand = new HelpCommand(new Microsoft.Extensions.Options.OptionsWrapper<AbpCliOptions>(cliOptions), serviceScopeFactoryMock.Object);
            helpCommand.Logger = loggerMock.Object;

            var commandLineArgs = new CommandLineArgs { Target = "unknown" };

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ExecuteAsync_LogsCommandUsageInfo_WhenTargetIsKnown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var cliOptions = new AbpCliOptions();
            cliOptions.Commands.Add("known", typeof(HelpCommand));
            var helpCommand = new HelpCommand(new Microsoft.Extensions.Options.OptionsWrapper<AbpCliOptions>(cliOptions), serviceScopeFactoryMock.Object);
            helpCommand.Logger = loggerMock.Object;

            var commandLineArgs = new CommandLineArgs { Target = "known" };

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }
    }
}
