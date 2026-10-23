using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class ProjectCreationCommandBaseTests
    {
        [Fact]
        public async Task LogInformation_CalledWithConnectionString()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var command = new NewCommand(
                null, null, null, null, null, null, null, null, null, null, null, null, null)
            {
                Logger = loggerMock.Object
            };

            var commandLineArgs = new CommandLineArgs
            {
                Options = new AbpCommandLineOptions
                {
                    { "connectionString", "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;" }
                }
            };

            // Act
            await command.GetProjectBuildArgsAsync(commandLineArgs, "", "");

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Connection string:"))), Times.Once);
        }
    }
}
