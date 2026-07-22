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
        public void ShowSuiteManualInstallCommand_LogsExpectedInformationMessages()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = CreateSuiteCommand();
            suiteCommand.Logger = loggerMock.Object;

            // Act
            // Call the private method via reflection since it's private
            var method = typeof(SuiteCommand).GetMethod("ShowSuiteManualInstallCommand", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(suiteCommand, null);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "You can also run the following command to install ABP Suite."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "dotnet tool install -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json"),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private SuiteCommand CreateSuiteCommand()
        {
            // Use null for dependencies that are not used in this test
            return (SuiteCommand)Activator.CreateInstance(
                typeof(SuiteCommand),
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                new object[] { null, null, null, null, null, null },
                null);
        }
    }
}
