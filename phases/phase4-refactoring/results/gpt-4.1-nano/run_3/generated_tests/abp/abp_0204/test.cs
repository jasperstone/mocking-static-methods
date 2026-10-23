using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Reflection;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;

namespace Volo.Abp.Cli.Tests
{
    public class LoginCommandErrorLoggingTests
    {
        private static object CreateLoginCommandWithLogger(ILogger<LoginCommand> logger)
        {
            var authService = new Mock<AuthService>().Object;
            var command = new LoginCommand(authService, null, null)
            {
                Logger = logger
            };
            return command;
        }

        [Fact]
        public void LogCliError_Should_LogError_ForInvalidUsername()
        {
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var command = (LoginCommand)CreateLoginCommandWithLogger(loggerMock.Object);
            var ex = new Exception("Invalid username or password");
            var method = typeof(LoginCommand).GetMethod("LogCliError", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(command, new object[] { ex, new CommandLineArgs() });
            loggerMock.Verify(x => x.LogError("Invalid username or password!"), Times.Once);
        }

        [Fact]
        public void LogCliError_Should_LogError_ForRequiresTwoFactor()
        {
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var command = (LoginCommand)CreateLoginCommandWithLogger(loggerMock.Object);
            var ex = new Exception("RequiresTwoFactor");
            var method = typeof(LoginCommand).GetMethod("LogCliError", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(command, new object[] { ex, new CommandLineArgs() });
            loggerMock.Verify(x => x.LogError("Two factor authentication is enabled for your account. Please use `abp login --device` command to login."), Times.Once);
        }

        [Fact]
        public void LogCliError_Should_LogError_FromHtmlPage()
        {
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var command = (LoginCommand)CreateLoginCommandWithLogger(loggerMock.Object);
            string htmlError = "<div class=\"error-page-container\"><h2 class=\"text-danger\">Error occurred</h2></div>";
            var ex = new Exception(htmlError);
            var method = typeof(LoginCommand).GetMethod("LogCliError", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(command, new object[] { ex, new CommandLineArgs() });
            loggerMock.Verify(x => x.LogError("Error occurred"), Times.Once);
        }

        [Fact]
        public void LogCliError_Should_LogError_Message_When_NoSpecificCondition()
        {
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var command = (LoginCommand)CreateLoginCommandWithLogger(loggerMock.Object);
            var ex = new Exception("Some other error");
            var method = typeof(LoginCommand).GetMethod("LogCliError", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(command, new object[] { ex, new CommandLineArgs() });
            loggerMock.Verify(x => x.LogError("Some other error"), Times.Once);
        }
    }
}
