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
        public async Task LogInformation_IsCalledOnGenerate_WhenResponseIsEmpty()
        {
            // Arrange
            var mockClient = new Mock<HttpClient>();
            var solutionId = Guid.NewGuid();
            var options = new CommandLineArgs.OptionsDictionary
            {
                { Options.Crud.Entity.Short, "entity.json" },
                { Options.Crud.Solution.Short, "solution.sln" }
            };
            var args = new CommandLineArgs
            {
                Options = options
            };

            // Setup file existence
            var tempFilePath = Path.GetTempFileName();
            File.WriteAllText(tempFilePath, "{}");
            options[Options.Crud.Entity.Short] = tempFilePath;
            options[Options.Crud.Solution.Short] = Path.ChangeExtension(tempFilePath, ".sln");

            // Mock File.Exists
            var fileExistsMethod = typeof(File).GetMethod("Exists");
            // Can't mock static methods directly, so assume files exist

            // Mock CreateClient to return our mock client
            _cliHttpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<bool>())).Returns(mockClient.Object);

            // Mock GetSolutionIdAsync to return solutionId
            var suiteCommand = new SuiteCommand(
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

            // Mock the HttpClient responses
            var responseContent = new StringContent(""); // empty response
            var mockResponse = new HttpResponseMessage
            {
                Content = responseContent
            };
            mockClient.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync(mockResponse);
            mockClient.Setup(c => c.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>())).ReturnsAsync(new HttpResponseMessage
            {
                Content = new StringContent("")
            });

            // Act
            await suiteCommand.GenerateCrudPageAsync(args);

            // Assert
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Generating CRUD Page...")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
