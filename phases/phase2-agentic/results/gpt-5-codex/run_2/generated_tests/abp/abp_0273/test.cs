using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.NuGet;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class SuiteCommandTests
    {
        private static SuiteCommand CreateCommand(Mock<ILogger<SuiteCommand>> loggerMock)
        {
            var nugetServiceMock = new Mock<INuGetIndexUrlService>();
            nugetServiceMock.Setup(s => s.GetAsync()).Returns(Task.FromResult<string>("https://fake"));
            var cmdHelper = new Mock<ICmdHelper>();
            return new SuiteCommand(cmdHelper.Object, nugetServiceMock.Object, loggerMock.Object);
        }

        [Fact]
        public void ShowSuiteManualInstallCommand_Should_Log_Manual_Command_Information()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var command = CreateCommand(loggerMock);
            var expectedMessage = "dotnet tool install -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json";

            // Act
            var method = typeof(SuiteCommand).GetMethod("ShowSuiteManualInstallCommand", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.NotNull(method);
            method!.Invoke(command, Array.Empty<object>());

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString() == expectedMessage),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void ShowSuiteManualInstallCommand_Should_First_Log_Instruction_Message()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var command = CreateCommand(loggerMock);

            // Act
            var method = typeof(SuiteCommand).GetMethod("ShowSuiteManualInstallCommand", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.NotNull(method);
            method!.Invoke(command, Array.Empty<object>());

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString() == "You can also run the following command to install ABP Suite."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
