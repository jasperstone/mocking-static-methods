using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using System.Threading.Tasks;

namespace DeploymentTests
{
    public class SelfHostDeployerTests
    {
        private class DummyLogger : ILogger
        {
            public List<string> LoggedMessages = new List<string>();
            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                LoggedMessages.Add(formatter(state, exception));
            }
        }

        [Fact]
        public void LogInformation_CalledOnStartSelfHostAsync()
        {
            // Arrange
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new DummyLogger();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger);

            var deploymentParameters = new DeploymentParameters
            {
                ApplicationPath = "/app",
                ApplicationName = "MyApp",
                PublishApplicationBeforeDeployment = false,
                RuntimeFlavor = RuntimeFlavor.CoreClr,
                ApplicationType = ApplicationType.Portable,
                ServerType = ServerType.Kestrel,
                Scheme = "http",
                ApplicationBaseUriHint = "http://localhost:5000",
                StatusMessagesEnabled = false,
                EnvironmentVariables = new Dictionary<string, string>(),
                TargetFramework = null
            };

            var deployer = new SelfHostDeployer(deploymentParameters, mockLoggerFactory.Object);

            // Mock the internal method to avoid actual process start
            var startCalled = false;
            deployer.GetType().GetMethod("StartSelfHostAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .CreateDelegate<Func<Uri, Task<(Uri, System.Threading.CancellationToken)>>>(
                    (Uri url) =>
                    {
                        startCalled = true;
                        return Task.FromResult((new Uri("http://localhost:5000"), new System.Threading.CancellationToken()));
                    });

            // Act
            var task = deployer.StartSelfHostAsync(new Uri("http://localhost:5000"));
            task.Wait();

            // Assert
            Assert.Contains("Executing", string.Join("\n", mockLogger.LoggedMessages));
        }
    }

    // Dummy classes to simulate the environment
    public class DeploymentParameters
    {
        public string ApplicationPath { get; set; }
        public string ApplicationName { get; set; }
        public bool PublishApplicationBeforeDeployment { get; set; }
        public RuntimeFlavor RuntimeFlavor { get; set; }
        public ApplicationType ApplicationType { get; set; }
        public ServerType ServerType { get; set; }
        public string Scheme { get; set; }
        public string ApplicationBaseUriHint { get; set; }
        public bool StatusMessagesEnabled { get; set; }
        public Dictionary<string, string> EnvironmentVariables { get; set; }
        public string TargetFramework { get; set; }
        public string PublishedApplicationRootPath { get; set; }
        public RuntimeArchitecture RuntimeArchitecture { get; set; }
    }

    public enum RuntimeFlavor { Clr, CoreClr }
    public enum ApplicationType { Portable, Standalone }
    public enum ServerType { HttpSys, Kestrel }
    public enum RuntimeArchitecture { x86, x64 }

    // Extension method placeholder
    public static class ProcessExtensions
    {
        public static void StartAndCaptureOutAndErrToLogger(this Process process, string executableName, ILogger logger)
        {
            // No-op for testing
        }
    }
}
