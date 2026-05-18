using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting
{
    public class SelfHostDeployerTests
    {
        [Fact]
        public async Task StartSelfHostAsync_LogsStartedInformation_WhenProcessStartsSuccessfully()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SelfHostDeployer>>();
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new TestLoggerProvider(mockLogger.Object));

            var deploymentParameters = new DeploymentParameters(new ApplicationTestFixture()
            {
                ApplicationPath = "/test/app",
                ApplicationName = "TestApp",
                ServerType = ServerType.Kestrel,
                RuntimeFlavor = RuntimeFlavor.CoreClr,
                ApplicationType = ApplicationType.Portable,
                Configuration = "Debug"
            });

            var deployer = new FakeSelfHostDeployer(deploymentParameters, loggerFactory);

            // Mock the process start to succeed without throwing
            deployer.SetMockProcess(new MockProcess());

            // Act
            var result = await deployer.StartSelfHostAsync(new Uri("http://localhost"));

            // Assert
            mockLogger.Verify(
                x => x.LogInformation(
                    "Started {fileName}. Process Id : {processId}",
                    It.IsAny<string>(),
                    It.IsAny<int>()),
                Times.Once);
        }

        private class FakeSelfHostDeployer : SelfHostDeployer
        {
            private Process _mockProcess;

            public FakeSelfHostDeployer(DeploymentParameters deploymentParameters, ILoggerFactory loggerFactory)
                : base(deploymentParameters, loggerFactory)
            {
            }

            public void SetMockProcess(Process mockProcess)
            {
                _mockProcess = mockProcess;
            }

            protected override Process CreateProcess() => _mockProcess ?? new MockProcess();
        }

        private class MockProcess : Process
        {
            public MockProcess()
            {
                StartInfo = new ProcessStartInfo();
                Id = 12345;
                HasExited = false;
            }

            public override void StartAndCaptureOutAndErrToLogger(string executableName, ILogger logger)
            {
                // Simulate successful start without actually starting
            }
        }

        private class TestLoggerProvider : ILoggerProvider
        {
            private readonly ILogger _logger;

            public TestLoggerProvider(ILogger logger) => _logger = logger;

            public ILogger CreateLogger(string categoryName) => _logger;

            public void Dispose() { }
        }
    }
}
