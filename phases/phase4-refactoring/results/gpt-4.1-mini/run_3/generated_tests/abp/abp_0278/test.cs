using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.Volo.Abp.Cli.Commands
{
    public class SuiteCommandTests
    {
        [Fact]
        public void ShowSuiteManualUpdateCommand_LogsExpectedErrorMessages()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = CreateSuiteCommand();
            suiteCommand.Logger = loggerMock.Object;

            // Act
            var method = typeof(SuiteCommand).GetMethod("ShowSuiteManualUpdateCommand", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(suiteCommand, null);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "You can also run the following command to update ABP Suite."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "dotnet tool update -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json"),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private SuiteCommand CreateSuiteCommand()
        {
            // Use null or default for constructor parameters since we won't call methods that use them
            return (SuiteCommand)Activator.CreateInstance(
                typeof(SuiteCommand),
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                new object[] { null, null, null, null, null, null },
                null);
        }
    }
}
