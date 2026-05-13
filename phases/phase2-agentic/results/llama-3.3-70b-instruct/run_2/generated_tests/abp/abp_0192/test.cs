using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Core;
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
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var abpCliOptions = new AbpCliOptions();
            var helpCommand = new HelpCommand(new OptionsWrapper<AbpCliOptions>(abpCliOptions), serviceScopeFactoryMock.Object);
            helpCommand.Logger = loggerMock.Object;
            var commandLineArgs = new CommandLineArgs { Target = string.Empty };

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsUsageInfo_WhenTargetIsUnknown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var abpCliOptions = new AbpCliOptions();
            var helpCommand = new HelpCommand(new OptionsWrapper<AbpCliOptions>(abpCliOptions), serviceScopeFactoryMock.Object);
            helpCommand.Logger = loggerMock.Object;
            var commandLineArgs = new CommandLineArgs { Target = "unknown" };

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation(It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ExecuteAsync_LogsCommandUsageInfo_WhenTargetIsKnown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var abpCliOptions = new AbpCliOptions();
            abpCliOptions.Commands.Add("known", typeof(HelpCommand));
            var helpCommand = new HelpCommand(new OptionsWrapper<AbpCliOptions>(abpCliOptions), serviceScopeFactoryMock.Object);
            helpCommand.Logger = loggerMock.Object;
            var commandLineArgs = new CommandLineArgs { Target = "known" };
            var scopeMock = new Mock<IServiceScope>();
            serviceScopeFactoryMock.Setup(sf => sf.CreateScope()).Returns(scopeMock.Object);
            var serviceProviderMock = new Mock<IServiceProvider>();
            scopeMock.SetupGet(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
            var commandMock = new Mock<IConsoleCommand>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(HelpCommand))).Returns(commandMock.Object);

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation(It.IsAny<string>()), Times.Once);
        }
    }
}
