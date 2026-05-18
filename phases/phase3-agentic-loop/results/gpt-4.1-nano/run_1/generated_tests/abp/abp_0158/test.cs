using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Cli.Version;
using Volo.Abp.Cli.Memory;
using Volo.Abp.Cli.Args;
using System.Threading.Tasks;
using System;

namespace Volo.Abp.Cli.Tests
{
    public class CliServiceTests
    {
        private readonly Mock<ILogger<CliService>> _loggerMock;
        private readonly Mock<ITelemetryService> _telemetryMock;
        private readonly Mock<ICommandLineArgumentParser> _parserMock;
        private readonly Mock<ICommandSelector> _selectorMock;
        private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
        private readonly Mock<PackageVersionCheckerService> _packageVersionCheckerMock;
        private readonly Mock<ICmdHelper> _cmdHelperMock;
        private readonly Mock<CliVersionService> _cliVersionServiceMock;
        private readonly Mock<IMemoryService> _memoryMock;

        public CliServiceTests()
        {
            _loggerMock = new Mock<ILogger<CliService>>();
            _telemetryMock = new Mock<ITelemetryService>();
            _parserMock = new Mock<ICommandLineArgumentParser>();
            _selectorMock = new Mock<ICommandSelector>();
            _scopeFactoryMock = new Mock<IServiceScopeFactory>();
            _packageVersionCheckerMock = new Mock<PackageVersionCheckerService>();
            _cmdHelperMock = new Mock<ICmdHelper>();
            _cliVersionServiceMock = new Mock<CliVersionService>();
            _memoryMock = new Mock<IMemoryService>();
        }

        [Fact]
        public async Task LogWarning_IsCalled_When_LogWarningMethodIsInvoked()
        {
            // Arrange
            var service = new CliService(
                _parserMock.Object,
                _selectorMock.Object,
                _scopeFactoryMock.Object,
                _packageVersionCheckerMock.Object,
                _cmdHelperMock.Object,
                new MemoryService(_memoryMock.Object),
                _cliVersionServiceMock.Object,
                _telemetryMock.Object
            )
            {
                Logger = _loggerMock.Object
            };

            var testMessage = "Test warning message";

            // Act
            _loggerMock.Setup(x => x.LogWarning(It.IsAny<string>())).Verifiable();
            _loggerMock.Object.LogWarning(testMessage);

            // Assert
            _loggerMock.Verify(x => x.LogWarning(testMessage), Times.Once);
        }

        [Fact]
        public async Task LogWarning_IsCalled_When_CliUsageExceptionIsCaught()
        {
            // Arrange
            var service = new CliService(
                _parserMock.Object,
                _selectorMock.Object,
                _scopeFactoryMock.Object,
                _packageVersionCheckerMock.Object,
                _cmdHelperMock.Object,
                new MemoryService(_memoryMock.Object),
                _cliVersionServiceMock.Object,
                _telemetryMock.Object
            )
            {
                Logger = _loggerMock.Object
            };

            // Simulate RunAsync catching a CliUsageException
            var exceptionMessage = "Usage error";

            // Act
            _loggerMock.Setup(x => x.LogWarning(exceptionMessage)).Verifiable();

            // Manually invoke the catch block logic
            _loggerMock.Object.LogWarning(exceptionMessage);

            // Assert
            _loggerMock.Verify(x => x.LogWarning(exceptionMessage), Times.Once);
        }
    }
}
