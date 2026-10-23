using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Threading;
using Xunit;

public class LoginCommandTests
{
    public class TestableCommandLineArgs : CommandLineArgs
    {
        public new string Target { get; set; }
        public new Dictionary<string, string> Options { get; set; }
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogError_WhenLoginFails()
    {
        // Arrange
        var authServiceMock = new Mock<AuthService>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        var loggerMock = new Mock<ILogger<LoginCommand>>();

        authServiceMock.Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Some error message"));

        var loginCommand = new LoginCommand(authServiceMock.Object, cancellationTokenProviderMock.Object, null)
        {
            Logger = loggerMock.Object
        };

        var commandLineArgs = new TestableCommandLineArgs
        {
            Target = "testuser",
            Options = new Dictionary<string, string>
            {
                { "password", "testpassword" }
            }
        };

        // Act
        await loginCommand.ExecuteAsync(commandLineArgs);

        // Assert
        loggerMock.Verify(
            x => x.LogError("Some error message"),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogError_WhenDeviceLoginFails()
    {
        // Arrange
        var authServiceMock = new Mock<AuthService>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        var loggerMock = new Mock<ILogger<LoginCommand>>();

        authServiceMock.Setup(x => x.DeviceLoginAsync())
            .ThrowsAsync(new Exception("Some error message"));

        var loginCommand = new LoginCommand(authServiceMock.Object, cancellationTokenProviderMock.Object, null)
        {
            Logger = loggerMock.Object
        };

        var commandLineArgs = new TestableCommandLineArgs
        {
            Options = new Dictionary<string, string>
            {
                { "device", "true" }
            }
        };

        // Act
        await loginCommand.ExecuteAsync(commandLineArgs);

        // Assert
        loggerMock.Verify(
            x => x.LogError("Some error message"),
            Times.Once);
    }
}
