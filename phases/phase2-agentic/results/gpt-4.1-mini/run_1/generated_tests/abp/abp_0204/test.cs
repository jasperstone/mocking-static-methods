using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class LoginCommandTests
{
    private readonly Mock<AuthService> _authServiceMock;
    private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
    private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
    private readonly Mock<ILogger<LoginCommand>> _loggerMock;
    private readonly LoginCommand _loginCommand;

    public LoginCommandTests()
    {
        _authServiceMock = new Mock<AuthService>(MockBehavior.Strict, null, null, null);
        _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
        _loggerMock = new Mock<ILogger<LoginCommand>>();
        _loginCommand = new LoginCommand(_authServiceMock.Object, _cancellationTokenProviderMock.Object, _remoteServiceExceptionHandlerMock.Object)
        {
            Logger = _loggerMock.Object
        };
    }

    [Fact]
    public void LogCliError_LogsInvalidUsernameOrPassword()
    {
        var ex = new Exception("Invalid username or password");
        var args = new CommandLineArgs();

        _loggerMock.Setup(l => l.LogError("Invalid username or password!"));

        _loginCommand.InvokePrivateMethod("LogCliError", ex, args);

        _loggerMock.Verify(l => l.LogError("Invalid username or password!"), Times.Once);
    }

    [Fact]
    public void LogCliError_LogsRequiresTwoFactor()
    {
        var ex = new Exception("RequiresTwoFactor");
        var args = new CommandLineArgs();

        _loggerMock.Setup(l => l.LogError("Two factor authentication is enabled for your account. Please use `abp login --device` command to login."));

        _loginCommand.InvokePrivateMethod("LogCliError", ex, args);

        _loggerMock.Verify(l => l.LogError("Two factor authentication is enabled for your account. Please use `abp login --device` command to login."), Times.Once);
    }

    [Fact]
    public void LogCliError_LogsErrorMessageFromHtmlPage()
    {
        var htmlMessage = "<div class=\"error-page-container\"><h2 class=\"text-danger\">Some error<eof/></h2></div>";
        var ex = new Exception(htmlMessage);
        var args = new CommandLineArgs();

        _loggerMock.Setup(l => l.LogError("Some error"));

        _loginCommand.InvokePrivateMethod("LogCliError", ex, args);

        _loggerMock.Verify(l => l.LogError("Some error"), Times.Once);
    }

    [Fact]
    public void LogCliError_LogsExceptionMessageWhenNoOtherConditionMatches()
    {
        var ex = new Exception("Some other error message");
        var args = new CommandLineArgs();

        _loggerMock.Setup(l => l.LogError("Some other error message"));

        _loginCommand.InvokePrivateMethod("LogCliError", ex, args);

        _loggerMock.Verify(l => l.LogError("Some other error message"), Times.Once);
    }
}

internal static class PrivateMethodInvoker
{
    public static void InvokePrivateMethod(this object obj, string methodName, params object[] parameters)
    {
        var method = obj.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (method == null)
            throw new InvalidOperationException($"Method {methodName} not found on type {obj.GetType().FullName}");
        method.Invoke(obj, parameters);
    }
}
