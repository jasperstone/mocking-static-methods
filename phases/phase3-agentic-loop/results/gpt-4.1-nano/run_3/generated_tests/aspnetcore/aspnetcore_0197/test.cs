using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting
{
    public class NginxDeployerTests
    {
        private class TestNginxDeployer : NginxDeployer
        {
            public List<string> LogMessages { get; } = new List<string>();
            public bool IsTraceLogged { get; private set; } = false;
            public string LastConfigContent { get; private set; }
            public bool WriteAllTextCalled { get; private set; } = false;
            public bool StartProcessCalled { get; private set; } = false;
            public bool WaitForExitCalled { get; private set; } = false;
            public int ProcessExitCode { get; set; } = 0;
            public bool FileExistsReturn { get; set; } = true;
            public string PidFileContent { get; set; } = "1234";

            public TestNginxDeployer(DeploymentParameters parameters, ILoggerFactory loggerFactory)
                : base(parameters, loggerFactory)
            {
            }

            protected override void LogDebug(string message, params object[] args)
            {
                LogMessages.Add($"Debug: {string.Format(message, args)}");
            }

            protected override void LogWarning(string message, params object[] args)
            {
                LogMessages.Add($"Warning: {string.Format(message, args)}");
            }

            protected override void LogInformation(string message, params object[] args)
            {
                LogMessages.Add($"Info: {string.Format(message, args)}");
            }

            protected override bool IsEnabled(LogLevel level)
            {
                if (level == LogLevel.Trace)
                {
                    IsTraceLogged = true;
                }
                return true;
            }

            protected override void LogTrace(string message, params object[] args)
            {
                LogMessages.Add($"Trace: {string.Format(message, args)}");
            }

            protected override void WriteAllText(string path, string content)
            {
                LastConfigContent = content;
                WriteAllTextCalled = true;
            }

            protected override void StartAndCaptureOutAndErrToLogger(Process process, string description, ILogger logger)
            {
                StartProcessCalled = true;
            }

            protected override void WaitForExit(Process process, int milliseconds)
            {
                WaitForExitCalled = true;
                process.ExitCode = ProcessExitCode;
            }

            protected override bool FileExists(string path)
            {
                return FileExistsReturn;
            }

            protected override string ReadAllText(string path)
            {
                return PidFileContent;
            }
        }

        [Fact]
        public void SetupNginx_LogsTrace_WhenTraceEnabled()
        {
            // Arrange
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var deploymentParams = new DeploymentParameters
            {
                ApplicationPath = "/tmp/app",
                ServerConfigTemplateContent = "config content with placeholders"
            };

            var deployer = new TestNginxDeployer(deploymentParams, mockLoggerFactory.Object);
            // Act
            deployer.SetupNginx("http://redirect", new Uri("http://localhost:5000"));

            // Assert
            Assert.True(deployer.IsTraceLogged);
            Assert.Contains("Config File Content:", deployer.LogMessages.First());
        }

        [Fact]
        public void SetupNginx_WritesConfigContent()
        {
            // Arrange
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var deploymentParams = new DeploymentParameters
            {
                ApplicationPath = "/tmp/app",
                ServerConfigTemplateContent = "config content with placeholders"
            };

            var deployer = new TestNginxDeployer(deploymentParams, mockLoggerFactory.Object);
            // Act
            deployer.SetupNginx("http://redirect", new Uri("http://localhost:5000"));

            // Assert
            Assert.True(deployer.WriteAllTextCalled);
            Assert.Contains("config content with placeholders", deployer.LastConfigContent);
        }

        [Fact]
        public void SetupNginx_LogsWarning_WhenPidFileMissing()
        {
            // Arrange
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var deploymentParams = new DeploymentParameters
            {
                ApplicationPath = "/tmp/app",
                ServerConfigTemplateContent = "config content with placeholders"
            };

            var deployer = new TestNginxDeployer(deploymentParams, mockLoggerFactory.Object);
            deployer.FileExistsReturn = false; // simulate missing pid file

            // Act
            deployer.SetupNginx("http://redirect", new Uri("http://localhost:5000"));

            // Assert
            Assert.Contains(deployer.LogMessages, msg => msg.StartsWith("Warning:"));
        }

        [Fact]
        public void SetupNginx_LogsInfo_WhenPidFileExists()
        {
            // Arrange
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var deploymentParams = new DeploymentParameters
            {
                ApplicationPath = "/tmp/app",
                ServerConfigTemplateContent = "config content with placeholders"
            };

            var deployer = new TestNginxDeployer(deploymentParams, mockLoggerFactory.Object);
            deployer.FileExistsReturn = true; // simulate existing pid file
            deployer.PidFileContent = "5678";

            // Act
            deployer.SetupNginx("http://redirect", new Uri("http://localhost:5000"));

            // Assert
            Assert.Contains(deployer.LogMessages, msg => msg.Contains("started"));
        }
    }
}
