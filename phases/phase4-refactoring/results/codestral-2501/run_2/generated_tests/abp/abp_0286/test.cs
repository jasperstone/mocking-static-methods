using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json.Linq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Version;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public void StartSuite_WhenPortIsAlreadyInUse_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new TestSuiteCommand(
                Mock.Of<AbpNuGetIndexUrlService>(),
                Mock.Of<PackageVersionCheckerService>(),
                Mock.Of<ICmdHelper>(),
                Mock.Of<AuthService>(),
                Mock.Of<CliHttpClientFactory>(),
                Mock.Of<SuiteAppSettingsService>()
            )
            {
                Logger = loggerMock.Object
            };

            // Act
            suiteCommand.StartSuite();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Port \"{suiteCommand._abpSuitePort}\" is already in use.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        private class TestSuiteCommand : SuiteCommand
        {
            public TestSuiteCommand(
                AbpNuGetIndexUrlService nuGetIndexUrlService,
                PackageVersionCheckerService packageVersionCheckerService,
                ICmdHelper cmdHelper,
                AuthService authService,
                CliHttpClientFactory cliHttpClientFactory,
                SuiteAppSettingsService suiteAppSettingsService)
                : base(nuGetIndexUrlService, packageVersionCheckerService, cmdHelper, authService, cliHttpClientFactory, suiteAppSettingsService)
            {
            }

            protected override bool IsPortAlreadyInUse()
            {
                return true;
            }
        }
    }
}
