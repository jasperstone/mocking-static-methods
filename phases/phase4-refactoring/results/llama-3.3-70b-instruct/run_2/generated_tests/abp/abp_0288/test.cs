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
        var apiKeyServiceMock = new Mock<IApiKeyService>();
        var jsonSerializerMock = new Mock<IJsonSerializer>();
        var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        var cliOptionsMock = new Mock<IOptionsSnapshot<AbpCliOptions>>();
        var authenticationServiceMock = new Mock<IIdentityModelAuthenticationService>();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var cmdHelperMock = new Mock<ICmdHelper>();

        var abpNuGetIndexUrlService = new AbpNuGetIndexUrlService(apiKeyServiceMock.Object);
        var packageVersionCheckerService = new PackageVersionCheckerService(jsonSerializerMock.Object, remoteServiceExceptionHandlerMock.Object, cancellationTokenProviderMock.Object, apiKeyServiceMock.Object, new CliHttpClientFactory(httpClientFactoryMock.Object, cancellationTokenProviderMock.Object));
        var cmdHelper = new CmdHelper(cliOptionsMock.Object);
        var authService = new AuthService(authenticationServiceMock.Object, loggerMock.Object, cancellationTokenProviderMock.Object, new CliHttpClientFactory(httpClientFactoryMock.Object, cancellationTokenProviderMock.Object), remoteServiceExceptionHandlerMock.Object, jsonSerializerMock.Object);
        var cliHttpClientFactory = new CliHttpClientFactory(httpClientFactoryMock.Object, cancellationTokenProviderMock.Object);
        var suiteAppSettingsService = new SuiteAppSettingsService(cmdHelperMock.Object);

        var suiteCommand = new SuiteCommand(
            abpNuGetIndexUrlService,
            packageVersionCheckerService,
            cmdHelper,
            authService,
            cliHttpClientFactory,
            suiteAppSettingsService
        );
        suiteCommand.Logger = loggerMock.Object;

        // Act
        suiteCommand.KillSuite();

        // Assert
        loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
    }
}
