using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.AspNetCore.Server.IntegrationTesting.Common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class NginxDeployerTests
{
    private readonly Mock<ILogger<NginxDeployer>> _loggerMock;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly DeploymentParameters _deploymentParameters;
    private readonly NginxDeployer _nginxDeployer;

    public NginxDeployerTests()
    {
        _loggerMock = new Mock<ILogger<NginxDeployer>>();
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);
        _deploymentParameters = new DeploymentParameters
        {
            ApplicationPath = Path.GetTempPath(),
            ServerConfigTemplateContent = "[user][errorlog][accesslog][listenPort][redirectUri][pidFile]"
        };
        _nginxDeployer = new NginxDeployer(_deploymentParameters, _loggerFactoryMock.Object);
    }

    [Fact]
    public void SetupNginx_LogsConfigContent_WhenTraceIsEnabled()
    {
        // Arrange
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);

        // Act
        _nginxDeployer.GetType().GetMethod("SetupNginx", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(_nginxDeployer, new object[] { "http://redirectUri", new Uri("http://originalUri") });

        // Assert
        _loggerMock.Verify(x => x.Log(
            LogLevel.Trace,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Config File Content")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }

    [Fact]
    public void SetupNginx_ThrowsInvalidOperationException_WhenUserNameIsNull()
    {
        // Arrange
        var processMock = new Mock<Process>();
        processMock.Setup(x => x.StandardOutput.ReadToEnd()).Returns((string)null);
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "whoami",
            RedirectStandardOutput = true
        };
        processMock.Setup(x => x.StartInfo).Returns(processStartInfo);
        processMock.Setup(x => x.Start()).Returns(true);
        processMock.Setup(x => x.WaitForExit(10_000)).Returns(true);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _nginxDeployer.GetType().GetMethod("SetupNginx", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(_nginxDeployer, new object[] { "http://redirectUri", new Uri("http://originalUri") }));
    }

    [Fact]
    public void SetupNginx_LogsWarning_WhenPidFileDoesNotExist()
    {
        // Arrange
        var pidFile = Path.Combine(_deploymentParameters.ApplicationPath, "non_existent.pid");
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);

        // Act
        _nginxDeployer.GetType().GetMethod("SetupNginx", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(_nginxDeployer, new object[] { "http://redirectUri", new Uri("http://originalUri") });

        // Assert
        _loggerMock.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to find nginx PID file")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }
}
