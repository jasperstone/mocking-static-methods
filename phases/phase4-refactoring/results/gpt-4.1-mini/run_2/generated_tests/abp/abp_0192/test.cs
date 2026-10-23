using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Cli;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class HelpCommandTests
    {
        private class FakeCommand : IConsoleCommand
        {
            public Task ExecuteAsync(CommandLineArgs commandLineArgs) => Task.CompletedTask;
            public string GetUsageInfo() => "Fake command usage info";
        }

        private class TestOptions : IOptions<AbpCliOptions>
        {
            public AbpCliOptions Value { get; }

            public TestOptions(AbpCliOptions options)
            {
                Value = options;
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

            var args = new CommandLineArgs(null, null);

            // Act
            await helpCommand.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(l => l.Log(
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

            var args = new CommandLineArgs(null, "nonexistent");

            // Act
            await helpCommand.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("There is no command named")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(l => l.Log(
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
            var options = new AbpCliOptions();
            options.Commands["fake"] = typeof(FakeCommand);

            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var fakeCommandInstance = new FakeCommand();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(FakeCommand))).Returns(fakeCommandInstance);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(FakeCommand))).Returns(fakeCommandInstance);
            serviceScopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);

            var helpCommand = new HelpCommand(new TestOptions(options), serviceScopeFactoryMock.Object)
            {
                Logger = loggerMock.Object
            };

            var args = new CommandLineArgs(null, "fake");

            // Act
            await helpCommand.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Fake command usage info")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
