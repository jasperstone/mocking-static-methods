using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectBuilding;

namespace Volo.Abp.Cli.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        private readonly Mock<ILogger<AbpIoSourceCodeStore>> _loggerMock;
        private readonly Mock<CliVersionService> _cliVersionServiceMock;
        private readonly AbpIoSourceCodeStore _store;

        public AbpIoSourceCodeStoreTests()
        {
            _loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
            _cliVersionServiceMock = new Mock<CliVersionService>();
            var options = Options.Create(new AbpCliOptions());
            _store = new AbpIoSourceCodeStore(
                options,
                new Mock<IJsonSerializer>().Object,
                new Mock<IRemoteServiceExceptionHandler>().Object,
                new Mock<ICancellationTokenProvider>().Object,
                new Mock<CliHttpClientFactory>().Object,
                _cliVersionServiceMock.Object
            );
            _store.Logger = _loggerMock.Object;
        }

        [Fact]
        public async Task GetAsync_Should_LogWarning_When_LatestVersionIsNull()
        {
            // Arrange
            var store = _store;
            var name = "TestTemplate";
            var type = "Template";
            store.GetType().GetProperty("GetLatestSourceCodeVersionAsync").SetValue(store, (Func<string, string, object, bool, Task<string>>)((n, t, _, __) => Task.FromResult<string>(null)));
            store.GetType().GetProperty("GetLocalTemplates").SetValue(store, (Func<List<TemplateFile>>)(() => new List<TemplateFile> { new TemplateFile(new byte[0], "1.0.0", null, "nuget") }));
            _cliVersionServiceMock.Setup(s => s.GetCurrentCliVersionAsync()).ReturnsAsync(new SemanticVersion(1, 0, 0));
            // Act
            await Assert.ThrowsAsync<CliUsageException>(() => store.GetAsync("TestTemplate", type));
            // Assert
            _loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("The remote service is currently unavailable"))), Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetAsync_Should_LogWarning_ForVersionMismatch()
        {
            // Arrange
            var store = _store;
            var name = "TestTemplate";
            var type = "Template";
            var currentVersion = new SemanticVersion(1, 0, 0);
            var templateVersion = new SemanticVersion(2, 0, 0);
            _cliVersionServiceMock.Setup(s => s.GetCurrentCliVersionAsync()).ReturnsAsync(currentVersion);
            // Act
            await store.GetAsync(name, type, "2.0.0");
            // Assert
            _loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("different than the CLI version"))), Times.AtLeastOnce);
        }
    }
}
