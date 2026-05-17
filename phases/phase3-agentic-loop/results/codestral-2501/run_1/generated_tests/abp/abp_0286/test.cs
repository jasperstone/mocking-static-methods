using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class SuiteCommandTests
    {
        private readonly Mock<ILogger<SuiteCommand>> _loggerMock;
        private readonly Mock<ICmdHelper> _cmdHelperMock;
        private readonly Mock<SuiteAppSettingsService> _suiteAppSettingsServiceMock;
        private readonly SuiteCommand _suiteCommand;

        public SuiteCommandTests()
        {
            _loggerMock = new Mock<ILogger<SuiteCommand>>();
            _cmdHelperMock = new Mock<ICmdHelper>();
            _suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>(_cmdHelperMock.Object);
            _suiteCommand = new SuiteCommand(
                new AbpNuGetIndexUrlService(new Mock<IApiKeyService>().Object),
                new PackageVersionCheckerService(
                    new Mock<IJsonSerializer>().Object,
                    new Mock<IRemoteServiceExceptionHandler>().Object,
                    new Mock<ICancellationTokenProvider>().Object,
                    new Mock<IApiKeyService>().Object,
                    new CliHttpClientFactory(
                        new Mock<IHttpClientFactory>().Object,
                        new Mock<ICancellationTokenProvider>().Object)),
                _cmdHelperMock.Object,
                new AuthService(
                    new Mock<IIdentityModelAuthenticationService>().Object,
                    new Mock<ILogger<AuthService>>().Object,
                    new Mock<ICancellationTokenProvider>().Object,
                    new CliHttpClientFactory(
                        new Mock<IHttpClientFactory>().Object,
                        new Mock<ICancellationTokenProvider>().Object),
                    new RemoteServiceExceptionHandler(
                        new Mock<IJsonSerializer>().Object,
                        new Mock<ICancellationTokenProvider>().Object),
                    new Mock<IJsonSerializer>().Object),
                new CliHttpClientFactory(
                    new Mock<IHttpClientFactory>().Object,
                    new Mock<ICancellationTokenProvider>().Object),
                _suiteAppSettingsServiceMock.Object)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public void StartSuite_WhenPortIsAlreadyInUse_LogsError()
        {
            // Arrange
            var ipGlobalPropertiesMock = new Mock<IPGlobalProperties>();
            ipGlobalPropertiesMock.Setup(x => x.GetActiveTcpListeners()).Returns(new[] { new IPEndPoint(IPAddress.Any, 3000) });
            var suiteCommand = new SuiteCommand(
                new AbpNuGetIndexUrlService(new Mock<IApiKeyService>().Object),
                new PackageVersionCheckerService(
                    new Mock<IJsonSerializer>().Object,
                    new Mock<IRemoteServiceExceptionHandler>().Object,
                    new Mock<ICancellationTokenProvider>().Object,
                    new Mock<IApiKeyService>().Object,
                    new CliHttpClientFactory(
                        new Mock<IHttpClientFactory>().Object,
                        new Mock<ICancellationTokenProvider>().Object)),
                _cmdHelperMock.Object,
                new AuthService(
                    new Mock<IIdentityModelAuthenticationService>().Object,
                    new Mock<ILogger<AuthService>>().Object,
                    new Mock<ICancellationTokenProvider>().Object,
                    new CliHttpClientFactory(
                        new Mock<IHttpClientFactory>().Object,
                        new Mock<ICancellationTokenProvider>().Object),
                    new RemoteServiceExceptionHandler(
                        new Mock<IJsonSerializer>().Object,
                        new Mock<ICancellationTokenProvider>().Object),
                    new Mock<IJsonSerializer>().Object),
                new CliHttpClientFactory(
                    new Mock<IHttpClientFactory>().Object,
                    new Mock<ICancellationTokenProvider>().Object),
                _suiteAppSettingsServiceMock.Object)
            {
                Logger = _loggerMock.Object
            };

            // Act
            suiteCommand.StartSuite();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Port \"3000\" is already in use.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
