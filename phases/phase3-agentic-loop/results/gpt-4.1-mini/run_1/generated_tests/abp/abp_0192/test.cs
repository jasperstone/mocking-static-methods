using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class HelpCommandTests
    {
        private class TestCommand : IConsoleCommand
        {
            public Task ExecuteAsync(CommandLineArgs commandLineArgs) => Task.CompletedTask;
            public string GetUsageInfo() => "TestCommand Usage Info";
            public static string GetShortDescription() => "Test command short description";
        }

        private class TestOptions : IOptions<AbpCliOptions>
        {
            public AbpCliOptions Value { get; }

            public TestOptions(AbpCliOptions value)
            {
                Value = value;
            }
        }

        [Fact]
        public async Task ExecuteAsync_LogsUsageInfo_WhenTargetIsNullOrWhitespace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var options = new AbpCliOptions();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

            var helpCommand = new HelpCommand(new TestOptions(options), serviceScopeFactoryMock.Object)
            {
                Logger = loggerMock.Object
            };

            var args = new CommandLineArgs(target: " ");

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
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var options = new AbpCliOptions();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

            var helpCommand = new HelpCommand(new TestOptions(options), serviceScopeFactoryMock.Object)
            {
                Logger = loggerMock.Object
            };

            var args = new CommandLineArgs(target: "nonexistent");

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
            var loggerMock = new Mock<ILogger<HelpCommand>>();
            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

            var commands = new Dictionary<string, Type>
            {
                { "test", typeof(TestCommand) }
            };

            var options = new AbpCliOptions();
            foreach (var cmd in commands)
            {
                options.Commands.Add(cmd.Key, cmd.Value);
            }

            var testCommand = new TestCommand();

            // Setup GetService to return testCommand for typeof(TestCommand)
            serviceProviderMock.Setup(sp => sp.GetService(typeof(TestCommand))).Returns(testCommand);
            // Setup GetRequiredService to call GetService internally (simulate)
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(TestCommand)))
                .Returns((Type t) => serviceProviderMock.Object.GetService(t));

            serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
            serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);

            var helpCommand = new HelpCommand(new TestOptions(options), serviceScopeFactoryMock.Object)
            {
                Logger = loggerMock.Object
            };

            var args = new CommandLineArgs(target: "test");

            // Act
            await helpCommand.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("TestCommand Usage Info")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
