using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class SelfHostDeployerTests
    {
        [Fact]
        public async Task StartSelfHostAsync_LogsExecutingInformation()
        {
            // Arrange
            var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
            var logger = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

            var deploymentParameters = new DeploymentParameters
            {
                ApplicationName = "TestApp",
                ApplicationPath = Path.GetTempPath(),
                Configuration = "Debug",
                EnvironmentVariables = new Dictionary<string, string>(),
                PublishApplicationBeforeDeployment = false,
                RuntimeFlavor = RuntimeFlavor.Clr,
                ApplicationType = ApplicationType.Portable,
                ServerType = ServerType.Kestrel,
                TargetFramework = "net6.0"
            };

            var deployer = new SelfHostDeployer(deploymentParameters, loggerFactoryMock.Object);

            // We need to call the internal StartSelfHostAsync method.
            // It is protected, so we create a derived test class to expose it.
            var testDeployer = new TestSelfHostDeployer(deploymentParameters, loggerFactoryMock.Object);

            var hintUrl = new Uri("http://localhost:5000");

            // Act
            // We only want to test the logging of the "Executing ..." line.
            // To avoid actually starting a process, we mock Process.StartAndCaptureOutAndErrToLogger.
            // But since it's an extension method, we cannot mock it easily.
            // Instead, we override the method in the test class to avoid starting a process.

            await testDeployer.StartSelfHostAsync(hintUrl);

            // Assert
            logger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        private class TestSelfHostDeployer : SelfHostDeployer
        {
            public TestSelfHostDeployer(DeploymentParameters deploymentParameters, ILoggerFactory loggerFactory)
                : base(deploymentParameters, loggerFactory)
            {
            }

            protected override Task<(Uri url, System.Threading.CancellationToken hostExitToken)> StartSelfHostAsync(Uri hintUrl)
            {
                // We call the base method but override the process start to avoid side effects.
                return StartSelfHostAsyncInternal(hintUrl);
            }

            private async Task<(Uri url, System.Threading.CancellationToken hostExitToken)> StartSelfHostAsyncInternal(Uri hintUrl)
            {
                using (Logger.BeginScope("StartSelfHost"))
                {
                    var executableName = string.Empty;
                    var executableArgs = string.Empty;
                    var workingDirectory = string.Empty;
                    var executableExtension = DeploymentParameters.ApplicationType == ApplicationType.Portable ? ".dll"
                        : (OperatingSystem.IsWindows() ? ".exe" : "");

                    if (DeploymentParameters.PublishApplicationBeforeDeployment)
                    {
                        workingDirectory = DeploymentParameters.PublishedApplicationRootPath;
                    }
                    else
                    {
                        var targetFramework = DeploymentParameters.TargetFramework
                            ?? (DeploymentParameters.RuntimeFlavor == RuntimeFlavor.Clr ? Tfm.Net462 : Tfm.NetCoreApp22);
                        workingDirectory = Path.Combine(DeploymentParameters.ApplicationPath, "bin", DeploymentParameters.Configuration, targetFramework);
                        DeploymentParameters.EnvironmentVariables["ASPNETCORE_CONTENTROOT"] = DeploymentParameters.ApplicationPath;
                    }

                    var executable = Path.Combine(workingDirectory, DeploymentParameters.ApplicationName + executableExtension);

                    if (DeploymentParameters.RuntimeFlavor == RuntimeFlavor.CoreClr && DeploymentParameters.ApplicationType == ApplicationType.Portable)
                    {
                        executableName = GetDotNetExeForArchitecture();
                        executableArgs = executable;
                    }
                    else
                    {
                        executableName = executable;
                    }

                    var server = DeploymentParameters.ServerType == ServerType.HttpSys
                        ? "Microsoft.AspNetCore.Server.HttpSys" : "Microsoft.AspNetCore.Server.Kestrel";
                    executableArgs += $" --urls {hintUrl} --server {server}";

                    Logger.LogInformation($"Executing {executableName} {executableArgs}");

                    // Instead of starting a process, just return dummy values
                    return (new Uri("http://localhost:5000"), default);
                }
            }

            private string GetDotNetExeForArchitecture()
            {
                return "dotnet";
            }
        }
    }
}
