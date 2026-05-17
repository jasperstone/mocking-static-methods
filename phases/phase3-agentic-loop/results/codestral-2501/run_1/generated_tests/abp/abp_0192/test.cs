using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Options;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class HelpCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_ShouldLogUsageInfo_WhenTargetIsNullOrWhiteSpace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var cliOptionsMock = new Mock<IOptions<AbpCliOptions>>();
            var commandLineArgs = new CommandLineArgs();

            var helpCommand = new HelpCommandMock(cliOptionsMock.Object, serviceScopeFactoryMock.Object, loggerMock.Object);

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldLogWarningAndUsageInfo_WhenCommandNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var cliOptionsMock = new Mock<IOptions<AbpCliOptions>>();
            var commandLineArgs = new CommandLineArgs(target: "UnknownCommand");

            var helpCommand = new HelpCommandMock(cliOptionsMock.Object, serviceScopeFactoryMock.Object, loggerMock.Object);

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>()),
                Times.Once);

            loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldLogCommandUsageInfo_WhenCommandFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var cliOptionsMock = new Mock<IOptions<AbpCliOptions>>();
            var commandLineArgs = new CommandLineArgs(target: "KnownCommand");

            var commandMock = new Mock<IConsoleCommand>();
            commandMock.Setup(x => x.GetUsageInfo()).Returns("Usage Info");

            serviceProviderMock.Setup(x => x.GetRequiredService(typeof(IConsoleCommand))).Returns(commandMock.Object);
            serviceScopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
            serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);

            cliOptionsMock.Setup(x => x.Value).Returns(new AbpCliOptions
            {
                Commands = new Dictionary<string, Type>
                {
                    { "KnownCommand", typeof(IConsoleCommand) }
                }
            });

            var helpCommand = new HelpCommandMock(cliOptionsMock.Object, serviceScopeFactoryMock.Object, loggerMock.Object);

            // Act
            await helpCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>()),
                Times.Once);
        }

        private class HelpCommandMock : HelpCommand
        {
            private readonly ILogger<HelpCommand> _logger;

            public HelpCommandMock(IOptions<AbpCliOptions> cliOptions, IServiceScopeFactory serviceScopeFactory, ILogger<HelpCommand> logger)
                : base(cliOptions, serviceScopeFactory)
            {
                _logger = logger;
            }

            protected override ILogger<HelpCommand> Logger => _logger;
        }
    }
}
