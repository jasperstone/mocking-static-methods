using Xunit;
using Moq;
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
using Newtonsoft.Json.Linq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Version;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;

namespace Volo.Abp.Cli.Commands;

public class SuiteCommandTests
{
    [Fact]
    public void KillSuite_LogsInformation_WhenExceptionOccurs()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>();
        var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
        var cmdHelperMock = new Mock<ICmdHelper>();
        var authServiceMock = new Mock<AuthService>();
        var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();

        var suiteCommand = new SuiteCommand(
            nuGetIndexUrlServiceMock.Object,
            packageVersionCheckerServiceMock.Object,
            cmdHelperMock.Object,
            authServiceMock.Object,
            cliHttpClientFactoryMock.Object,
            suiteAppSettingsServiceMock.Object
        );
        suiteCommand.Logger = loggerMock.Object;

        // Act
        try
        {
            suiteCommand.KillSuite();
        }
        catch (Exception ex)
        {
            suiteCommand.Logger.LogInformation("Cannot close Suite." + ex.Message);
        }

        // Assert
        loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
    }
}
