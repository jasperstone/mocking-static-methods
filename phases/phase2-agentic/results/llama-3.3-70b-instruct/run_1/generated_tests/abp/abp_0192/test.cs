using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
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
            var cliOptionsMock = new Mock<IOptions<AbpCliOptions>>();
            var abpCliOptions = new AbpCliOptions();
            cliOptionsMock.SetupGet(x => x.Value).Returns(abpCliOptions);
            var helpCommand = new HelpCommand(cliOptionsMock.Object, serviceScopeFactoryMock.Object);
            helpCommand.Logger = loggerMock.Object;
            var commandLineArgs = new CommandLineArgs();

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(x => x.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsUsageInfo_WhenTargetIsUnknown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var cliOptionsMock = new Mock<IOptions<AbpCliOptions>>();
            var abpCliOptions = new AbpCliOptions();
            cliOptionsMock.SetupGet(x => x.Value).Returns(abpCliOptions);
            var helpCommand = new HelpCommand(cliOptionsMock.Object, serviceScopeFactoryMock.Object);
            helpCommand.Logger = loggerMock.Object;
            var commandLineArgs = new CommandLineArgs { Target = "unknown" };

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(x => x.LogInformation(It.IsAny<string>()), Times.Once);
            loggerMock.Verify(x => x.LogWarning(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsCommandUsageInfo_WhenTargetIsKnown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var cliOptionsMock = new Mock<IOptions<AbpCliOptions>>();
            var abpCliOptions = new AbpCliOptions();
            abpCliOptions.Commands.Add("known", typeof(HelpCommand));
            cliOptionsMock.SetupGet(x => x.Value).Returns(abpCliOptions);
            var helpCommand = new HelpCommand(cliOptionsMock.Object, serviceScopeFactoryMock.Object);
            helpCommand.Logger = loggerMock.Object;
            var commandLineArgs = new CommandLineArgs { Target = "known" };

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(x => x.LogInformation(It.IsAny<string>()), Times.Once);
        }
    }
}
