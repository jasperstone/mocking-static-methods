using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Tests
{
    public class SuiteCommandTests
    {
        private readonly Mock<ILogger<SuiteCommand>> _loggerMock;
        private readonly Mock<AbpNuGetIndexUrlService> _nuGetIndexUrlServiceMock;
        private readonly Mock<PackageVersionCheckerService> _packageVersionCheckerServiceMock;
        private readonly Mock<ICmdHelper> _cmdHelperMock;
        private readonly Mock<AuthService> _authServiceMock;
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<SuiteAppSettingsService> _suiteAppSettingsServiceMock;
        private readonly SuiteCommand _suiteCommand;

        public SuiteCommandTests()
        {
            _loggerMock = new Mock<ILogger<SuiteCommand>>();
            _nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>();
            _packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
            _cmdHelperMock = new Mock<ICmdHelper>();
            _authServiceMock = new Mock<AuthService>();
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            _suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();

            _suiteCommand = new SuiteCommand(
                _nuGetIndexUrlServiceMock.Object,
                _packageVersionCheckerServiceMock.Object,
                _cmdHelperMock.Object,
                _authServiceMock.Object,
                _cliHttpClientFactoryMock.Object,
                _suiteAppSettingsServiceMock.Object
            )
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task LogInformation_IsCalledOnSuccessfulInstall()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs
            {
                Target = "",
                Options = new Dictionary<string, string>()
            };

            var mockClient = new Mock<HttpClient>();
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("")
            };

            _cliHttpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<bool>())).Returns(mockClient.Object);
            mockClient.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync(responseMessage);
            mockClient.Setup(c => c.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>())).ReturnsAsync(responseMessage);
            mockClient.Setup(c => c.GetHttpResponseMessageWithRetryAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<ILogger>(),
                It.IsAny<TimeSpan[]>()
            )).ReturnsAsync(responseMessage);

            // Act
            await _suiteCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ABP Suite has been successfully installed.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task LogInformation_IsCalledOnManualInstallCommand()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs
            {
                Target = "install",
                Options = new Dictionary<string, string>()
            };

            var mockClient = new Mock<HttpClient>();
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("")
            };

            _cliHttpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<bool>())).Returns(mockClient.Object);
            mockClient.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync(responseMessage);
            mockClient.Setup(c => c.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>())).ReturnsAsync(responseMessage);
            mockClient.Setup(c => c.GetHttpResponseMessageWithRetryAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<ILogger>(),
                It.IsAny<TimeSpan[]>()
            )).ReturnsAsync(responseMessage);

            // Act
            await _suiteCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("You can also run the following command to install ABP Suite.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task LogError_IsCalledWhenResponseIsNotWhiteSpace()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs
            {
                Target = "generate",
                Options = new Dictionary<string, string>()
            };

            var mockClient = new Mock<HttpClient>();
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("Error response")
            };

            _cliHttpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<bool>())).Returns(mockClient.Object);
            mockClient.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync(responseMessage);
            mockClient.Setup(c => c.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>())).ReturnsAsync(responseMessage);
            mockClient.Setup(c => c.GetHttpResponseMessageWithRetryAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<ILogger>(),
                It.IsAny<TimeSpan[]>()
            )).ReturnsAsync(responseMessage);

            // Act
            await _suiteCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error response")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
