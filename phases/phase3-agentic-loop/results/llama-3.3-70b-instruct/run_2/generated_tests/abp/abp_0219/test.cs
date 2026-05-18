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
            var commandBase = new NewCommand(
                null, null, null, null, null, null, null, null, null, null, null, null, null, null);
            commandBase.Logger = loggerMock.Object;
            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options.Add("cs", "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;");

            // Act
            commandBase.ExecuteAsync(commandLineArgs).Wait();

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Connection string: Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;"))), Times.Once);
        }

        [Fact]
        public void LogInformation_DatabaseProvider_LogsDatabaseProvider()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var commandBase = new NewCommand(
                null, null, null, null, null, null, null, null, null, null, null, null, null, null);
            commandBase.Logger = loggerMock.Object;
            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options.Add("db", "SqlServer");

            // Act
            commandBase.ExecuteAsync(commandLineArgs).Wait();

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Database provider: SqlServer"))), Times.Once);
        }

        [Fact]
        public void LogInformation_DatabaseManagementSystem_LogsDatabaseManagementSystem()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var commandBase = new NewCommand(
                null, null, null, null, null, null, null, null, null, null, null, null, null, null);
            commandBase.Logger = loggerMock.Object;
            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options.Add("dbms", "MySql");

            // Act
            commandBase.ExecuteAsync(commandLineArgs).Wait();

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("DBMS: MySql"))), Times.Once);
        }

        [Fact]
        public void LogInformation_UIFramework_LogsUIFramework()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var commandBase = new NewCommand(
                null, null, null, null, null, null, null, null, null, null, null, null, null, null);
            commandBase.Logger = loggerMock.Object;
            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options.Add("ui", "MVC");

            // Act
            commandBase.ExecuteAsync(commandLineArgs).Wait();

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("UI Framework: MVC"))), Times.Once);
        }
    }
}
