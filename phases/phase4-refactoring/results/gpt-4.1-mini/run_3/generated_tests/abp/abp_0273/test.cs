using System;
using System.Threading.Tasks;
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
            suiteCommand.GetType()
                .GetMethod("ShowSuiteManualInstallCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(suiteCommand, null);

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
            // We create SuiteCommand with null dependencies because we only test ShowSuiteManualInstallCommand which only uses Logger.
            return (SuiteCommand)Activator.CreateInstance(
                typeof(SuiteCommand),
                new object[] { null, null, null, null, null, null });
        }
    }
}
