using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Server.IntegrationTesting.Common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting;

public class NginxDeployerTests
{
    [Fact]
    public void SetupNginx_LogsDebugMessagesWithCorrectParameters()
    {
        // Arrange
        var deploymentParameters = new DeploymentParameters(Path.GetTempPath(), ServerType.Nginx, RuntimeFlavor.CoreClr, RuntimeArchitecture.x64);
        deploymentParameters.ApplicationPath = Path.GetTempPath();
        deploymentParameters.ServerConfigTemplateContent = "[user][errorlog][accesslog][listenPort][redirectUri][pidFile]";

        var mockLogger = new Mock<ILogger<NginxDeployer>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var deployer = new NginxDeployer(deploymentParameters, loggerFactory.Object);

        // Mock GetUserName using reflection to replace the private static method
        var getUserNameMethod = typeof(NginxDeployer).GetMethod("GetUserName", BindingFlags.NonPublic | BindingFlags.Static);
        var dm = new DynamicMethod("MockGetUserName", typeof(string), Type.EmptyTypes, typeof(NginxDeployer).Module);
        var il = dm.GetILGenerator();
        il.Emit(OpCodes.Ldstr, "testuser");
        il.Emit(OpCodes.Ret);
        var mockGetUserName = dm.CreateDelegate(typeof(Func<string>));
        getUserNameMethod!.CreateDelegate(typeof(Func<string>), mockGetUserName);

        // Act
        deployer.SetupNginx("http://localhost:5000", new Uri("http://localhost:8080"));

        // Assert - Verify the LogDebug calls at line 148 and following
        mockLogger.Verify(
            l => l.LogDebug(
                "Using PID file: {pidFile}",
                It.IsAny<object[]>(),
                It.IsAny<Exception>()),
            Times.Once);

        mockLogger.Verify(
            l => l.LogDebug(
                "Using Error Log file: {errorLog}",
                It.IsAny<object[]>(),
                It.IsAny<Exception>()),
            Times.Once);

        mockLogger.Verify(
            l => l.LogDebug(
                "Using Access Log file: {accessLog}",
                It.IsAny<object[]>(),
                It.IsAny<Exception>()),
            Times.Once);
    }
}
