using System;
using System.IO;
using Microsoft.AspNetCore.Server.IntegrationTesting.Common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Deployers
{
    public class NginxDeployerTests
    {
        [Fact]
        public void SetupNginx_LogsTrace_WhenTraceEnabled()
        {
            // Arrange
            var logger = new Mock<ILogger<NginxDeployer>>();
            logger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            
            var deploymentParameters = new DeploymentParameters
            {
                ApplicationPath = Path.GetTempPath(),
                ServerConfigTemplateContent = "template content"
            };
            
            var loggerFactory = new Mock<ILoggerFactory>();
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            
            var deployer = new NginxDeployer(deploymentParameters, loggerFactory.Object);
            
            // Set private _configFile field using reflection
            var configFileField = typeof(NginxDeployer).GetField("_configFile", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            configFileField?.SetValue(deployer, Path.GetTempFileName());

            // Act
            deployer.SetupNginx("http://redirect", new Uri("http://localhost:8080"));

            // Assert
            logger.Verify(
                l => l.LogTrace(
                    It.Is<string>(s => s.Contains("Config File Content") && 
                                     s.Contains("===START CONFIG===") && 
                                     s.Contains("===END CONFIG===")),
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public void SetupNginx_DoesNotLogTrace_WhenTraceDisabled()
        {
            // Arrange
            var logger = new Mock<ILogger<NginxDeployer>>();
            logger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
            
            var deploymentParameters = new DeploymentParameters
            {
                ApplicationPath = Path.GetTempPath(),
                ServerConfigTemplateContent = "template content"
            };
            
            var loggerFactory = new Mock<ILoggerFactory>();
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            
            var deployer = new NginxDeployer(deploymentParameters, loggerFactory.Object);
            
            // Set private _configFile field using reflection
            var configFileField = typeof(NginxDeployer).GetField("_configFile", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            configFileField?.SetValue(deployer, Path.GetTempFileName());

            // Act
            deployer.SetupNginx("http://redirect", new Uri("http://localhost:8080"));

            // Assert
            logger.Verify(
                l => l.LogTrace(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Never);
        }
    }
}
