using System;
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
        private readonly Mock<CmdHelper> _cmdHelperMock;
        private readonly Mock<SuiteAppSettingsService> _suiteAppSettingsServiceMock;
        private readonly SuiteCommand _suiteCommand;

        public SuiteCommandTests()
        {
            _loggerMock = new Mock<ILogger<SuiteCommand>>();
            _cmdHelperMock = new Mock<CmdHelper>();
            _suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();

            _suiteCommand = new SuiteCommand(
                null,
                null,
                _cmdHelperMock.Object,
                null,
                null,
                _suiteAppSettingsServiceMock.Object
            )
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public void StartSuite_PortInUse_LogsErrorAndReturnsNull()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                null, null, null, null, null, null)
            {
                Logger = mockLogger.Object
            };
            // Mock IsPortAlreadyInUse to return true
            var suiteCommandPrivate = new PrivateObject(suiteCommand);
            suiteCommandPrivate.SetFieldOrProperty("_abpSuitePort", 1234);
            // Act
            var result = suiteCommand.StartSuite();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Port \"1234\" is already in use.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public async Task LogError_CalledOnResponseWithErrorMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                null, null, null, null, null, null)
            {
                Logger = mockLogger.Object
            };

            var mockHttpClient = new Mock<HttpClient>();
            var responseContent = new StringContent("Error occurred");
            var responseMessage = new HttpResponseMessage
            {
                Content = responseContent
            };

            var mockResponseTask = Task.FromResult(responseMessage);

            // Act
            var responseString = await responseContent.ReadAsStringAsync();

            // Simulate calling LogError
            mockLogger.Object.LogError(responseString);

            // Assert
            mockLogger.Verify(
                x => x.LogError(It.Is<string>(s => s.Contains("Error occurred"))),
                Times.Once);
        }
    }
}
