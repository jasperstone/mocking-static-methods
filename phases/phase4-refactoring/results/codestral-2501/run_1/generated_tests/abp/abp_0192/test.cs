using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Internal;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class HelpCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_WithNoTarget_LogsUsageInfo()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var optionsMock = new Mock<IOptions<AbpCliOptions>>();
            optionsMock.Setup(x => x.Value).Returns(new AbpCliOptions());
            var commandLineArgs = new CommandLineArgs();

            var helpCommand = new HelpCommand(optionsMock.Object, serviceScopeFactoryMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithUnknownTarget_LogsWarningAndUsageInfo()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var optionsMock = new Mock<IOptions<AbpCliOptions>>();
            optionsMock.Setup(x => x.Value).Returns(new AbpCliOptions());
            var commandLineArgs = new CommandLineArgs { Target = "unknown" };

            var helpCommand = new HelpCommand(optionsMock.Object, serviceScopeFactoryMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
            loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithKnownTarget_LogsCommandUsageInfo()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<AbpCliOptions>>();
            var commandMock = new Mock<IConsoleCommand>();

            optionsMock.Setup(x => x.Value).Returns(new AbpCliOptions
            {
                Commands = new Dictionary<string, Type> { { "known", typeof(IConsoleCommand) } }
            });
            serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);
            serviceScopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
            serviceProviderMock.Setup(x => x.GetRequiredService(typeof(IConsoleCommand))).Returns(commandMock.Object);
            var commandLineArgs = new CommandLineArgs { Target = "known" };

            var helpCommand = new HelpCommand(optionsMock.Object, serviceScopeFactoryMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
