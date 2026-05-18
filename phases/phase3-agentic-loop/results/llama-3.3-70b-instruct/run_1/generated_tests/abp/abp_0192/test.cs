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
            var serviceScopeFactoryMock = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
            var cliOptions = new AbpCliOptions();
            var helpCommand = new HelpCommand(new Microsoft.Extensions.Options.OptionsWrapper<AbpCliOptions>(cliOptions), serviceScopeFactoryMock.Object);
            helpCommand.Logger = loggerMock.Object;
            var commandLineArgs = new CommandLineArgs("abp", string.Empty);

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);
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
            var commandLineArgs = new CommandLineArgs("abp", "unknown");

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Exactly(2));
        }

        [Fact]
        public async Task ExecuteAsync_LogsCommandUsageInfo_WhenTargetIsKnown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var serviceScopeFactoryMock = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
            var cliOptions = new AbpCliOptions();
            cliOptions.Commands.Add("known", typeof(HelpCommand));
            var helpCommand = new HelpCommand(new Microsoft.Extensions.Options.OptionsWrapper<AbpCliOptions>(cliOptions), serviceScopeFactoryMock.Object);
            helpCommand.Logger = loggerMock.Object;
            var commandLineArgs = new CommandLineArgs("abp", "known");
            var scopeMock = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScope>();
            var serviceProviderMock = new Mock<Microsoft.Extensions.DependencyInjection.IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetRequiredService(It.IsAny<Type>())).Returns(new object());
            scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
            serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);
        }
    }
}
