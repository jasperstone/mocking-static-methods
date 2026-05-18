using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Http;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class SuiteCommandTests
    {
        [Fact]
        public void KillSuite_LogsInformation_WhenSuiteProcessesKilled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>(MockBehavior.Strict, null);
            var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>(MockBehavior.Strict, null);
            var authServiceMock = new Mock<AuthService>(MockBehavior.Strict, null);
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>(MockBehavior.Strict, null);
            var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>(MockBehavior.Strict, null);

            var processes = new List<Process>();
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.ProcessName).Returns("abp-suite");
            processMock.Setup(p => p.Kill());
            processes.Add(processMock.Object);

            var suiteCommand = new TestSuiteCommand(
                nuGetIndexUrlServiceMock.Object,
                packageVersionCheckerServiceMock.Object,
                cmdHelperMock.Object,
                authServiceMock.Object,
                cliHttpClientFactoryMock.Object,
                suiteAppSettingsServiceMock.Object,
                processes
            );
            suiteCommand.Logger = loggerMock.Object;

            // Act
            suiteCommand.InvokeKillSuite();

            // Assert
            processMock.Verify(p => p.Kill(), Times.Once);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Suite closed."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void KillSuite_LogsInformation_WhenExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>(MockBehavior.Strict, null);
            var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>(MockBehavior.Strict, null);
            var authServiceMock = new Mock<AuthService>(MockBehavior.Strict, null);
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>(MockBehavior.Strict, null);
            var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>(MockBehavior.Strict, null);

            var processes = new List<Process>();
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.ProcessName).Returns("abp-suite");
            processMock.Setup(p => p.Kill()).Throws(new InvalidOperationException("Test exception"));
            processes.Add(processMock.Object);

            var suiteCommand = new TestSuiteCommand(
                nuGetIndexUrlServiceMock.Object,
                packageVersionCheckerServiceMock.Object,
                cmdHelperMock.Object,
                authServiceMock.Object,
                cliHttpClientFactoryMock.Object,
                suiteAppSettingsServiceMock.Object,
                processes
            );
            suiteCommand.Logger = loggerMock.Object;

            // Act
            suiteCommand.InvokeKillSuite();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().StartsWith("Cannot close Suite.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class TestSuiteCommand : SuiteCommand
        {
            private readonly IEnumerable<Process> _processes;

            public TestSuiteCommand(
                AbpNuGetIndexUrlService nuGetIndexUrlService,
                PackageVersionCheckerService packageVersionCheckerService,
                ICmdHelper cmdHelper,
                AuthService authService,
                CliHttpClientFactory cliHttpClientFactory,
                SuiteAppSettingsService suiteAppSettingsService,
                IEnumerable<Process> processes)
                : base(nuGetIndexUrlService, packageVersionCheckerService, cmdHelper, authService, cliHttpClientFactory, suiteAppSettingsService)
            {
                _processes = processes;
            }

            protected override IEnumerable<Process> GetProcessesRelatedWithSuite()
            {
                return _processes;
            }

            public void InvokeKillSuite()
            {
                base.KillSuite();
            }
        }
    }
}
