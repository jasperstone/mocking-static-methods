using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class ProjectCreationCommandBaseTests
    {
        [Fact]
        public void LogInformation_ConnectionString_LogsConnectionString()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var commandBase = new Mock<ProjectCreationCommandBase>(
                null, null, null, null, null, null, null, null, null, null, null, null, null);
            commandBase.SetupGet(cb => cb.Logger).Returns(loggerMock.Object);

            var connectionString = "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;";

            // Act
            commandBase.Object.Logger.LogInformation($"Connection string: {connectionString}");

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(It.Is<string>(s => s.Contains(connectionString))),
                Times.Once);
        }
    }
}
