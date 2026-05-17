using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;

namespace Volo.Abp.Cli.Tests
{
    public class SuiteCommandTests
    {
        private readonly Mock<ILogger<SuiteCommand>> _loggerMock;
        private readonly Mock<ICmdHelper> _cmdHelperMock;
        private readonly Mock<SuiteAppSettingsService> _suiteAppSettingsServiceMock;
        private readonly Mock<AuthService> _authServiceMock;
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;

        public SuiteCommandTests()
        {
            _loggerMock = new Mock<ILogger<SuiteCommand>>();
            _cmdHelperMock = new Mock<ICmdHelper>();
            _suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();
            _authServiceMock = new Mock<AuthService>();
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        }

        [Fact]
        public void StartSuite_PortInUse_LogsError()
        {
            // Arrange
            var suiteCommand = new SuiteCommand(
                null, null, _cmdHelperMock.Object, null, _cliHttpClientFactoryMock.Object, null)
            {
                Logger = _loggerMock.Object
            };
            // Set private field via reflection
            var field = typeof(SuiteCommand).GetField("_abpSuitePort", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(suiteCommand, 1234);

            // Use reflection to invoke StartSuite and simulate port in use
            var method = typeof(SuiteCommand).GetMethod("StartSuite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method.Invoke(suiteCommand, null);

            // Verify that LogError was called with the expected message
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Port \"1234\" is already in use.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
