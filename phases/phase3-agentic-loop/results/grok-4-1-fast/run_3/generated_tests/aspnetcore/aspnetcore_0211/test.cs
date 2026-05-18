using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.AspNetCore.Server.IntegrationTesting.Common;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class SelfHostDeployerTests
    {
        [Fact]
        public async Task StartSelfHostAsync_LogsExecutableCommandLine()
        {
            // Arrange
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var deploymentParameters = new DeploymentParameters
            {
                ApplicationName = "TestApp",
                ApplicationPath = Path.GetTempPath(),
                Configuration = "Release",
                RuntimeFlavor = RuntimeFlavor.CoreClr,
                ApplicationType = ApplicationType.Portable,
                ServerType = ServerType.Kestrel,
                PublishApplicationBeforeDeployment = false
            };

            var deployer = new SelfHostDeployer(deploymentParameters, loggerFactory);
            
            // Capture logger calls by replacing the logger
            var logMessages = new System.Collections.Generic.List<string>();
            var mockLogger = new Mock<ILogger<SelfHostDeployer>>();
            mockLogger.Setup(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((state, ex) => 
                {
                    logMessages.Add(state?.ToString() ?? "");
                    return "";
                })))
                .Returns((LogLevel level, EventId id, object state, Exception ex, Func<object, Exception, string> formatter) => "");

            // Use reflection to replace the logger since it's protected
            var loggerField = typeof(ApplicationDeployer).GetField("_logger", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField?.SetValue(deployer, mockLogger.Object);

            var hintUrl = new Uri("http://localhost:5000");

            // Act
            await deployer.StartSelfHostAsync(hintUrl);

            // Assert
            Assert.Contains(logMessages, msg => 
                msg.StartsWith("Executing ") && 
                msg.Contains("TestApp") && 
                msg.Contains("--urls http://localhost:5000") &&
                msg.Contains("Microsoft.AspNetCore.Server.Kestrel"));
        }
    }
}
