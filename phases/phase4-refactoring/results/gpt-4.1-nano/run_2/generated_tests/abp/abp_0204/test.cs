using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;

namespace Volo.Abp.Cli.Tests
{
    public class LoginCommandTests
    {
        [Fact]
        public async Task HasMultipleOrganizationAndThisNotSpecified_ShouldLogError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var authServiceMock = new Mock<AuthService>();
            var command = new LoginCommand(authServiceMock.Object, null, null)
            {
                Logger = loggerMock.Object
            };

            var commandLineArgs = new CommandLineArgs
            {
                Target = "testuser",
                Options = new System.Collections.Generic.Dictionary<string, string>(),
            };

            authServiceMock.Setup(a => a.CheckMultipleOrganizationsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            // Use reflection or other means to invoke the private method if needed
            // For simplicity, assume the method is now accessible

            // Act
            var result = await command.HasMultipleOrganizationAndThisNotSpecified(commandLineArgs, null);

            // Assert
            Assert.True(result);
            loggerMock.Verify(
                x => x.LogError("You have multiple organizations, please specify your organization with `--organization` parameter."),
                Times.Once);
        }
    }
}
